using System;
using _Project.Develop.Runtime.Configs.Gameplay.Characters;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.Characters.Movements;
using _Project.Develop.Runtime.Gameplay.Characters.Movements.Movement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Gameplay.Characters.Factories
{
    public class CharacterFactory
    {
        public Character Create(CharacterConfig config, Vector3 position)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.Prefab == null) throw new InvalidOperationException("Assign a character prefab.");
            Character character = Object.Instantiate(config.Prefab, position, Quaternion.identity);
            try
            {
                Initialize(character, config);
                return character;
            }
            catch
            {
                Object.Destroy(character.gameObject);
                throw;
            }
        }

        public void Initialize(Character character, CharacterConfig config)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (float.IsNaN(config.GravityScale) || float.IsInfinity(config.GravityScale) || config.GravityScale < 0f)
                throw new ArgumentOutOfRangeException(nameof(config.GravityScale));
            Rigidbody2D body = character.GetComponent<Rigidbody2D>();
            Collider2D collider = character.GetComponent<Collider2D>();
            if (body == null || collider == null || collider.isTrigger)
                throw new InvalidOperationException("Character requires Rigidbody2D and a solid Collider2D.");
            ICharacterMotor motor = new RigidbodyCharacterMotor(body, config.GravityScale);
            character.Initialize(motor, new DirectionalMover(motor, config.MaxSpeed, config.Acceleration,
                config.Deceleration, config.TurnDeceleration));
        }
    }
}
