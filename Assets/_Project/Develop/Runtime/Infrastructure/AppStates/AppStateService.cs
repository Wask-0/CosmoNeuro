using System;
using _Project.Develop.Runtime.Utilities.Input.Common;
using UnityEngine;

namespace _Project.Develop.Runtime.Infrastructure.AppStates
{
    public class AppStateService
    {
        private readonly ICommonInput _input;

        public AppState Current { get; private set; } =
            AppState.Loading;

        public event Action<AppState> Changed;

        public AppStateService(ICommonInput input)
        {
            _input = input ??
                     throw new ArgumentNullException(nameof(input));
        }

        public void SetState(AppState state)
        {
            CommonInputMode inputMode = state switch
            {
                AppState.Loading => CommonInputMode.Disabled,
                AppState.MainMenu => CommonInputMode.UI,
                AppState.Gameplay => CommonInputMode.Gameplay,
                AppState.Paused => CommonInputMode.UI,
                AppState.Dead => CommonInputMode.UI,
                AppState.Cutscene => CommonInputMode.UI,

                _ => throw new ArgumentOutOfRangeException(nameof(state))
            };

            if (Current == state)
                return;

            if (state == AppState.Paused || state == AppState.Dead)
                Time.timeScale = 0f;
            else if (Current == AppState.Paused || Current == AppState.Dead)
                Time.timeScale = 1f;

            _input.SetMode(inputMode);

            Current = state;

            Changed?.Invoke(state);
        }
    }
}
