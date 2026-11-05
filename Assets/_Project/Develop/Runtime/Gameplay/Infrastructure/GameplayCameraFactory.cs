using System;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    public sealed class GameplayCameraFactory
    {
        private const string PrefabPath = "Prefab/Gameplay/GameplayCamera";

        private readonly ResourcesAssetsLoader _assets;
        private readonly SceneUnityObjects _sceneObjects;

        public GameplayCameraFactory(ResourcesAssetsLoader assets, SceneUnityObjects sceneObjects)
        {
            _assets = assets;
            _sceneObjects = sceneObjects;
        }

        public Camera Create(Transform target)
        {
            GameObject prefab = _assets.Load<GameObject>(PrefabPath);
            if (!prefab.TryGetComponent(out GameplayCameraFollow followPrefab))
                throw new InvalidOperationException($"'{PrefabPath}' requires {nameof(GameplayCameraFollow)}.");

            GameplayCameraFollow follow = _sceneObjects.Own(Object.Instantiate(followPrefab));
            follow.Bind(target);
            return follow.GetComponent<Camera>();
        }
    }
}
