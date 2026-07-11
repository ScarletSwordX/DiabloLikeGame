using Gameplay.Bootstrap;
using Gameplay.Character.Player.Model;
using Gameplay.Character.Player.View;
using Gameplay.Character.Presenter;
using Gameplay.Character.View;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.EventBus;
using Gameplay.Input;
using Gameplay.Item;
using Gameplay.Skill;
using UnityEngine;

namespace Gameplay.Character.Player.Presenter
{
    [RequireComponent(typeof(CharacterPresenter))]
    [RequireComponent(typeof(PlayerView))]
    public class PlayerPresenter : MonoBehaviour
    {
        readonly PlayerModel _model = new PlayerModel();
        PlayerView _view;
        CharacterPresenter _character;
        [SerializeField] PlayerAttackHitbox _attackHitbox;
        SkillSystem _skillSystem;
        ItemSystem _itemSystem;
        SkillData[] _skillDefinitions;
        ItemDefinition[] _hotbarItems = new ItemDefinition[3];
        PlayerSkillAnimationOverride _skillAnimationOverride;

        public PlayerModel Model => _model;
        public int EntityId => _character.Model.EntityId;

        /// <summary>
        /// 单次动作动画结束（Attack / Skill / Item / Hit / Dizzy）后由 <see cref="View.OnFinish"/> 调用，回到 Idle。
        /// </summary>
        public void NotifyActionAnimationFinished()
        {
            _view?.ResetCastClipSpeedScaling();

            if (_model.State == PlayerActivityState.Dead)
                return;

            switch (_model.State)
            {
                case PlayerActivityState.Attacking:
                case PlayerActivityState.UsingSkill:
                case PlayerActivityState.UsingItem:
                case PlayerActivityState.Hit:
                case PlayerActivityState.Dizzy:
                    _model.State = PlayerActivityState.Idle;
                    break;
            }
        }

        void Awake()
        {
            _view = GetComponent<PlayerView>();
            _character = GetComponent<CharacterPresenter>();
            _skillAnimationOverride = GetComponent<PlayerSkillAnimationOverride>();
            _view.Bind(_model, GetComponent<CharacterView>());
            if (_attackHitbox == null)
                _attackHitbox = GetComponentInChildren<PlayerAttackHitbox>(true);
            _attackHitbox?.Bind(_model, GetComponent<CharacterEntity>());
        }

        bool _inputBound;
        bool _wired;

        public bool IsWired => _wired;

        public void Initialize(
            PlayerMovementConfig config,
            SkillSystem skillSystem,
            SkillData[] skillDefinitions,
            ItemSystem itemSystem = null,
            ItemDefinition[] hotbarItems = null)
        {
            UnbindInput();
            if (config != null)
                _model.AttackDamage = config.AttackDamage;

            _skillSystem = skillSystem;
            _itemSystem = itemSystem;
            ApplyLoadout(skillDefinitions, hotbarItems);
            _wired = skillSystem != null;
            _attackHitbox?.Bind(_model, GetComponent<CharacterEntity>());
            BindInput(force: true);
            GameplayInputLog.Presenter(
                "Initialize",
                $"wired skill={skillSystem != null} item={itemSystem != null} skills={skillDefinitions?.Length ?? 0} hotbar={CountHotbar(hotbarItems)}");
        }

        public void ApplyLoadout(SkillData[] skillDefinitions, ItemDefinition[] hotbarItems)
        {
            _skillDefinitions = NormalizeSkills(skillDefinitions);
            ApplyHotbar(hotbarItems);
            _skillAnimationOverride?.ApplySkillSlots(_skillDefinitions);
        }

        static SkillData[] NormalizeSkills(SkillData[] skills)
        {
            var result = new SkillData[GameplaySessionConfig.SkillSlotCount];
            if (skills == null) return result;
            for (var i = 0; i < result.Length && i < skills.Length; i++)
                result[i] = skills[i];
            return result;
        }

        static int CountHotbar(ItemDefinition[] items)
        {
            if (items == null) return 0;
            var n = 0;
            foreach (var item in items)
                if (item != null) n++;
            return n;
        }

