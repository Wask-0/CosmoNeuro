using System;
using System.Threading;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Interaction.Scanning;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction
{
    public sealed class InteractionController : Controller
    {
        private readonly ICharacterInput _input;
        private readonly CircleInteractionScanner _scanner;
        private readonly InteractionContext _context;
        private readonly InteractionState _state;
        private readonly DialogueState _dialogue;
        private readonly GameTimeService _time;
        private CancellationTokenSource _operation;
        private bool _busy;

        public InteractionController(ICharacterInput input, CircleInteractionScanner scanner,
            IInteractor interactor, InteractionState state, DialogueState dialogue, GameTimeService time)
        {
            _input = input;
            _scanner = scanner;
            _context = new InteractionContext(interactor);
            _state = state;
            _dialogue = dialogue;
            _time = time;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_input.CurrentMode != CommonInputMode.Gameplay || _busy || _dialogue.IsActive || _time.IsFastForwarding)
            {
                if (_input.CurrentMode == CommonInputMode.Gameplay && (_busy || _time.IsFastForwarding))
                    _input.TryConsumeInteract();
                _state.ClearInteraction(); return;
            }
            Interactable nearest = _scanner.FindNearest(_context);
            if (nearest == null) { _state.ClearInteraction(); return; }
            _state.SetInteraction(nearest.GetInteractionInfo(_context), nearest.InteractionPoint, nearest);
            if (nearest is ISecondaryInteractable && _input.PackTentPressed)
            { _input.TryConsumeInteract(); Run(nearest, true); }
            else if (nearest.AcceptsBagButton ? _input.BagPressed : _input.TryConsumeInteract()) Run(nearest, false);
        }

        private async void Run(Interactable target, bool secondary)
        {
            _busy = true;
            _state.ClearInteraction();
            var operation = CancellationTokenSource.CreateLinkedTokenSource(target.destroyCancellationToken);
            _operation = operation;
            Transform point = target.InteractionPoint;
            try
            {
                InteractionResult result = secondary
                    ? await ((ISecondaryInteractable)target).InteractSecondaryAsync(_context, operation.Token)
                    : await target.InteractAsync(_context, operation.Token);
                if (IsEnabled)
                {
                    _state.ReportResult(result, point);
                    if (result.Status == InteractionStatus.Failed)
                        Debug.LogError(result.FailedMessage, target);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception exception) { Debug.LogException(exception); }
            finally
            {
                if (ReferenceEquals(_operation, operation)) _operation = null;
                operation.Dispose();
                _busy = false;
            }
        }

        protected override void OnDisable()
        { _operation?.Cancel(); _state.ClearInteraction(); }
    }
}
