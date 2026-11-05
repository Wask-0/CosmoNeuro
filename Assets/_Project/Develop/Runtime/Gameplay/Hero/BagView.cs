using _Project.Develop.Runtime.Gameplay.Characters.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BagView : MonoBehaviour
    {
        [SerializeField] private Vector2 _backOffset = new(0.42f, 0.28f);

        private SpriteRenderer _renderer;
        private Character _carrier;

        private void Awake() => _renderer = GetComponent<SpriteRenderer>();

        public void Attach(Character hero) => _carrier = hero;
        public void Detach() => _carrier = null;
        public void SetVisible(bool visible) => _renderer.enabled = visible;

        private void LateUpdate()
        {
            if (_carrier == null)
                return;

            float facing = _carrier.Facing;
            _renderer.flipX = facing > 0f;
            transform.position = _carrier.Position + new Vector3(-facing * _backOffset.x, _backOffset.y, 0f);
        }
    }
}
