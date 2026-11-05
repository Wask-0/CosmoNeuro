using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class ToggleRotationInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private Transform _target;
        [Tooltip("Local rotation offset from the initial closed pose to the open pose.")]
        [SerializeField] private Vector3 _rotationOffset = new Vector3(0f, 90f, 0f);
        [SerializeField] private float _duration = 1f;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isInitialized;
        private bool _isOpen;

        public async Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new InvalidOperationException("ToggleRotationInteractionAction requires a target.");

            if (!IsFinite(_duration) || _duration < 0f)
                throw new InvalidOperationException("ToggleRotationInteractionAction duration must be finite and non-negative.");

            if (!IsFinite(_rotationOffset.x) || !IsFinite(_rotationOffset.y) || !IsFinite(_rotationOffset.z))
                throw new InvalidOperationException("ToggleRotationInteractionAction rotation offset must be finite.");

            // Capture once: repeated interactions always use the same two poses.
            if (!_isInitialized)
            {
                _closedRotation = _target.localRotation;
                _openRotation = _closedRotation * Quaternion.Euler(_rotationOffset);
                _isInitialized = true;
            }

            Quaternion startRotation = _target.localRotation;
            Quaternion endRotation = _isOpen ? _closedRotation : _openRotation;
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
            // A cancelled animation retries the same destination on the next interaction.
            _isOpen = !_isOpen;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
