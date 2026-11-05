using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Interaction;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.Utilities.Input.Common;

namespace _Project.Develop.Runtime.UI.Gameplay.Interaction
{
    public sealed class InteractionPresenter : IPresenter
    {
        private readonly InteractionState _state;
        private readonly InteractionView _view;
        private readonly InteractionConfig _config;
        private readonly ICommonInput _input;
        public InteractionPresenter(InteractionState state, InteractionView view, InteractionConfig config, ICommonInput input)
        { _state = state; _view = view; _config = config; _input = input; }
        public void Initialize() { _state.Changed += Refresh; _input.ModeChanged += Refresh; Refresh(); }
        private void Refresh()
        {
            if (_state.HasInteraction && _input.CurrentMode == CommonInputMode.Gameplay)
                _view.Show(_state.CurrentTarget, _state.CurrentInfo.ActionText, _state.CurrentInfo.CanExecute,
                    _config.PromptHeight, _state.CurrentInfo.SecondaryActionText, _state.CurrentInfo.PrimaryKey);
            else _view.Hide();
        }
        public void Dispose() { _state.Changed -= Refresh; _input.ModeChanged -= Refresh; }
    }
}
