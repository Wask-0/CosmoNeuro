using _Project.Develop.Runtime.Configs.Gameplay.Characters;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.Characters.Factories;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public sealed class HeroFactory
    {
        private readonly CharacterFactory _characterFactory;
        private readonly CharacterControllerFactory _controllerFactory;
        private readonly ControllersUpdateService _controllerUpdates;
        private readonly SceneUnityObjects _sceneObjects;

        public HeroFactory(
            ControllersUpdateService controllerUpdates,
            CharacterFactory characterFactory,
            CharacterControllerFactory controllerFactory,
            SceneUnityObjects sceneObjects)
        {
            _characterFactory = characterFactory;
            _controllerFactory = controllerFactory;
            _controllerUpdates = controllerUpdates;
            _sceneObjects = sceneObjects;
        }

        public Character Create(
            CharacterConfig config,
            Vector3 position)
        {
            Character character =
                _characterFactory.Create(config, position);

            CompositeController controller = null;

            try
            {
                controller = _controllerFactory.Create(character);

                _sceneObjects.Own(character);

                controller.Enable();

                _controllerUpdates.Add(
                    controller,
                    () => character == null || character.IsDestroyed);

                return character;
            }
            catch
            {
                controller?.Disable();
                character.Destroy();
                throw;
            }
        }
    }
}
