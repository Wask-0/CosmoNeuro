using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Develop.Runtime.Utilities.SceneManagement
{
    public class SceneLoaderService
    {
        public IEnumerator LoadAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            AsyncOperation wait = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            
            if (wait == null)
                throw new InvalidOperationException($"Could not start loading scene '{sceneName}'.");

            yield return new WaitWhile(() => wait.isDone == false);
        }

        public IEnumerator UnloadAsync(string sceneName)
        {
            AsyncOperation wait = SceneManager.UnloadSceneAsync(sceneName);

            if (wait == null)
                throw new InvalidOperationException($"Could not start unloading scene '{sceneName}'.");

            yield return new WaitWhile(() => wait.isDone == false);
        }
    }
}