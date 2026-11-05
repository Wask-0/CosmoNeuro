using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    // Зона проверяет положение героя сама: так ей не мешает выключенная физика при подъёме на уступ.
    public abstract class HeroZone : MonoBehaviour, IWorldBindable
    {
        [SerializeField] private Vector2 _size = new(4f, 30f);

        private HeroContext _hero;
        private bool _inside;

        public virtual void Bind(DIContainer container) => _hero = container.Resolve<HeroContext>();

        protected virtual void Update()
        {
            if (_hero?.Character == null)
                return;

            Vector2 offset = _hero.Character.Position - transform.position;
            bool inside = Mathf.Abs(offset.x) <= _size.x * 0.5f && Mathf.Abs(offset.y) <= _size.y * 0.5f;
            if (inside == _inside)
                return;

            _inside = inside;
            if (inside) OnHeroEntered();
            else OnHeroExited();
        }

        protected abstract void OnHeroEntered();
        protected virtual void OnHeroExited() { }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 0.6f, 0.35f);
            Gizmos.DrawCube(transform.position, _size);
        }
    }
}
