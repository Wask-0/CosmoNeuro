using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class RotateObjectInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] private float _duration = 1f;

        public async Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new InvalidOperationException("RotateObjectInteractionAction requires a target.");

            if (float.IsNaN(_duration) || float.IsInfinity(_duration) || _duration < 0f)
                throw new InvalidOperationException("RotateObjectInteractionAction duration must be finite and non-negative.");

            if (!IsFinite(_rotationOffset.x) || !IsFinite(_rotationOffset.y) || !IsFinite(_rotationOffset.z))
                throw new InvalidOperationException("RotateObjectInteractionAction rotation offset must be finite.");

            Quaternion startRotation = _target.localRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(_rotationOffset);
            float elapsedTime = 0f;

            while (elapsedTime < _duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_target == null)
                    throw new OperationCanceledException(cancellationToken);

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _duration);
                _target.localRotation = Quaternion.Slerp(startRotation, endRotation, progress);

                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new OperationCanceledException(cancellationToken);

            _target.localRotation = endRotation;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
