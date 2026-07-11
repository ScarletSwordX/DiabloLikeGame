using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.EventBus;
using Gameplay.Item;
using Gameplay.Skill;
using UnityEngine;

namespace Gameplay.DebugTest
{
    public class GameplaySelfTestRunner : MonoBehaviour
    {
        int _passed;
        int _failed;

        [ContextMenu("Run Skill System Self Test")]
        public void RunAll()
        {
            _passed = _failed = 0;
            GameEventBus.ResetForTests();

            T01_ConfigLoad();
            T02_Cooldown();
            T03_Damage();
            T04_PickupConsume();

            Debug.Log($"[SelfTest] 完成: {_passed} 通过, {_failed} 失败");
        }

        void T01_ConfigLoad()
        {
            var fb = ScriptableObject.CreateInstance<SkillData>();
            fb.SetFireballDefaults();
            Assert(fb.CooldownSeconds == 1f && fb.EffectProfile.Damage.Amount == 10f, "T01 配置读取");
        }

        void T02_Cooldown()
        {
            var cd = new CooldownService();
            const int caster = 1;
            const string skill = "fireball";
            cd.StartCooldown(caster, skill, 2f);
            var q1 = cd.Query(new CooldownQuery { CasterId = caster, SkillId = skill });
            var q2 = cd.Query(new CooldownQuery { CasterId = caster, SkillId = skill });
            Assert(!q1.CanCast && !q2.CanCast, "T02 冷却限制");
        }

        void T03_Damage()
        {
            var go = new GameObject("TestDummy");
            var entity = go.AddComponent<CharacterEntity>();
            entity.ConfigureAsEnemy(100f, 0f);
            var res = entity.ProcessDamage(new DamageRequest { RawDamage = 10f, TargetId = entity.EntityId });
            Assert(Mathf.Approximately(res.FinalDamage, 10f) && Mathf.Approximately(entity.CurrentHp, 90f), "T03 伤害生效");
            Destroy(go);
        }

        void T04_PickupConsume()
        {
            var go = new GameObject("ItemTest");
            var effectGo = new GameObject("Effect");
            var effect = effectGo.AddComponent<EffectSystem>();
            var catalog = ScriptableObject.CreateInstance<StatusCatalog>();
            catalog.Entries = System.Array.Empty<StatusDefinition>();
            catalog.RebuildIndex();
            effect.Initialize(catalog);
            var items = go.AddComponent<ItemSystem>();
            var def = ScriptableObject.CreateInstance<ItemDefinition>();
            def.SetHealPotionDefaults();
            items.Initialize(effect, new[] { def });

            var playerGo = new GameObject("Player");
            var player = playerGo.AddComponent<CharacterEntity>();
            var req = new PickupRequest { PickerId = player.EntityId, ItemInstanceId = 42, ItemDefinitionId = "heal_potion" };
            var r1 = items.TryPickup(req);
            var r2 = items.TryPickup(req);
            Assert(r1.Status == PickupResultStatus.Success && r2.Status == PickupResultStatus.AlreadyConsumed, "T04 拾取消耗");

            Destroy(go);
            Destroy(effectGo);
            Destroy(playerGo);
        }

        void Assert(bool condition, string name)
        {
            if (condition)
            {
                _passed++;
                Debug.Log($"[SelfTest] PASS {name}");
            }
            else
            {
                _failed++;
                Debug.LogError($"[SelfTest] FAIL {name}");
            }
        }
    }
}
