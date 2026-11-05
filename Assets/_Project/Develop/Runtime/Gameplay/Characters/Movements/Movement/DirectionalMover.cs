using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Movements.Movement
{
    public class DirectionalMover
    {
        private readonly ICharacterMotor _motor;
        private readonly float _maxSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _turnDeceleration;
        private float _direction;
        private float _speedMultiplier = 1f;

        public bool HasMoveInput => _direction != 0f;

        public DirectionalMover(ICharacterMotor motor, float maxSpeed, float acceleration,
            float deceleration, float turnDeceleration)
        {
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
            if (!IsPositive(maxSpeed) || !IsPositive(acceleration) ||
                !IsPositive(deceleration) || !IsPositive(turnDeceleration))
                throw new ArgumentOutOfRangeException(nameof(maxSpeed), "Movement parameters must be finite and positive.");
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            _deceleration = deceleration;
            _turnDeceleration = turnDeceleration;
        }

        public void SetSpeedMultiplier(float multiplier) => _speedMultiplier = Mathf.Max(0f, multiplier);

        public void Move(float direction) => _direction = float.IsNaN(direction) ? 0f : Mathf.Clamp(direction, -1f, 1f);

        public void Update(float deltaTime)
        {
            float velocity = _motor.CurrentVelocity.x;
            float target = _direction * _maxSpeed * _speedMultiplier;
            if (target == 0f)
                velocity = Mathf.MoveTowards(velocity, 0f, _deceleration * deltaTime);
            else if (velocity * target < 0f)
            {
                // Spend the remaining part of the step accelerating after the stop.
                float stopTime = Mathf.Abs(velocity) / _turnDeceleration;
                velocity = stopTime >= deltaTime
                    ? Mathf.MoveTowards(velocity, 0f, _turnDeceleration * deltaTime)
                    : Mathf.MoveTowards(0f, target, _acceleration * (deltaTime - stopTime));
            }
            else
                velocity = Mathf.MoveTowards(velocity, target, _acceleration * deltaTime);
            _motor.SetHorizontalVelocity(velocity);
        }

        private static bool IsPositive(float value) => value > 0f && !float.IsInfinity(value);
    }
}
