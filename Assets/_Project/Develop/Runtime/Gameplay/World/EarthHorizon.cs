using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    // Земля медленно поднимается над горизонтом, когда герой приближается к линии видимости.
    public sealed class EarthHorizon : MonoBehaviour, IWorldBindable
    {
        [SerializeField] private float _startX;
        [SerializeField] private float _endX;
        [SerializeField] private float _hiddenY;
        [SerializeField] private float _visibleY;

        private HeroContext _hero;

        public void Bind(DIContainer container) => _hero = container.Resolve<HeroContext>();

        private void LateUpdate()
        {
            if (_hero?.Character == null) return;
            float progress = Mathf.InverseLerp(_startX, _endX, _hero.Character.Position.x);
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(_hiddenY, _visibleY, progress * progress * (3f - 2f * progress));
            transform.position = position;
        }
    }
}
