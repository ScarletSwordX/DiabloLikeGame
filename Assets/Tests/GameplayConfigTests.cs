using Gameplay.Bootstrap;
using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Input;
using Gameplay.Effect;
using Gameplay.EventBus;
using Gameplay.Item;
using Gameplay.Skill;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Tests
{
    public class GameplayConfigTests
    {
        [Test]
        public void T01_ConfigLoad_FireballDefaults()
        {
            var fb = ScriptableObject.CreateInstance<SkillData>();
            fb.SetFireballDefaults();
            Assert.AreEqual(SkillKind.Projectile, fb.Kind);
            Assert.AreEqual(1f, fb.CooldownSeconds);
            Assert.AreEqual(0.35f, fb.PreCastSeconds, 0.01f);
            Assert.AreEqual(0.4f, fb.PostCastSeconds, 0.01f);
            Assert.IsFalse(fb.UsesDurationPhase);
            Assert.IsTrue(fb.UsesPostCastPhase);
            Assert.IsTrue(fb.UsesProjectile);
            Assert.IsTrue(fb.ProjectileSpawnOnAnimationEvent);
            Assert.AreEqual(SkillDeliveryKind.Projectile, fb.Delivery.Kind);
            Assert.IsTrue(fb.IsActiveSkill);
            Assert.AreEqual(12f, fb.ProjectileSettings.FlightSpeed, 0.01f);
            Assert.AreEqual(24f, fb.ProjectileSettings.MaxFlightDistance, 0.01f);
            Assert.IsNull(fb.Area);
            Assert.IsTrue(fb.EffectProfile.HasDamage);
            Assert.AreEqual(10f, fb.EffectProfile.Damage.Amount);
        }

        [Test]
        public void T01b_GameInput_HasEightGameplayActions()
        {
            Assert.AreEqual(7, GameInputActions.ButtonActions.Length);
            Assert.AreEqual("Attack", GameInputActions.Attack);
            Assert.AreEqual("Item3", GameInputActions.Item3);
        }

        [Test]
        public void T02_Cooldown_BlocksSecondCast()
        {
            var cd = new CooldownService();
            cd.StartCooldown(1, "fireball", 2f);
            var q = cd.Query(new CooldownQuery { CasterId = 1, SkillId = "fireball" });
            Assert.IsFalse(q.CanCast);
        }

        [Test]
        public void T03_Damage_ReducesHp()
        {
            var go = new GameObject("dummy");
            go.AddComponent<Gameplay.Character.Presenter.CharacterPresenter>();
            go.AddComponent<Gameplay.Character.View.CharacterView>();
            var entity = go.AddComponent<CharacterEntity>();
            entity.ConfigureAsEnemy(100f, 0f);
            entity.ProcessDamage(new DamageRequest { RawDamage = 10f });
            Assert.AreEqual(90f, entity.CurrentHp, 0.01f);
            entity.SetCurrentHp(50f);
            Assert.AreEqual(50f, entity.CurrentHp, 0.01f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void T04_Pickup_ConsumedOnce()
        {
            GameEventBus.ResetForTests();
            var effectGo = new GameObject("effect");
            var effect = effectGo.AddComponent<EffectSystem>();
            var catalog = ScriptableObject.CreateInstance<StatusCatalog>();
            var status = ScriptableObject.CreateInstance<StatusDefinition>();
            status.SetShieldDefaults();
            catalog.Entries = new[] { status };
            catalog.RebuildIndex();
            effect.Initialize(catalog);
            var itemGo = new GameObject("items");
            var items = itemGo.AddComponent<ItemSystem>();
            var def = ScriptableObject.CreateInstance<ItemDefinition>();
            def.SetHealPotionDefaults();
            items.Initialize(effect, new[] { def });

            var playerGo = new GameObject("player");
            playerGo.AddComponent<Gameplay.Character.Presenter.CharacterPresenter>();
            playerGo.AddComponent<Gameplay.Character.View.CharacterView>();
            var player = playerGo.AddComponent<CharacterEntity>();
            player.ConfigureAsEnemy(100f, 0f);
            player.SetCurrentHp(50f);
            var req = new PickupRequest
            {
                PickerId = player.EntityId,
                ItemInstanceId = 99,
                ItemDefinitionId = "heal_potion"
            };
            Assert.AreEqual(PickupResultStatus.Success, items.TryPickup(req).Status);
            Assert.AreEqual(PickupResultStatus.AlreadyConsumed, items.TryPickup(req).Status);
            Assert.AreEqual(0, items.GetCharges(player.EntityId, "heal_potion"));
            Assert.AreEqual(80f, player.CurrentHp, 0.01f);
            Assert.AreEqual(UseResultStatus.Invalid, items.TryUse(new UseRequest
            {
                UserId = player.EntityId,
                ItemDefinitionId = "heal_potion"
            }).Status);

            Object.DestroyImmediate(effectGo);
            Object.DestroyImmediate(itemGo);
            Object.DestroyImmediate(playerGo);
        }

        [Test]
        public void T04b_OnUsePickup_AddsChargesWithoutImmediateEffect()
        {
            GameEventBus.ResetForTests();
            var effectGo = new GameObject("effect");
            var effect = effectGo.AddComponent<EffectSystem>();
            effect.Initialize(ScriptableObject.CreateInstance<StatusCatalog>());
            var itemGo = new GameObject("items");
            var items = itemGo.AddComponent<ItemSystem>();
            var def = ScriptableObject.CreateInstance<ItemDefinition>();
            def.SetHealPotionDefaults();
            def.Id = "heal_potion_pickup";
            def.TriggerType = ItemTriggerType.OnUse;
            def.Charges = 2;
            items.Initialize(effect, new[] { def });

            var playerGo = new GameObject("player");
            playerGo.AddComponent<Gameplay.Character.Presenter.CharacterPresenter>();
            playerGo.AddComponent<Gameplay.Character.View.CharacterView>();
            var player = playerGo.AddComponent<CharacterEntity>();
            player.ConfigureAsEnemy(100f, 0f);
            player.SetCurrentHp(50f);

            var req = new PickupRequest
            {
                PickerId = player.EntityId,
                ItemInstanceId = 100,
                ItemDefinitionId = "heal_potion_pickup"
            };
            Assert.AreEqual(PickupResultStatus.Success, items.TryPickup(req).Status);
            Assert.AreEqual(50f, player.CurrentHp, 0.01f);
            Assert.AreEqual(2, items.GetCharges(player.EntityId, "heal_potion_pickup"));
            Assert.AreEqual(UseResultStatus.Success, items.TryUse(new UseRequest
            {
                UserId = player.EntityId,
                ItemDefinitionId = "heal_potion_pickup"
            }).Status);
            Assert.AreEqual(80f, player.CurrentHp, 0.01f);
            Assert.AreEqual(1, items.GetCharges(player.EntityId, "heal_potion_pickup"));

            Object.DestroyImmediate(effectGo);
            Object.DestroyImmediate(itemGo);
            Object.DestroyImmediate(playerGo);
        }

        [Test]
        public void T05_SkillCatalog_TryGet()
        {
            var catalog = ScriptableObject.CreateInstance<SkillCatalog>();
            var fb = ScriptableObject.CreateInstance<SkillData>();
            fb.SetFireballDefaults();
            catalog.Entries = new[] { fb };
            catalog.RebuildIndex();
            Assert.IsTrue(catalog.TryGet("fireball", out var resolved));
            Assert.AreEqual(fb, resolved);
            Assert.IsFalse(catalog.TryGet("missing", out _));
        }

        [Test]
        public void T06_SessionConfig_ResolveSkillSlots()
        {
            var catalog = ScriptableObject.CreateInstance<SkillCatalog>();
            var fb = ScriptableObject.CreateInstance<SkillData>();
            fb.SetFireballDefaults();
            var shield = ScriptableObject.CreateInstance<SkillData>();
            shield.SetShieldDefaults();
            catalog.Entries = new[] { fb, shield };
            catalog.RebuildIndex();

            var config = ScriptableObject.CreateInstance<GameplaySessionConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("_skillCatalog").objectReferenceValue = catalog;
            so.FindProperty("_skillSlot0").FindPropertyRelative("SkillId").stringValue = "fireball";
            so.FindProperty("_skillSlot1").FindPropertyRelative("SkillId").stringValue = "shield";
            so.FindProperty("_skillSlot2").FindPropertyRelative("SkillId").stringValue = string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();

            var skills = config.ResolveSkills();
            Assert.AreEqual(3, skills.Length);
            Assert.AreEqual(fb, skills[0]);
            Assert.AreEqual(shield, skills[1]);
            Assert.IsNull(skills[2]);
        }

        [Test]
        public void T07_SessionConfig_InvalidItemId_ReturnsNull()
        {
            var catalog = ScriptableObject.CreateInstance<ItemCatalog>();
            catalog.Entries = System.Array.Empty<ItemDefinition>();
            catalog.RebuildIndex();

            var config = ScriptableObject.CreateInstance<GameplaySessionConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("_itemCatalog").objectReferenceValue = catalog;
            so.FindProperty("_itemSlot0").FindPropertyRelative("ItemId").stringValue = "missing";
            so.ApplyModifiedPropertiesWithoutUndo();

            var items = config.ResolveItems();
            Assert.IsNull(items[0]);
        }

        [Test]
        public void T08_SkillSlotInstance_ReusesUuidWhenSameSkillInSlot()
        {
            var model = new Gameplay.Skill.Model.SkillRuntimeModel();
            var fb = ScriptableObject.CreateInstance<SkillData>();
            fb.SetFireballDefaults();
            model.SetLoadout(new[] { fb, null, null });
            var firstUuid = model.GetSlot(0).InstanceUuid;

            model.SetLoadout(new[] { fb, null, null }, model.Slots);
            Assert.AreEqual(firstUuid, model.GetSlot(0).InstanceUuid);
        }

        [Test]
        public void T09_SkillSlotInstance_NewUuidWhenSkillChanges()
        {
            var model = new Gameplay.Skill.Model.SkillRuntimeModel();
            var fb = ScriptableObject.CreateInstance<SkillData>();
            fb.SetFireballDefaults();
            var shield = ScriptableObject.CreateInstance<SkillData>();
            shield.SetShieldDefaults();
            model.SetLoadout(new[] { fb, null, null });
            var firstUuid = model.GetSlot(0).InstanceUuid;

            model.SetLoadout(new[] { shield, null, null }, model.Slots);
            Assert.AreNotEqual(firstUuid, model.GetSlot(0).InstanceUuid);
        }

        [Test]
        public void T10_DizzyStatus_SetsIsDizzyOnModel()
        {
            var catalog = ScriptableObject.CreateInstance<StatusCatalog>();
            var dizzy = ScriptableObject.CreateInstance<StatusDefinition>();
            dizzy.SetDizzyDefaults();
            catalog.Entries = new[] { dizzy };
            catalog.RebuildIndex();

            var model = new Gameplay.Character.Model.CharacterModel();
            model.SetStatusCatalog(catalog);
            model.Initialize(Faction.Enemy, 100f, 0f);

            model.ApplyBuff(new BuffApplyRequest
            {
                BuffId = "dizzy",
                Duration = 2f,
                Magnitude = 0f
            });

            Assert.IsTrue(model.IsDizzy);
            Assert.IsTrue(model.HasActiveStatus("dizzy"));
            model.ClearBuff("dizzy");
            Assert.IsFalse(model.IsDizzy);
        }

        [Test]
        public void T11_BurnStatus_RegisteredInCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<StatusCatalog>();
            var burn = ScriptableObject.CreateInstance<StatusDefinition>();
            burn.SetBurnDefaults();
            catalog.Entries = new[] { burn };
            catalog.RebuildIndex();

            Assert.IsTrue(catalog.TryGet("burn", out var def));
            Assert.AreEqual(StatusKind.DamageOverTime, def.Kind);
            Assert.AreEqual(1f, def.TickIntervalSeconds);
        }

        [Test]
        public void T12_LightingballEffect_HasDizzyStatus()
        {
            var effect = AssetDatabase.LoadAssetAtPath<SkillEffectData>(
                "Assets/Data/Skills/Effects/LightingballEffect.asset");
            Assert.IsNotNull(effect);
            Assert.IsTrue(effect.Profile.HasAnyStatus);
            Assert.AreEqual("dizzy", effect.Profile.StatusEffects[0].StatusId);
            Assert.AreEqual(2f, effect.Profile.StatusEffects[0].DurationSeconds, 0.01f);
        }

        [Test]
        public void T13_FirePotion_HasBurnStatusWithArea()
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Data/Items/FirePotion.asset");
            Assert.IsNotNull(item);
            Assert.IsTrue(item.EffectProfile.HasAnyStatus);
            Assert.AreEqual("burn", item.EffectProfile.StatusEffects[0].StatusId);
            Assert.AreEqual(5f, item.EffectProfile.StatusEffects[0].Magnitude, 0.01f);
            Assert.AreEqual(6f, item.EffectProfile.StatusEffects[0].DurationSeconds, 0.01f);
            Assert.AreEqual(4f, item.EffectProfile.AreaRadius, 0.01f);
        }

        [Test]
        public void T14_SkillCastClipTiming_FireballClipHasLaunchEvent()
        {
            var fb = AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/Fireball.asset");
            Assert.IsNotNull(fb);
            Assert.IsNotNull(fb.CastClip);

            var timing = SkillCastClipTiming.Resolve(fb);
            Assert.IsTrue(timing.Valid);
            Assert.Greater(timing.LaunchTime, 0f);
            Assert.AreEqual(0.35f, timing.PreTargetSeconds, 0.01f);
            Assert.AreEqual(0.4f, timing.PostTargetSeconds, 0.01f);
            Assert.Greater(timing.PrePlaybackSpeed, 1f);
            Assert.Greater(timing.TotalWallClockSeconds, 0f);
        }

        [Test]
        public void T15_SkillCastClipTiming_ComputesSpeedFromSyntheticClip()
        {
            var clip = new AnimationClip();
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0f, 0f, 1f, 1f));
            clip.AddEvent(new AnimationEvent
            {
                time = 0.5f,
                functionName = SkillCastClipTiming.LaunchProjectileEventName
            });

            var skill = ScriptableObject.CreateInstance<SkillData>();
            skill.Kind = SkillKind.Projectile;
            skill.CastClip = clip;
            skill.PreCastSeconds = 0.25f;
            skill.PostCastSeconds = 0.25f;

            var timing = SkillCastClipTiming.Resolve(skill);
            Assert.IsTrue(timing.Valid);
            Assert.AreEqual(0.5f, timing.LaunchTime, 0.001f);
            Assert.AreEqual(2f, timing.PrePlaybackSpeed, 0.01f);
            Assert.AreEqual(2f, timing.PostPlaybackSpeed, 0.01f);
            Assert.AreEqual(0.5f, timing.TotalWallClockSeconds, 0.01f);
        }

        [Test]
        public void T16_ShieldDefaults_StatusApplyPreCastOnly()
        {
            var shield = ScriptableObject.CreateInstance<SkillData>();
            shield.SetShieldDefaults();
            Assert.AreEqual(SkillKind.StatusApply, shield.Kind);
            Assert.AreEqual(0.5f, shield.PreCastSeconds, 0.01f);
            Assert.IsFalse(shield.UsesDurationPhase);
            Assert.IsFalse(shield.UsesPostCastPhase);
            Assert.AreEqual(0f, shield.EffectivePostCastSeconds);
            Assert.AreEqual(0.5f, shield.GetLogicCastLockSeconds(), 0.01f);
        }

        [Test]
        public void T17_ChanneledSkill_UsesAllThreePhases()
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            skill.Kind = SkillKind.Channeled;
            skill.PreCastSeconds = 0.2f;
            skill.DurationSeconds = 1f;
            skill.PostCastSeconds = 0.3f;
            Assert.IsTrue(skill.UsesDurationPhase);
            Assert.IsTrue(skill.UsesPostCastPhase);
            Assert.AreEqual(1.5f, skill.GetLogicCastLockSeconds(), 0.01f);
        }
    }
}
