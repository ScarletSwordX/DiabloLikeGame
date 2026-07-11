using Gameplay.Character.Enemy.Presenter;
using UnityEngine;

namespace Gameplay.Character
{
    /// <summary>
    /// 标记训练假人；配置由 EnemyPresenter 序列化字段驱动。
    /// </summary>
    [RequireComponent(typeof(EnemyPresenter))]
    public class TrainingDummyMarker : MonoBehaviour
    {
    }
}
