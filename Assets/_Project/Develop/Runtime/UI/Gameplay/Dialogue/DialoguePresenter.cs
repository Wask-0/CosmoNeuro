using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Dialogue
{
    public sealed class DialoguePresenter : IPresenter
    {
        private readonly DialogueState _state;
        private readonly DialogueView _view;
        public DialoguePresenter(DialogueState state, DialogueView view) { _state = state; _view = view; }
        public void Initialize() { _state.Changed += Refresh; Refresh(); }
        private void Refresh()
        {
            if (_state.IsActive) _view.Show(_state.Current.Speaker, _state.Current.Text);
            else _view.Hide();
        }
        public void Dispose() => _state.Changed -= Refresh;
    }
}
