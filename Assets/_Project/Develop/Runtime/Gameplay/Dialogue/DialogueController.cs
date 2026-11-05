using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Dialogue
{
    public sealed class DialogueController : Controller
    {
        private readonly DialogueState _state;
        private readonly ICharacterInput _input;
        private readonly Transform _hero;
        private readonly InteractionConfig _config;
        public DialogueController(DialogueState state, ICharacterInput input, Transform hero, InteractionConfig config)
        { _state = state; _input = input; _hero = hero; _config = config; }

        protected override void UpdateLogic(float deltaTime)
        {
            if (!_state.IsActive || _input.CurrentMode != CommonInputMode.Gameplay) return;
            if (_state.TracksSource && (_state.Source == null || _hero == null ||
                ((Vector2)(_state.Source.position - _hero.position)).sqrMagnitude >
                _config.DialogueCloseDistance * _config.DialogueCloseDistance))
            { _state.Close(); _input.TryConsumeInteract(); return; }
            if (_state.StartedFrame != UnityEngine.Time.frameCount && _input.TryConsumeInteract())
                _state.Advance();
        }

        protected override void OnDisable() => _state.Close();
    }
}