        void ApplyHotbar(ItemDefinition[] hotbarItems)
        {
            _hotbarItems = new ItemDefinition[3];
            if (hotbarItems == null) return;
            for (var i = 0; i < _hotbarItems.Length && i < hotbarItems.Length; i++)
                _hotbarItems[i] = hotbarItems[i];
        }

        void BindInput(bool force = false)
        {
            if (_inputBound) return;
            if (!force && !isActiveAndEnabled) return;
            GameplayInputBus.Attack += TryAttack;
            GameplayInputBus.Skill1 += HandleSkill1;
            GameplayInputBus.Skill2 += HandleSkill2;
            GameplayInputBus.Skill3 += HandleSkill3;
            GameplayInputBus.Item1 += HandleItem1;
            GameplayInputBus.Item2 += HandleItem2;
            GameplayInputBus.Item3 += HandleItem3;
            _inputBound = true;
            GameplayInputLog.Presenter("BindInput", "subscribed Attack/Skill/Item");
        }

        void UnbindInput()
        {
            if (!_inputBound) return;
            GameplayInputBus.Attack -= TryAttack;
            GameplayInputBus.Skill1 -= HandleSkill1;
            GameplayInputBus.Skill2 -= HandleSkill2;
            GameplayInputBus.Skill3 -= HandleSkill3;
            GameplayInputBus.Item1 -= HandleItem1;
            GameplayInputBus.Item2 -= HandleItem2;
            GameplayInputBus.Item3 -= HandleItem3;
            _inputBound = false;
            GameplayInputLog.Presenter("UnbindInput", "unsubscribed");
        }

        void OnEnable()
        {
            GameplayMvpSession.Ready += OnSessionReady;
            BindInput();
            TryWireFromSession();
        }

        void OnDisable()
        {
            GameplayMvpSession.Ready -= OnSessionReady;
            UnbindInput();
        }

        void OnSessionReady() => TryWireFromSession();

        void TryWireFromSession()
        {
            if (_wired || !GameplayMvpSession.IsReady) return;
            if (GameplayMvpSession.Player != null && GameplayMvpSession.Player != GetComponent<PlayerController>())
                return;

            _skillSystem = GameplayMvpSession.SkillSystem;
            _itemSystem = GameplayMvpSession.ItemSystem;
            if (_skillDefinitions == null || _skillDefinitions.Length == 0)
                ApplyLoadout(_skillSystem != null ? _skillSystem.Equipped : null, GameplayMvpSession.HotbarItems);
            else if (GameplayMvpSession.HotbarItems != null)
                ApplyHotbar(GameplayMvpSession.HotbarItems);
            _wired = _skillSystem != null;
            if (_wired)
                GameplayInputLog.Presenter("TryWireFromSession", "skill+item systems resolved");
        }

        void HandleSkill1() => TryCastSkillAtSlot(0);

        void HandleSkill2() => TryCastSkillAtSlot(1);

        void HandleSkill3() => TryCastSkillAtSlot(2);

        void HandleItem1() => TryUseItemFromAction(GameInputActions.Item1);

        void HandleItem2() => TryUseItemFromAction(GameInputActions.Item2);

        void HandleItem3() => TryUseItemFromAction(GameInputActions.Item3);

        void TryAttack()
        {
            GameplayInputLog.Presenter("TryAttack", "received");
            if (_view.IsActionBlocked)
            {
                GameplayInputLog.Presenter("TryAttack", "rejected: action blocked");
                return;
            }

            if (!_view.TryPlayAttack())
            {
                GameplayInputLog.Presenter("TryAttack", "rejected: TryPlayAttack failed");
                return;
            }

            _attackHitbox?.BeginSwing();
            GameplayInputLog.Presenter("TryAttack", "attack started (MeleeSector, layer filter off)");
        }

