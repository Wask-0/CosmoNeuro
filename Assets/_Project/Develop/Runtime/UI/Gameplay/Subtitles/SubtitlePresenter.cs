using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Subtitles
{
    public sealed class SubtitlePresenter : IPresenter
    {
        private readonly MonologueService _monologues;
        private readonly SubtitleView _view;

        public SubtitlePresenter(MonologueService monologues, SubtitleView view)
        {
            _monologues = monologues;
            _view = view;
        }

        public void Initialize()
        {
            _monologues.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            if (_monologues.IsPlaying) _view.Show(_monologues.CurrentText);
            else _view.Hide();
        }

        public void Dispose() => _monologues.Changed -= Refresh;
    }
}
