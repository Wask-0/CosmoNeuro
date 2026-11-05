using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Controllers.Movement
{
    public class SideScrollerMovementController : Controller
    {
        private const float CoyoteTime = 0.12f;
        private const float JumpBufferTime = 0.12f;

        private readonly Character _character;
        private readonly ICharacterInput _input;
        private readonly DialogueState _dialogue;
        private readonly GameTimeService _time;
        private readonly HeroMobilityService _mobility;
        private readonly TentService _tents;
        private float _sinceGrounded;
        private float _sinceJumpPressed = float.MaxValue;

        public event Action Jumped;

        public SideScrollerMovementController(Character character, ICharacterInput input,
            DialogueState dialogue, GameTimeService time, HeroMobilityService mobility, TentService tents)
        {
            _character = character;
            _input = input;
            _dialogue = dialogue;
            _time = time;
            _mobility = mobility;
            _tents = tents;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_character == null || _character.IsScripted)
                return;

            _character.SetSpeedMultiplier(_mobility.SpeedMultiplier);

            bool blocked = _time.IsFastForwarding || _dialogue.BlocksGameplay;
            float move = blocked ? 0f : _input.Move;
            _character.SetMoveDirection(ApplyTentLeash(move));

            _sinceGrounded = _character.IsGrounded ? 0f : _sinceGrounded + deltaTime;
            _sinceJumpPressed = !blocked && _input.JumpPressed ? 0f : _sinceJumpPressed + deltaTime;

            if (_sinceJumpPressed > JumpBufferTime || _sinceGrounded > CoyoteTime)
                return;

            _sinceJumpPressed = float.MaxValue;
            _sinceGrounded = float.MaxValue;
            _character.Jump(_mobility.JumpHeight);
            Jumped?.Invoke();
        }

        // Стоящую палатку нельзя бросить: без неё следующая опасная фаза была бы неизбежной смертью.
        private float ApplyTentLeash(float move)
        {
            if (move == 0f || !_tents.IsBeyondLeash(_character.Position, Mathf.Sign(move)))
                return move;
            return 0f;
        }

        protected override void OnDisable()
        {
            if (_character != null) _character.SetMoveDirection(0f);
        }
    }
}
