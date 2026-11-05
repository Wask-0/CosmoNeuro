using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Tents
{
    public class TentFactory
    {
        private const string PrefabPath = "Prefab/Gameplay/Tent";

        private readonly ResourcesAssetsLoader _assets;
        private readonly SceneUnityObjects _sceneObjects;

        public TentFactory(
            ResourcesAssetsLoader assets,
            SceneUnityObjects sceneObjects)
        {
            _assets = assets;
            _sceneObjects = sceneObjects;
        }

        public GameObject Create(Vector3 position)
        {
            GameObject prefab =
                _assets.Load<GameObject>(PrefabPath);

            GameObject tent = Object.Instantiate(
                prefab,
                position,
                Quaternion.identity);

            _sceneObjects.Own(tent.transform);
            
            return tent;
        }
    }
}