using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-100)]
    public sealed class GameplayCameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new(0f, 1f, -10f);
        [SerializeField, Min(0f)] private float _verticalSmoothTime = 0.22f;

        private Transform _target;
        private float _verticalVelocity;

        public void Bind(Transform target)
        {
            _target = target;
            if (_target != null)
                transform.position = _target.position + _offset;
        }

        private void LateUpdate() => UpdatePosition();

        private void UpdatePosition()
        {
            if (_target == null)
                return;

            Vector3 desired = _target.position + _offset;
            desired.y = Mathf.SmoothDamp(transform.position.y, desired.y, ref _verticalVelocity, _verticalSmoothTime);
            transform.position = desired;
        }
    }
}
