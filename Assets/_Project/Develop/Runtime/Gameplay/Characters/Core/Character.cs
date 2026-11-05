using System;
using _Project.Develop.Runtime.Gameplay.Characters.Movements;
using _Project.Develop.Runtime.Gameplay.Characters.Movements.Movement;
using _Project.Develop.Runtime.Utilities.FoundationUnity.Lifecycle;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Core
{
    [DefaultExecutionOrder(100)]
    public class Character : MonoDestroyable, IDirectionalMover
    {
        private ICharacterMotor _motor;
        private DirectionalMover _mover;
        public bool HasMoveInput => _mover?.HasMoveInput ?? false;
        public Vector2 CurrentVelocity => _motor?.CurrentVelocity ?? Vector2.zero;
        public Vector3 Position => transform.position;
        public bool IsGrounded => _motor != null && _motor.IsGrounded;
        public bool IsScripted { get; private set; }
        public float Facing { get; private set; } = 1f;

        public void Initialize(ICharacterMotor motor, DirectionalMover mover)
        {
            if (_motor != null)
                throw new InvalidOperationException("Character is already initialized.");
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
            _mover = mover ?? throw new ArgumentNullException(nameof(mover));
        }

        public void SetMoveDirection(float direction)
        {
            _mover?.Move(direction);
            if (direction != 0f) Facing = Mathf.Sign(direction);
        }

        public void Face(float direction)
        {
            if (direction != 0f) Facing = Mathf.Sign(direction);
        }

        public void SetSpeedMultiplier(float multiplier) => _mover?.SetSpeedMultiplier(multiplier);

        public void Jump(float height)
            => _motor?.SetVerticalVelocity(Mathf.Sqrt(2f * _motor.GravityMagnitude * Mathf.Max(0f, height)));

        // Подъём и спуск по уступу двигают героя напрямую, поэтому физика на это время выключается.
        public void BeginScriptedMotion()
        {
            IsScripted = true;
            SetMoveDirection(0f);
            _motor?.SetSimulated(false);
        }

        public void EndScriptedMotion()
        {
            _motor?.SetSimulated(true);
            IsScripted = false;
        }

        private void FixedUpdate()
        {
            if (_motor == null || IsScripted)
                return;
            _mover.Update(Time.fixedDeltaTime);
            _motor.Update(Time.fixedDeltaTime);
        }

        private void OnDisable() => SetMoveDirection(0f);
    }
}
