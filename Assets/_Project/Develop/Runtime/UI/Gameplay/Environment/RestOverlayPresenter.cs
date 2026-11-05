using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    public sealed class RestOverlayPresenter : IPresenter
    {
        private readonly TentService _tents;
        private readonly GameTimeService _time;
        private readonly RestOverlayView _view;
        private bool _shown;

        public RestOverlayPresenter(TentService tents, GameTimeService time, RestOverlayView view)
        {
            _tents = tents;
            _time = time;
            _view = view;
        }

        public void Initialize()
        {
            _tents.RestStarted += OnRestStarted;
            _time.Advanced += OnAdvanced;
            _view.Hide();
        }

        private void OnRestStarted()
        {
            _shown = true;
            _view.Show("Герой отдыхает в палатке");
        }

        private void OnAdvanced(GameTimeSnapshot snapshot, double seconds)
        {
            if (!_shown || _time.IsFastForwarding) return;
            _shown = false;
            _view.Hide();
        }

        public void Dispose()
        {
            _tents.RestStarted -= OnRestStarted;
            _time.Advanced -= OnAdvanced;
        }
    }
}
