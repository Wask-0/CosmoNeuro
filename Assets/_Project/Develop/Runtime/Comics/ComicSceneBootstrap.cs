using System;
using System.Collections;
using _Project.Develop.Runtime.Configs;
using _Project.Develop.Runtime.Infrastructure;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Develop.Runtime.Comics
{
    // Страница комикса: кадры появляются один за другим, по клику или через паузу.
    public sealed class ComicSceneBootstrap : SceneBootstrap
    {
        private const float AutoAdvanceSeconds = 9f;

        [SerializeField] private ComicPanelView[] _panels = Array.Empty<ComicPanelView>();
        [SerializeField] private TMP_Text _hint;

        private ComicInputArgs _args;
        private AppStateService _appState;
        private SceneSwitcherService _switcher;
        private ICoroutinesPerformer _coroutines;
        private ComicDefinition _comic;
        private int _shown;
        private float _timer;
        private bool _running;
        private bool _leaving;

        public override void ProcessRegistrations(DIContainer container, IInputSceneArgs sceneArgs = null)
        {
            _args = sceneArgs as ComicInputArgs ?? new ComicInputArgs(ComicId.Intro, Scenes.Gameplay);
            _appState = container.Resolve<AppStateService>();
            _switcher = container.Resolve<SceneSwitcherService>();
            _coroutines = container.Resolve<ICoroutinesPerformer>();
            _comic = container.Resolve<ConfigsProviderService>().GetConfig<ComicConfig>().Get(_args.Comic);
        }

        public override IEnumerator Initialize()
        {
            foreach (ComicPanelView panel in _panels)
                panel.Hide();
            _hint.text = "[ЛКМ / Пробел] Дальше     [Esc] Пропустить";
            yield break;
        }

        public override void Run()
        {
            _appState.SetState(AppState.Cutscene);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _running = true;
            ShowNext();
        }

        private void Update()
        {
            if (!_running || _leaving)
                return;

            if (WasSkipPressed())
            {
                Leave();
                return;
            }

            _timer -= Time.unscaledDeltaTime;
            if (WasAdvancePressed() || _timer <= 0f)
                ShowNext();
        }

        private void ShowNext()
        {
            int count = Mathf.Min(_comic.Panels.Length, _panels.Length);
            if (_shown >= count)
            {
                Leave();
                return;
            }

            ComicPanel panel = _comic.Panels[_shown];
            _panels[_shown].Show(panel.Image, panel.Caption);
            _shown++;
            _timer = AutoAdvanceSeconds;
        }

        private void Leave()
        {
            _leaving = true;
            _coroutines.StartPerform(_switcher.ProcessSwitchTo(_args.NextScene));
        }

        private static bool WasSkipPressed()
            => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        private static bool WasAdvancePressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            return Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        }
    }
}
