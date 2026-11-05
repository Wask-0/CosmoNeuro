using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class MoveInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private float _duration = 1f;

        public async Task ExecuteAsync(
            InteractionContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new InvalidOperationException("MoveInteractionAction requires a target.");

            if (float.IsNaN(_duration) || float.IsInfinity(_duration) || _duration < 0f)
                throw new InvalidOperationException("MoveInteractionAction duration must be finite and non-negative.");

            Vector3 startPosition = _target.position;
            Vector3 endPosition = startPosition + _offset;

            float elapsedTime = 0f;

            while (elapsedTime < _duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_target == null)
                    throw new OperationCanceledException(cancellationToken);

                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsedTime / _duration);

                _target.position = Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress);

                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_target == null)
                throw new OperationCanceledException(cancellationToken);

            _target.position = endPosition;
        }
    }
}
