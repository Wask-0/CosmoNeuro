using System;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Utilities.Controllers;

namespace _Project.Develop.Runtime.Gameplay.GameTime
{
    public sealed class GameTimeController : Controller, IDisposable
    {
        private readonly GameTimeService _gameTime;
        private readonly AppStateService _appState;

        public GameTimeController(
            GameTimeService gameTime,
            AppStateService appState)
        {
            _gameTime = gameTime ??
                        throw new ArgumentNullException(nameof(gameTime));

            _appState = appState ??
                        throw new ArgumentNullException(nameof(appState));
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_appState.Current != AppState.Gameplay)
                return;

            _gameTime.Advance(deltaTime);
        }

        public void Dispose()
        {
            Disable();
        }
    }
}