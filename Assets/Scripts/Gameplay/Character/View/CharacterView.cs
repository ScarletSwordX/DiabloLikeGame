using Gameplay.Character.Model;
using UnityEngine;

namespace Gameplay.Character.View
{
    /// <summary>
    /// 角色共用视觉（位移、受击闪色）；动画由 PlayerView / EnemyView 各自管理。
    /// </summary>
    public class CharacterView : MonoBehaviour
    {
        CharacterModel _model;

        [Header("视觉状态（仅 View）")]
        [SerializeField] Color _hitFlashColor = new Color(1f, 0.4f, 0.4f);
        [SerializeField] float _hitFlashDuration = 0.1f;

        Renderer[] _renderers;
        Color[] _defaultColors;
        float _flashTimer;

        public CharacterModel Model => _model;
        public Transform AimPoint => transform;
        public Vector3 WorldPosition => transform.position;

        public void Bind(CharacterModel model)
        {
            _model = model;
            CacheRenderers();
        }

        void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _defaultColors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].material != null && _renderers[i].material.HasProperty("_BaseColor"))
                    _defaultColors[i] = _renderers[i].material.color;
            }
        }

        public void ApplyDisplacement(Vector3 displacement)
        {
            if (displacement.sqrMagnitude > 0.0001f)
                transform.position += displacement;
        }

        public void PlayHitFlash()
        {
            _flashTimer = _hitFlashDuration;
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                    _renderers[i].material.color = _hitFlashColor;
            }
        }

        void Update()
        {
            if (_flashTimer <= 0f) return;
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
                RestoreColors();
        }

        void RestoreColors()
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                    _renderers[i].material.color = _defaultColors[i];
            }
        }
    }
}
