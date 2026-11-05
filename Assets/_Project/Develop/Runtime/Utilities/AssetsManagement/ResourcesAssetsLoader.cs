using UnityEngine;

namespace _Project.Develop.Runtime.Utilities.AssetsManagement
{
    public class ResourcesAssetsLoader
    {
        public T Load<T>(string resourcePath) where T : Object
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new System.ArgumentException("Resources path must not be empty.", nameof(resourcePath));
            
            T asset = Resources.Load<T>(resourcePath);
            
            if (asset == null)
                throw new System.InvalidOperationException($"Missing Resources asset '{resourcePath}' of type {typeof(T).FullName}.");
            return asset;
        }
    }
}
