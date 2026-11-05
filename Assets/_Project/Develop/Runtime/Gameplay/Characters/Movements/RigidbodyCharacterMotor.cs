using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Movements
{
    public class RigidbodyCharacterMotor : ICharacterMotor
    {
        private readonly Rigidbody2D _body;
        private readonly ContactFilter2D _groundFilter;

        public Vector2 CurrentVelocity => _body.linearVelocity;
        public bool IsGrounded => _body.simulated && _body.IsTouching(_groundFilter);
        public float GravityMagnitude => Mathf.Abs(Physics2D.gravity.y * _body.gravityScale);

        public RigidbodyCharacterMotor(Rigidbody2D body, float gravityScale)
        {
            _body = body != null ? body : throw new ArgumentNullException(nameof(body));
            _body.bodyType = RigidbodyType2D.Dynamic;
            _body.freezeRotation = true;
            _body.gravityScale = gravityScale;
            _body.linearDamping = 0f;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Опорой считаем только поверхности, на которых можно стоять: стены прыжок не перезаряжают.
            _groundFilter = new ContactFilter2D { useTriggers = false };
            _groundFilter.SetNormalAngle(45f, 135f);
        }

        public void SetHorizontalVelocity(float velocity)
            => _body.linearVelocity = new Vector2(velocity, _body.linearVelocity.y);

        public void SetVerticalVelocity(float velocity)
            => _body.linearVelocity = new Vector2(_body.linearVelocity.x, velocity);

        public void SetSimulated(bool simulated)
        {
            _body.linearVelocity = Vector2.zero;
            _body.simulated = simulated;
        }

        public void Update(float deltaTime) { }
    }
}
