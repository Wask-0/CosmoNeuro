using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public readonly struct InteractionContext
    {
        public IInteractor Interactor { get; }
        public _Project.Develop.Runtime.Gameplay.Dialogue.DialogueState Dialogue => Interactor.Dialogue;
        public _Project.Develop.Runtime.Gameplay.Resources.HeroResources Resources => Interactor.Resources;
        private readonly List<GameObject> _destroyAfterSuccess;

        public InteractionContext(IInteractor interactor)
        {
            Interactor = interactor;
            _destroyAfterSuccess = null;
        }

        internal InteractionContext(IInteractor interactor, List<GameObject> destroyAfterSuccess)
        { Interactor = interactor; _destroyAfterSuccess = destroyAfterSuccess; }

        internal void DestroyAfterSuccess(GameObject target)
        {
            if (_destroyAfterSuccess == null)
                throw new InvalidOperationException("Destruction must run inside an interaction operation.");
            if (target == null) throw new InvalidOperationException("Destruction requires a target.");
            if (!_destroyAfterSuccess.Contains(target)) _destroyAfterSuccess.Add(target);
        }
    }
}
