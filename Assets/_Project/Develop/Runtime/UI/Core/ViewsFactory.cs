using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.UI.Core
{
    public class ViewsFactory
    {
        private readonly ResourcesAssetsLoader _resourcesAssetsLoader;

        private readonly Dictionary<string, string> _viewIDToResourcesPath = new Dictionary<string, string>()
        {
            {ViewIDs.TestPopup, "UI/TestPopup" },
            {ViewIDs.ConfirmPopup, "UI/Confirm/ConfirmPopup"},
            {ViewIDs.SettingsPopup, "UI/Settings/SettingsPopup"},
            {ViewIDs.PausePopup, "UI/Gameplay/Pause/PausePopup"},
            
            {ViewIDs.MainSceneScreen, "UI/MainScene/MainSceneScreenView" },
        };

        public ViewsFactory(ResourcesAssetsLoader resourcesAssetsLoader)
        {
            _resourcesAssetsLoader = resourcesAssetsLoader;
        }

        public TView Create<TView>(string viewID, Transform parent = null) where TView : MonoBehaviour, IView
        {
            if (_viewIDToResourcesPath.TryGetValue(viewID, out string resourcePath) == false)
                throw new ArgumentException($"You didn;t set reource path for {typeof(TView)}, searched id: {viewID}");

            GameObject prefap = _resourcesAssetsLoader.Load<GameObject>(resourcePath);
            GameObject instance = Object.Instantiate(prefap, parent);
            TView view = instance.GetComponent<TView>();

            if (view == null)
                throw new InvalidOperationException($"Not found {typeof(TView)} component on view instance");

            return view;
        }

        public void Release<TView>(TView view) where TView : MonoBehaviour, IView
        {
            Object.Destroy(view.gameObject);
        }
    }
}