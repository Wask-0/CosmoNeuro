using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.UI;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.UI.MainScene;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.MainScene.Infrastructure
{
    public class MainSceneContextRegistrations
    {
        public static void Process(DIContainer container)
        {
            container.RegisterAsSingle(_ => new SceneUnityObjects()).NonLazy();

            container.RegisterAsSingle(CreateUIRoot).NonLazy();
            container.RegisterAsSingle(CreateMainScenePresentersFactory);
            container.RegisterAsSingle(CreateMainSceneScreenPresenter).NonLazy();
            
            container.RegisterAsSingle(CreateMainScenePopupService);
        }

        private static MainSceneUIRoot CreateUIRoot(DIContainer container)
        {
            MainSceneUIRoot prefab = container
                .Resolve<ResourcesAssetsLoader>().Load<MainSceneUIRoot>("UI/MainScene/MainSceneUIRoot");

            return container.Resolve<SceneUnityObjects>().Own(Object.Instantiate(prefab));
        }
        
        private static MainScenePresentersFactory CreateMainScenePresentersFactory(DIContainer container)
        {
            return new MainScenePresentersFactory(container);
        }
        
        private static MainSceneScreenPresenter CreateMainSceneScreenPresenter(DIContainer container)
        {
            MainSceneUIRoot uiRoot = container.Resolve<MainSceneUIRoot>();

            MainSceneScreenView view = container
                .Resolve<ViewsFactory>()
                .Create<MainSceneScreenView>(ViewIDs.MainSceneScreen, uiRoot.HUDLayer);
            
            MainSceneScreenPresenter presenter = container.
                Resolve<MainScenePresentersFactory>().
                CreateMainMenuScreen(view);
            
            return presenter;
        }
        
        private static MainScenePopupService CreateMainScenePopupService(DIContainer container)
        {
            return new MainScenePopupService(
                container.Resolve<ViewsFactory>(),
                container.Resolve<ProjectPresentersFactory>(),
                container.Resolve<MainSceneUIRoot>());
        }
    }
}