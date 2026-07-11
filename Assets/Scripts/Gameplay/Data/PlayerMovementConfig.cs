using UnityEngine;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Gameplay/Player Movement Config")]
    public class PlayerMovementConfig : ScriptableObject
    {
        public float MoveSpeed = 6f;
        public float AttackDamage = 8f;
    }
}
