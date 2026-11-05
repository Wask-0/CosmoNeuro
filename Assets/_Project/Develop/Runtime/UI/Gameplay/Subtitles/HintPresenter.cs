using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Subtitles
{
    public sealed class HintPresenter : IPresenter
    {
        private readonly HintService _hints;
        private readonly HintView _view;

        public HintPresenter(HintService hints, HintView view)
        {
            _hints = hints;
            _view = view;
        }

        public void Initialize()
        {
            _hints.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            string current = _hints.Current;
            if (string.IsNullOrEmpty(current)) _view.Hide();
            else _view.Show(current);
        }

        public void Dispose() => _hints.Changed -= Refresh;
    }
}
