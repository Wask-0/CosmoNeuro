using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Movements
{
    public interface ICharacterMotor
    {
        Vector2 CurrentVelocity { get; }
        bool IsGrounded { get; }
        float GravityMagnitude { get; }
        void SetHorizontalVelocity(float velocity);
        void SetVerticalVelocity(float velocity);
        void SetSimulated(bool simulated);
        void Update(float deltaTime);
    }
}
