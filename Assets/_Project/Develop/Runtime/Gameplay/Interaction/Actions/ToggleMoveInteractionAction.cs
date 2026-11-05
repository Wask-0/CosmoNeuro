using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class ToggleMoveInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private Transform _target;
        [Tooltip("World-space offset from the initial position to the moved position.")]
        [SerializeField] private Vector3 _offset;
        [SerializeField] private float _duration = 1f;

        private Vector3 _initialPosition;
        private Vector3 _movedPosition;
        private bool _isInitialized;
        private bool _isMoved;

        public async Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new InvalidOperationException("ToggleMoveInteractionAction requires a target.");

            if (!IsFinite(_duration) || _duration < 0f)
                throw new InvalidOperationException("ToggleMoveInteractionAction duration must be finite and non-negative.");

            if (!IsFinite(_offset.x) || !IsFinite(_offset.y) || !IsFinite(_offset.z))
                throw new InvalidOperationException("ToggleMoveInteractionAction offset must be finite.");

            // Capture once: repeated interactions always use the same two positions.
            if (!_isInitialized)
            {
                _initialPosition = _target.position;
                _movedPosition = _initialPosition + _offset;
                _isInitialized = true;
            }

            Vector3 startPosition = _target.position;
            Vector3 endPosition = _isMoved ? _initialPosition : _movedPosition;
            float elapsedTime = 0f;

            while (elapsedTime < _duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_target == null)
                    throw new OperationCanceledException(cancellationToken);

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _duration);
                _target.position = Vector3.Lerp(startPosition, endPosition, progress);

                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new OperationCanceledException(cancellationToken);

            _target.position = endPosition;
            // A cancelled animation retries the same destination on the next interaction.
            _isMoved = !_isMoved;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
