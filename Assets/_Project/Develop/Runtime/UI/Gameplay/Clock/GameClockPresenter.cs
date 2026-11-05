using System;
using System.Globalization;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Infrastructure.DI;

namespace _Project.Develop.Runtime.UI.Gameplay.Clock
{
    public sealed class GameClockPresenter : IInitializable, IDisposable
    {
        private readonly GameTimeService _gameTime;
        private readonly GameClockView _view;

        private DateTime? _displayedDate;
        private bool _initialized;
        private bool _disposed;

        public GameClockPresenter(
            GameTimeService gameTime,
            GameClockView view)
        {
            _gameTime = gameTime ??
                        throw new ArgumentNullException(nameof(gameTime));

            _view = view != null
                ? view
                : throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(GameClockPresenter));

            if (_initialized)
                return;

            _initialized = true;

            _gameTime.Advanced += OnTimeAdvanced;

            Refresh(_gameTime.Current);
        }

        private void OnTimeAdvanced(
            GameTimeSnapshot snapshot,
            double elapsedGameSeconds)
        {
            Refresh(snapshot);
        }

        private void Refresh(GameTimeSnapshot snapshot)
        {
            _view.SetDayProgress(snapshot.DayProgress);

            DateTime date = snapshot.DateTime.Date;

            if (_displayedDate == date)
                return;

            _displayedDate = date;

            _view.SetDate(date.ToString(
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_initialized)
                _gameTime.Advanced -= OnTimeAdvanced;
        }
    }
}