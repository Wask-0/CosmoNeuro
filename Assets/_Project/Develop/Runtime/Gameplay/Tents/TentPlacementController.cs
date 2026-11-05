using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Dialogue;

namespace _Project.Develop.Runtime.Gameplay.Tents
{
    public sealed class TentPlacementController : Controller
    {
        private readonly Character _character;
        private readonly Collider2D _bodyCollider;
        private readonly ICharacterInput _input;
        private readonly TentService _tents;
        private readonly DialogueState _dialogue;

        public TentPlacementController(
            Character character,
            ICharacterInput input,
            TentService tents,
            DialogueState dialogue = null)
        {
            _character = character;
            _bodyCollider = character.GetComponent<Collider2D>();
            _input = input;
            _tents = tents;
            _dialogue = dialogue;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_character == null || _character.IsDestroyed)
                return;
            if (_dialogue != null && _dialogue.IsActive)
                return;

            if (_input.CurrentMode != CommonInputMode.Gameplay)
                return;

            if (!_tents.CanPlace || _character.IsScripted || !_character.IsGrounded)
                return;

            Vector3 feet = _character.Position;
            feet.y = _bodyCollider.bounds.min.y;

            if (_input.TryConsumeInteract())
                _tents.Use(feet);
        }
    }
}
