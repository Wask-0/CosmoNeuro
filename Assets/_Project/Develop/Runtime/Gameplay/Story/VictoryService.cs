using System;
using System.Collections;
using _Project.Develop.Runtime.Comics;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Story
{
    public sealed class VictoryService
    {
        private const float EndingDelay = 10f;

        private readonly AppStateService _app;
        private readonly MonologueService _monologues;
        private readonly SceneSwitcherService _switcher;
        private readonly ICoroutinesPerformer _coroutines;
        private readonly HeroLifeService _life;

        public bool IsCompleted { get; private set; }

        public event Action Completed;

        public VictoryService(AppStateService app, MonologueService monologues, SceneSwitcherService switcher,
            ICoroutinesPerformer coroutines, HeroLifeService life)
        {
            _app = app;
            _monologues = monologues;
            _switcher = switcher;
            _coroutines = coroutines;
            _life = life;
        }

        public void Complete()
        {
            if (IsCompleted || _life.IsDead || _app.Current != AppState.Gameplay)
                return;

            IsCompleted = true;
            _app.SetState(AppState.Cutscene);
            _monologues.PlayNow("earth_seen");
            Completed?.Invoke();
            _coroutines.StartPerform(ShowEnding());
        }

        private IEnumerator ShowEnding()
        {
            yield return new WaitForSecondsRealtime(EndingDelay);
            yield return _switcher.ProcessSwitchTo(Scenes.Comic,
                new ComicInputArgs(ComicId.Ending, Scenes.MainScene));
        }
    }
}
