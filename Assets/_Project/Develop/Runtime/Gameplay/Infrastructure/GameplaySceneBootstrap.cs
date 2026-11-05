using System;
using System.Collections;
using _Project.Develop.Runtime.Configs.Gameplay.Characters;
using _Project.Develop.Runtime.Infrastructure;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Characters;
using _Project.Develop.Runtime.Gameplay.Characters.Factories;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Audio;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.Gameplay.World;
using _Project.Develop.Runtime.UI.Gameplay.Environment;
using _Project.Develop.Runtime.UI.Gameplay.Resources;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using System.Linq;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.Controllers;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplaySceneBootstrap : SceneBootstrap
    {
        [SerializeField] private CommonInputUIBinding _uiInput;
        [SerializeField] private Transform _spawnPoint;
        
        private DIContainer _container;
        private GameplayInputArgs _inputArgs;
        
        private AppStateService _appState;
        private ICommonInput _input;
        
        private GameTimeController _gameTimeController;
        private RotatingStarfield _starfield;
        private ControllersUpdateService _controllersUpdateService = new();
        private StoryDirector _story;
        
        public override void ProcessRegistrations(DIContainer container, IInputSceneArgs sceneArgs = null)
        {
            if (sceneArgs != null && sceneArgs is not GameplayInputArgs)
                throw new ArgumentException($"Expected {nameof(GameplayInputArgs)}.", nameof(sceneArgs));
            
            _inputArgs = sceneArgs as GameplayInputArgs;
            
            _container = container;
            _appState = _container.Resolve<AppStateService>();
            _input = _container.Resolve<ICommonInput>();

            GameplayContextRegistrations.Process(_container);
        }

        public override IEnumerator Initialize()
        {
            Debug.Log("Инициализация игровой сцены");
            
            _uiInput.Initialize((CommonInputReader)_input);
            
            _gameTimeController = _container.Resolve<GameTimeController>();
            
            CharacterFactory characterFactory = _container.Resolve<CharacterFactory>();
            CharacterControllerFactory controllerFactory = _container.Resolve<CharacterControllerFactory>();
            
            HeroFactory heroFactory = new HeroFactory(
                _controllersUpdateService,
                characterFactory,
                controllerFactory,
                _container.Resolve<SceneUnityObjects>());
            
            CharacterConfig characterConfig = _container.Resolve<ConfigsProviderService>().GetConfig<CharacterConfig>();
            
            GameplayAudioDirector audio = _container.Resolve<GameplayAudioDirector>();
            controllerFactory.HeroJumped = audio.PlayJump;

            var hero = heroFactory.Create(characterConfig, _spawnPoint.position);
            _container.Resolve<HeroContext>().Bind(hero);

            BagView bag = Instantiate(_container.Resolve<ResourcesAssetsLoader>()
                .Load<BagView>("Prefab/Gameplay/HeroBag"));
            _container.Resolve<SceneUnityObjects>().Own(bag);
            _container.Resolve<HeroBag>().Bind(bag, hero);

            foreach (IWorldBindable bindable in FindObjectsByType<MonoBehaviour>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IWorldBindable>())
                bindable.Bind(_container);

            _story = _container.Resolve<StoryDirector>();
            var sceneControllers = new CompositeController(
                _container.Resolve<MonologueService>(),
                _story,
                audio,
                _container.Resolve<VisionController>(),
                _container.Resolve<HudSloshController>(),
                _container.Resolve<DirectionHintController>());
            sceneControllers.Enable();
            _controllersUpdateService.Add(sceneControllers, () => false);

            Camera camera = _container.Resolve<GameplayCameraFactory>().Create(hero.transform);
            if (camera.orthographic)
                _starfield = new RotatingStarfield(camera, hero.transform, _container.Resolve<GameTimeService>());

            yield break;
        }

        public override void Run()
        {
            _appState.SetState(AppState.Gameplay);
            
            _gameTimeController.Enable();

            _story.Begin();

            Debug.Log("Игровая сцена запущена");
        }
        
        private void Update()
        {
            _gameTimeController?.Update(Time.deltaTime);
            _controllersUpdateService?.Update(Time.deltaTime);
        }
        
        private void LateUpdate()
        {
            _starfield?.Refresh();
        }

        private void OnDestroy()
        {
            _starfield?.Dispose();
            _controllersUpdateService.Dispose();
        }
    }
}
