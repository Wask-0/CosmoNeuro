using System;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction
{
    public class InteractionState
    {
        public InteractionInfo CurrentInfo { get; private set; }
        
        public Transform CurrentTarget { get; private set; }
        public IInteractable CurrentInteractable { get; private set; }

        public bool HasInteraction => CurrentTarget != null;

        public event Action Changed;
        public event Action<InteractionResult, Transform> Resolved;

        public void ReportResult(InteractionResult result, Transform point) => Resolved?.Invoke(result, point);

        public void SetInteraction(InteractionInfo info, Transform target, IInteractable interactable = null)
        {
            if (ReferenceEquals(CurrentTarget, target)
                && ReferenceEquals(CurrentInteractable, interactable)
                && CurrentInfo.ActionText == info.ActionText
                && CurrentInfo.IsAvailable == info.IsAvailable
                && CurrentInfo.Type == info.Type
                && CurrentInfo.CanExecute == info.CanExecute
                && CurrentInfo.SecondaryActionText == info.SecondaryActionText
                && CurrentInfo.PrimaryKey == info.PrimaryKey)
                return;

            CurrentInfo = info;
            CurrentTarget = target;
            CurrentInteractable = interactable;

            Changed?.Invoke();
        }

        public void ClearInteraction()
        {
            if (ReferenceEquals(CurrentTarget, null) && CurrentInteractable == null)
                return;

            CurrentTarget = null;
            CurrentInteractable = null;
            CurrentInfo = default;
            Changed?.Invoke();
        }
    }
}
