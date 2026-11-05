using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    [DisallowMultipleComponent]
    public sealed class ParallelInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField, Tooltip("Drag components implementing IInteractionAction here. All listed actions run together.")]
        private List<MonoBehaviour> _actions = new();

        public async Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidateActions(new HashSet<IInteractionAction> { this });
            var actions = new IInteractionAction[_actions.Count];
            for (int i = 0; i < actions.Length; i++)
                actions[i] = (IInteractionAction)_actions[i];
            using var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var tasks = new Task[actions.Length];

            for (int i = 0; i < actions.Length; i++)
                tasks[i] = ExecuteChildAsync(actions[i], context, operation);

            await Task.WhenAll(tasks);
            cancellationToken.ThrowIfCancellationRequested();
        }

        internal void CollectReferencedActions(HashSet<IInteractionAction> referenced)
        {
            ValidateActions(new HashSet<IInteractionAction> { this });
            CollectReferences(referenced);
        }

        private void CollectReferences(HashSet<IInteractionAction> referenced)
        {
            foreach (MonoBehaviour component in _actions)
            {
                var action = (IInteractionAction)component;
                if (referenced.Add(action) && action is ParallelInteractionAction group)
                    group.CollectReferences(referenced);
            }
        }

        private void ValidateActions(HashSet<IInteractionAction> visited)
        {
            if (_actions == null)
                throw new InvalidOperationException("ParallelInteractionAction requires an actions list.");

            foreach (MonoBehaviour component in _actions)
            {
                if (component == null || component is not IInteractionAction action)
                    throw new InvalidOperationException("ParallelInteractionAction entries must be components implementing IInteractionAction.");

                if (!visited.Add(action))
                    throw new InvalidOperationException("ParallelInteractionAction cannot contain duplicate actions or cyclic group references.");

                if (action is ParallelInteractionAction group)
                    group.ValidateActions(visited);
            }
        }

        private static async Task ExecuteChildAsync(IInteractionAction action,
            InteractionContext context, CancellationTokenSource operation)
        {
            try
            {
                operation.Token.ThrowIfCancellationRequested();
                await action.ExecuteAsync(context, operation.Token);
            }
            catch
            {
                // Cancel siblings on failure and wait for their cleanup in WhenAll.
                operation.Cancel();
                throw;
            }
        }
    }
}
