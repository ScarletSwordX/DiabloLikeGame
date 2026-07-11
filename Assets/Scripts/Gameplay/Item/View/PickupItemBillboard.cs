using UnityEngine;

namespace Gameplay.Item.View
{
    /// <summary>
    /// 挂于拾取物图标子物体（SpriteRenderer），LateUpdate 面向主摄像机。
    /// </summary>
    public class PickupItemBillboard : MonoBehaviour
    {
        Camera _camera;

        void LateUpdate()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return;

            var toCamera = transform.position - _camera.transform.position;
            if (toCamera.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
        }
    }
}
