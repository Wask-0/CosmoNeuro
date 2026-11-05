using System.Collections;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.Runtime.UI.Gameplay.Death
{
    public sealed class DeathScreenPresenter : IPresenter
    {
        private readonly HeroLifeService _life;
        private readonly DeathScreenView _view;
        private readonly HeroDeathConfig _config;
        private readonly SceneSwitcherService _switcher;
        private readonly ICoroutinesPerformer _coroutines;
        private Coroutine _delay;
        private bool _leaving;
        public DeathScreenPresenter(HeroLifeService life, DeathScreenView view, HeroDeathConfig config,
            SceneSwitcherService switcher, ICoroutinesPerformer coroutines)
        { _life = life; _view = view; _config = config; _switcher = switcher; _coroutines = coroutines; }

        public void Initialize()
        {
            _life.Died += OnDied; _life.Changed += RefreshWarning;
            _view.RestartClicked += Restart; _view.MenuClicked += Menu;
            _view.Hide(); RefreshWarning();
        }
        private void RefreshWarning()
        {
            if (_life.IsDead || !_life.Exposure.IsInDanger) { _view.SetWarning(""); return; }
            double remaining = _life.Exposure.RemainingSeconds;
            int minutes = (int)(remaining / 60d);
            int seconds = (int)(remaining % 60d);
            string cause = _life.Exposure.Cause == DangerCause.Cold ? "Замерзание" : "Перегрев";
            _view.SetWarning($"{cause}: до смерти {minutes:00}:{seconds:00}\nНайдите укрытие — время игровое");
        }
        private void OnDied(DangerCause cause)
        {
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            Sprite image = cause switch
            {
                DangerCause.Cold => _config.ColdImage,
                DangerCause.Heat => _config.HeatImage,
                _ => null
            };
            _view.ShowDeath(cause, image);
            _delay = _coroutines.StartPerform(ShowChoices());
        }
        private IEnumerator ShowChoices()
        {
            if (_config.PopupDelayRealSeconds > 0f)
                yield return new WaitForSecondsRealtime(_config.PopupDelayRealSeconds);
            _view.ShowChoices();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_view.FirstSelected);
            _delay = null;
        }
        private void Restart() => Leave(Scenes.Gameplay);
        private void Menu() => Leave(Scenes.MainScene);
        private void Leave(string scene)
        {
            if (_leaving) return;
            _leaving = true;
            _view.SetButtonsInteractable(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            _coroutines.StartPerform(_switcher.ProcessSwitchTo(scene));
        }
        public void Dispose()
        {
            _life.Died -= OnDied; _life.Changed -= RefreshWarning;
            _view.RestartClicked -= Restart; _view.MenuClicked -= Menu;
            if (_delay != null) _coroutines.StopPerform(_delay);
        }
    }
}
