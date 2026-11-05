using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    public sealed class VisionController : Controller
    {
        private const float OpenRadius = 1600f;
        private const float TunnelRadius = 150f;
        private const float RadiusSpeed = 3f;

        private readonly HeroEnvironment _environment;
        private readonly HeroContext _hero;
        private readonly VisionHoleGraphic _graphic;
        private float _radius = OpenRadius;
        private Camera _camera;

        public VisionController(HeroEnvironment environment, HeroContext hero, VisionHoleGraphic graphic)
        {
            _environment = environment;
            _hero = hero;
            _graphic = graphic;
            _graphic.enabled = false;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_graphic == null)
                return;

            float target = _environment.InTunnel ? TunnelRadius : OpenRadius;
            _radius = Mathf.Lerp(_radius, target, 1f - Mathf.Exp(-RadiusSpeed * deltaTime));

            bool visible = _radius < OpenRadius - 40f;
            if (_graphic.enabled != visible) _graphic.enabled = visible;
            if (!visible || _hero.Character == null) return;

            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            Vector3 screen = _camera.WorldToScreenPoint(_hero.Character.Position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _graphic.rectTransform, screen, null, out Vector2 local);

            // Лёгкое дрожание: электроника костюма работает на пределе.
            float flicker = Mathf.PerlinNoise(Time.time * 3f, 0f) * 14f;
            _graphic.SetHole(local, _radius + flicker);
        }
    }
}
