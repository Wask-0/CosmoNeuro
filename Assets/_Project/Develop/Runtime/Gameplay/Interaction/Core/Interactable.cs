using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Actions;
using _Project.Develop.Runtime.Gameplay.Interaction.Requirements;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] private InteractionType _interactionType;
        [SerializeField] private Transform _successActions;
        [SerializeField] private Transform _failedActions;
        
        private IInteractionRequirement[] _requirements;
        
        private InteractionActionsRunner _successActionsRunner;
        private InteractionActionsRunner _failedActionsRunner;

        public abstract Transform InteractionPoint { get; }

        // Уступы работают только от кнопки мешка, чтобы E рядом со стеной по-прежнему ставил палатку.
        public virtual bool AcceptsBagButton => false;
        protected virtual string PrimaryKeyLabel => "E";
        protected virtual bool IsAvailableNow(InteractionContext context) => true;
        
        private bool _isEnabled = true;

        private CancellationTokenSource _operation;
        private bool _isBusy;
        private CancellationToken _destroyToken;

        public async Task<InteractionResult> InteractAsync(
            InteractionContext context, CancellationToken cancellationToken)
        {
            if (!_isEnabled || !isActiveAndEnabled || _isBusy)
                return new InteractionResult(InteractionStatus.Unavailable, "Interaction is unavailable or already running.");
            
            using CancellationTokenSource operation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _destroyToken);
            
            CancellationToken token = operation.Token;
            _isBusy = true;
            _operation = operation;
            var destroyAfterSuccess = new System.Collections.Generic.List<GameObject>();
            context = new InteractionContext(context.Interactor, destroyAfterSuccess);
            bool succeeded = false;
            try
            {
                token.ThrowIfCancellationRequested();
                
                foreach (IInteractionRequirement requirement in _requirements)
                {
                    token.ThrowIfCancellationRequested();
                    
                    if (requirement.IsSatisfied(context))
                        continue;

                    if (_failedActionsRunner != null)
                        await _failedActionsRunner.ExecuteAsync(context, token);
                    
                    token.ThrowIfCancellationRequested();

                    return new InteractionResult(false, "Interaction requirements are not satisfied.");
                }

                if (_successActionsRunner != null)
                    await _successActionsRunner.ExecuteAsync(context, token);
                
                token.ThrowIfCancellationRequested();

                succeeded = true;
                return new InteractionResult(true);
            }
            catch (InteractionRejectedException exception)
            {
                return new InteractionResult(InteractionStatus.Rejected, exception.Message);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception exception)
            {
                return new InteractionResult(InteractionStatus.Failed, exception.Message);
            }
            finally
            {
                _operation = null;
                _isBusy = false;
                if (succeeded)
                    foreach (GameObject target in destroyAfterSuccess)
                        if (target != null) Destroy(target);
            }
        }
        
        public void Enable() => _isEnabled = true;

        public void Disable(bool cancelCurrentOperation = true)
        {
            _isEnabled = false;
            if (cancelCurrentOperation) _operation?.Cancel();
        }

        protected virtual void OnDisable() => _operation?.Cancel();
        
        public InteractionInfo GetInteractionInfo(InteractionContext context)
        {
            if (!_isEnabled || !isActiveAndEnabled || _isBusy || !IsAvailableNow(context))
                return InteractionInfo.Unavailable;
            
            bool canExecute = true;
            foreach (IInteractionRequirement requirement in _requirements)
            {
                if (requirement.IsSatisfied(context)) continue;
                canExecute = false;
                break;
            }

            return new InteractionInfo(GetInteractionText(), true, _interactionType, canExecute,
                this is ISecondaryInteractable secondary ? secondary.SecondaryActionText : null,
                PrimaryKeyLabel);
        }
        
        protected virtual void Awake()
        {
            _destroyToken = destroyCancellationToken;
            
            _requirements = GetComponentsInChildren<IInteractionRequirement>(true);

            _successActionsRunner = CreateRunner(_successActions);
            _failedActionsRunner = CreateRunner(_failedActions);
        }
        
        protected abstract string GetInteractionText();
        
        private InteractionActionsRunner CreateRunner(Transform root)
        {
            if (root == null)
                return null;

            IInteractionAction[] actions = InteractionActionsRunner.CollectActions(root);

            return new InteractionActionsRunner(actions);
        }
    }
}
