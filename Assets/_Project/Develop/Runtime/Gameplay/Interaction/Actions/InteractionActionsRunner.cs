using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public class InteractionActionsRunner
    {
        private readonly IInteractionAction[] _actions;

        public InteractionActionsRunner(IInteractionAction[] actions) => _actions = actions;

        internal static IInteractionAction[] CollectActions(Transform root)
        {
            var actions = new List<IInteractionAction>(root.GetComponentsInChildren<IInteractionAction>(true));
            var referenced = new HashSet<IInteractionAction>();
            foreach (IInteractionAction action in actions)
            {
                if (action is ParallelInteractionAction group)
                    group.CollectReferencedActions(referenced);
            }

            // Explicit group members execute through their group, even if located elsewhere in the hierarchy.
            actions.RemoveAll(referenced.Contains);
            return actions.ToArray();
        }

        public async Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            foreach (IInteractionAction action in _actions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await action.ExecuteAsync(context, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