        void TryUseItemFromAction(string actionName)
        {
            GameplayInputLog.Item(actionName, "received");
            TryWireFromSession();
            if (!_wired || _itemSystem == null)
            {
                GameplayInputLog.Item(actionName,
                    $"rejected: not wired (session={GameplayMvpSession.IsReady} bootstrap={GameplayMvpSession.ItemSystem != null})");
                return;
            }

            if (_view.IsActionBlocked)
            {
                GameplayInputLog.Item(actionName, $"rejected: blocked ({_view.DescribeActionBlockReason()})");
                return;
            }

            var slot = HotbarSlotIndex(actionName);
            if (slot < 0 || slot >= _hotbarItems.Length)
            {
                GameplayInputLog.Item(actionName, "rejected: invalid slot");
                return;
            }

            var def = _hotbarItems[slot];
            if (def == null)
            {
                GameplayInputLog.Item(actionName, $"rejected: empty hotbar slot {slot}");
                return;
            }

            var result = _itemSystem.TryUse(new UseRequest
            {
                UserId = EntityId,
                ItemDefinitionId = def.Id
            });
            if (result.Status == UseResultStatus.Success)
            {
                _view.TryPlayItem();
                GameplayInputLog.Item(actionName, $"success item={def.Id}");
            }
            else
            {
                GameplayInputLog.Item(actionName, $"rejected: {result.Status}");
            }
        }

        static int HotbarSlotIndex(string actionName)
        {
            if (actionName == GameInputActions.Item1) return 0;
            if (actionName == GameInputActions.Item2) return 1;
            if (actionName == GameInputActions.Item3) return 2;
            return -1;
        }

        void TryCastSkillAtSlot(int slot)
        {
            var actionName = SlotToSkillAction(slot);
            GameplayInputLog.Skill(actionName, "received");
            TryWireFromSession();
            if (!_wired || _skillSystem == null)
            {
                GameplayInputLog.Skill(actionName,
                    $"rejected: not wired (session={GameplayMvpSession.IsReady} bootstrap={GameplayMvpSession.SkillSystem != null})");
                return;
            }

            if (_view.IsActionBlocked)
            {
                GameplayInputLog.Skill(actionName, $"rejected: blocked ({_view.DescribeActionBlockReason()})");
                return;
            }

            var slotInstance = _skillSystem.GetSlotInstance(slot);
            if (slotInstance.IsEmpty)
            {
                GameplayInputLog.Skill(actionName, "rejected: empty slot instance");
                return;
            }

            var def = slotInstance.Skill;
            if (def == null || !def.IsActiveSkill)
            {
                GameplayInputLog.Skill(actionName, "rejected: empty or inactive skill slot");
                return;
            }

            var caster = GetComponent<CharacterEntity>();
            var aim = def.UsesProjectile && def.Delivery != null
                ? def.Delivery.ResolveProjectileAimPoint(caster)
                : SkillAreaResolver.ResolveWorldCenter(caster, def.Area);
            var castRequest = new CastRequest
            {
                SkillId = def.Id,
                CooldownInstanceId = slotInstance.InstanceUuid,
                CasterId = EntityId,
                AimPoint = aim,
                TargetId = EntityId
            };
            GameplayInputLog.Skill(actionName,
                def.UsesProjectile
                    ? $"slot={slot} instance={slotInstance.InstanceUuid} skillId={def.Id} projectile speed={def.ProjectileSettings?.FlightSpeed:0.##} range={def.ProjectileSettings?.MaxFlightDistance:0.##} aim={aim}"
                    : $"slot={slot} instance={slotInstance.InstanceUuid} skillId={def.Id} area={def.Area?.Shape} center={aim} radius={def.Area?.ResolveEffectRadius():0.##}");

            var cast = _skillSystem.TryCast(castRequest);
            if (cast.Status == CastResultStatus.Success || cast.Status == CastResultStatus.InProgress)
            {
                _view.BeginCastClipSpeedScaling(def);

                if (def.UsesAnimationEventProjectileLaunch)
                {
                    var effectSystem = GameplayMvpSession.EffectSystem;
                    if (effectSystem != null)
                        _view.PrepareSkillProjectileLaunch(def, castRequest, effectSystem);
                    else
                        GameplayInputLog.Skill(actionName, "warning: EffectSystem missing for animation projectile");
                }

                _view.TryPlaySkillForAction(actionName);
                GameplayInputLog.Skill(actionName, $"success skill={def.Id} status={cast.Status}");
            }
            else
            {
                GameplayInputLog.Skill(actionName, $"rejected: {cast.Status}");
            }
        }

        static string SlotToSkillAction(int slot)
        {
            return slot switch
            {
                0 => GameInputActions.Skill1,
                1 => GameInputActions.Skill2,
                2 => GameInputActions.Skill3,
                _ => "Skill?"
            };
        }
    }
}
