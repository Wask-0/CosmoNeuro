using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Movements.Movement
{
    public interface IDirectionalMover
    {
        Vector2 CurrentVelocity { get; }
        void SetMoveDirection(float direction);
    }
}
