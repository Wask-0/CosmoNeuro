using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Infrastructure.Scopes
{
    // Register first so owned objects are released after the scope's services.
    public class ScopeUnityObjects : IDisposable
    {
        private readonly List<GameObject> _objects = new();
        public T Own<T>(T component) where T : Component
        {
            _objects.Add(component.gameObject);
            return component;
        }

        public void Dispose()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
                if (_objects[index] != null)
                    Object.Destroy(_objects[index]);
            _objects.Clear();
        }
    }

    public class ProjectUnityObjects : ScopeUnityObjects { }
    public class SceneUnityObjects : ScopeUnityObjects { }
}
