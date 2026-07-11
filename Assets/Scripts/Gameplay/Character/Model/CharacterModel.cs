using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.Data;

namespace Gameplay.Character.Model
{
    /// <summary>
    /// 角色数据与规则（无 Transform / Renderer）。
    /// </summary>
    public class CharacterModel
    {
        static int _nextId = 1;

        readonly List<ActiveStatus> _activeStatuses = new List<ActiveStatus>();
        StatusCatalog _statusCatalog;

        public int EntityId { get; private set; }
        public Faction Faction { get; private set; }
        public float CurrentHp { get; private set; }
        public float MaxHp { get; private set; }
        public float BaseMoveSpeed { get; private set; }
        public float SpeedMultiplier { get; private set; } = 1f;
        public float ShieldReduction { get; private set; }
        public bool IsDizzy { get; private set; }
        public bool IsAlive => CurrentHp > 0f;

        public void SetStatusCatalog(StatusCatalog catalog) => _statusCatalog = catalog;

        public void Initialize(Faction faction, float maxHp, float baseMoveSpeed)
        {
            EntityId = _nextId++;
            Faction = faction;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            BaseMoveSpeed = baseMoveSpeed;
        }

        public void ConfigureAsEnemy(float maxHp, float moveSpeed)
        {
            Faction = Faction.Enemy;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            BaseMoveSpeed = moveSpeed;
        }

        public void ConfigureMovement(float moveSpeed) => BaseMoveSpeed = moveSpeed;

        public void SetCurrentHp(float value) =>
            CurrentHp = UnityEngine.Mathf.Clamp(value, 0f, MaxHp);

        public MoveResult ComputeMove(MoveIntent intent)
        {
            var speed = BaseMoveSpeed * SpeedMultiplier * intent.SpeedMultiplier;
            var displacement = intent.Direction.normalized * (speed * intent.DeltaTime);
            return new MoveResult
            {
                Displacement = displacement,
                ActualSpeed = speed,
                Blocked = false
            };
        }

        public DamageResult ApplyDamage(DamageRequest request)
        {
            var final = request.RawDamage * (1f - ShieldReduction);
            final = UnityEngine.Mathf.Max(0f, final);
            CurrentHp = UnityEngine.Mathf.Max(0f, CurrentHp - final);
            return new DamageResult
            {
                FinalDamage = final,
                AbsorbedByShield = ShieldReduction > 0f && final < request.RawDamage,
                Killed = CurrentHp <= 0f
            };
        }

        public HealResult ApplyHeal(HealRequest request)
        {
            var before = CurrentHp;
            CurrentHp = UnityEngine.Mathf.Min(MaxHp, CurrentHp + request.Amount);
            var actual = CurrentHp - before;
            return new HealResult { ActualHeal = actual, Overheal = request.Amount - actual };
        }

        public BuffApplyResult ApplyBuff(BuffApplyRequest request)
        {
            if (_statusCatalog != null && _statusCatalog.TryGet(request.BuffId, out var statusDef))
                ApplyStatusFromDefinition(statusDef, request.Magnitude, request.Duration);
            else
                ApplyStatusLegacy(request.BuffId, request.Magnitude, request.Duration);

            return new BuffApplyResult { Success = true, InstanceId = EntityId };
        }

        public bool HasActiveStatus(string statusId)
        {
            var now = UnityEngine.Time.time;
            for (var i = 0; i < _activeStatuses.Count; i++)
            {
                var status = _activeStatuses[i];
                if (status.StatusId == statusId && status.EndTime > now)
                    return true;
            }

            return false;
        }

        public bool TryGetActiveStatus(string statusId, out ActiveStatus status)
        {
            var now = UnityEngine.Time.time;
            for (var i = 0; i < _activeStatuses.Count; i++)
            {
                var entry = _activeStatuses[i];
                if (entry.StatusId == statusId && entry.EndTime > now)
                {
                    status = entry;
                    return true;
                }
            }

            status = default;
            return false;
        }

        public bool TryGetStatusDefinition(string statusId, out StatusDefinition definition)
        {
            definition = null;
            return _statusCatalog != null && _statusCatalog.TryGet(statusId, out definition);
        }

        void ApplyStatusFromDefinition(StatusDefinition def, float magnitude, float duration)
        {
            _activeStatuses.RemoveAll(s => s.StatusId == def.Id);

            switch (def.Kind)
            {
                case StatusKind.DamageReduction:
                    ShieldReduction = magnitude;
                    break;
                case StatusKind.MoveSpeedMultiplier:
                    SpeedMultiplier = magnitude;
                    break;
                case StatusKind.Dizzy:
                    IsDizzy = true;
                    break;
            }

            _activeStatuses.Add(new ActiveStatus
            {
                StatusId = def.Id,
                Kind = def.Kind,
                EndTime = UnityEngine.Time.time + duration,
                Magnitude = magnitude
            });
        }

        void ApplyStatusLegacy(string buffId, float magnitude, float duration)
        {
            _activeStatuses.RemoveAll(s => s.StatusId == buffId);

            if (buffId == "shield")
                ShieldReduction = magnitude;
            else if (buffId == "slow" || buffId == "speed_boost")
                SpeedMultiplier = magnitude;

            _activeStatuses.Add(new ActiveStatus
            {
                StatusId = buffId,
                Kind = buffId == "shield" ? StatusKind.DamageReduction : StatusKind.MoveSpeedMultiplier,
                EndTime = UnityEngine.Time.time + duration,
                Magnitude = magnitude
            });
        }

        public void ClearBuff(string statusId)
        {
            if (_statusCatalog != null && _statusCatalog.TryGet(statusId, out var def))
                ClearStatusByKind(def.Kind);
            else if (statusId == "shield")
                ShieldReduction = 0f;
            else if (statusId == "slow" || statusId == "speed_boost")
                SpeedMultiplier = 1f;

            _activeStatuses.RemoveAll(s => s.StatusId == statusId);
        }

        void ClearStatusByKind(StatusKind kind)
        {
            switch (kind)
            {
                case StatusKind.DamageReduction:
                    ShieldReduction = 0f;
                    break;
                case StatusKind.MoveSpeedMultiplier:
                    SpeedMultiplier = 1f;
                    break;
                case StatusKind.Dizzy:
                    IsDizzy = false;
                    break;
            }
        }

        public struct ActiveStatus
        {
            public string StatusId;
            public StatusKind Kind;
            public float EndTime;
            public float Magnitude;
        }
    }
}
