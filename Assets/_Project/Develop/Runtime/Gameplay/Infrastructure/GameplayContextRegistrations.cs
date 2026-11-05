using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Characters.Factories;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.UI;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.UI.Gameplay;
using _Project.Develop.Runtime.UI.Gameplay.Clock;
using _Project.Develop.Runtime.UI.Gameplay.Resources;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.Interaction;
using _Project.Develop.Runtime.UI.Gameplay.Dialogue;
using _Project.Develop.Runtime.UI.Gameplay.Interaction;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.UI.Gameplay.Death;
using _Project.Develop.Runtime.Gameplay.Audio;
using _Project.Develop.Runtime.Gameplay.Dust;
using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.UI.Gameplay.Environment;
using _Project.Develop.Runtime.UI.Gameplay.Morale;
using _Project.Develop.Runtime.UI.Gameplay.Subtitles;
using _Project.Develop.Runtime.Utilities.Audio;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayContextRegistrations
    {
        public static void Process(DIContainer container)
        {
            container.RegisterAsSingle(_ => new SceneUnityObjects()).NonLazy();
            container.RegisterAsSingle(_ => new DialogueState()).NonLazy();
            container.RegisterAsSingle(_ => new InteractionState());
            container.RegisterAsSingle(_ => new HeroContext());
            container.RegisterAsSingle(_ => new HeroInventory());
            container.RegisterAsSingle(_ => new HeroEnvironment());
            container.RegisterAsSingle(_ => new HeroBag());
            container.RegisterAsSingle(_ => new HeroLandingEvents());
            container.RegisterAsSingle(_ => new DustCollectionState());
            container.RegisterAsSingle(_ => new HintService());
            container.RegisterAsSingle(c => c.Resolve<ConfigsProviderService>().GetConfig<HeroMobilityConfig>());
            container.RegisterAsSingle(c => c.Resolve<ConfigsProviderService>().GetConfig<StoryConfig>());
            container.RegisterAsSingle(c => c.Resolve<ConfigsProviderService>().GetConfig<GameplayAudioConfig>());
            container.RegisterAsSingle(c => new MonologueService(c.Resolve<StoryConfig>()));
            container.RegisterAsSingle(c => c.Resolve<ConfigsProviderService>().GetConfig<InteractionConfig>());
            container.RegisterAsSingle(c => new HeroLifeService(
                c.Resolve<GameTimeService>(), c.Resolve<DayScheduleService>(),
                c.Resolve<AppStateService>(), c.Resolve<DialogueState>(), c.Resolve<HeroResources>(),
                c.Resolve<HeroEnvironment>(), c.Resolve<HeroSurvivalConfig>())).NonLazy();
            container.RegisterAsSingle<ICharacterInput>(c => new CharacterInputReader(c.Resolve<ICommonInput>())).NonLazy();
            
            container.RegisterAsSingle(_ => new CharacterFactory());
            container.RegisterAsSingle(c => new GameplayCameraFactory(
                c.Resolve<ResourcesAssetsLoader>(),
                c.Resolve<SceneUnityObjects>()));
            
            container.RegisterAsSingle(c =>
                c.Resolve<ConfigsProviderService>()
                    .GetConfig<HeroSurvivalConfig>());

            container.RegisterAsSingle(c => new HeroResources(
                c.Resolve<HeroSurvivalConfig>(),
                c.Resolve<GameTimeService>(),
                c.Resolve<HeroInventory>())).NonLazy();
            
            container.RegisterAsSingle(c => new TentFactory(
                c.Resolve<ResourcesAssetsLoader>(),
                c.Resolve<SceneUnityObjects>()));

            container.RegisterAsSingle(c => new TentService(
                c.Resolve<TentFactory>(),
                c.Resolve<HeroResources>(),
                c.Resolve<GameTimeService>(),
                c.Resolve<HeroSurvivalConfig>(),
                c.Resolve<DayScheduleService>(),
                c.Resolve<HeroInventory>(),
                c.Resolve<HeroBag>()));

            container.RegisterAsSingle(c => new HeroMoraleService(
                c.Resolve<HeroMobilityConfig>(),
                c.Resolve<GameTimeService>(),
                c.Resolve<TentService>())).NonLazy();

            container.RegisterAsSingle(c => new HeroMobilityService(
                c.Resolve<HeroMobilityConfig>(),
                c.Resolve<HeroResources>(),
                c.Resolve<HeroMoraleService>(),
                c.Resolve<HeroBag>()));

            container.RegisterAsSingle(c => new CharacterControllerFactory(
                c.Resolve<ICharacterInput>(),
                c.Resolve<TentService>(),
                c.Resolve<DialogueState>(),
                c.Resolve<InteractionState>(),
                c.Resolve<InteractionConfig>(),
                c.Resolve<HeroResources>(),
                c.Resolve<GameTimeService>(),
                c.Resolve<HeroMobilityService>(),
                c.Resolve<HeroInventory>(),
                c.Resolve<HeroBag>(),
                c.Resolve<HeroMoraleService>(),
                c.Resolve<HeroLandingEvents>(),
                c.Resolve<HeroSurvivalConfig>(),
                c.Resolve<DayScheduleService>(),
                c.Resolve<DustCollectionState>(),
                c.Resolve<HintService>()));
            
            
            container.RegisterAsSingle(CreateGameTimeService);

            container.RegisterAsSingle(c => new GameTimeController(
                c.Resolve<GameTimeService>(),
                c.Resolve<AppStateService>())).NonLazy();
            container
                .RegisterAsSingle(CreateDayScheduleService)
                .NonLazy();

            container.RegisterAsSingle(CreateUIRoot).NonLazy();
            container.RegisterAsSingle(CreateGameClockPresenter).NonLazy();
            container.RegisterAsSingle(CreateHeroResourcesPresenter).NonLazy();
            container.RegisterAsSingle(CreateInteractionPresenter).NonLazy();
            container.RegisterAsSingle(CreateDialoguePresenter).NonLazy();
            container.RegisterAsSingle(CreateDeathScreenPresenter).NonLazy();
            container.RegisterAsSingle(CreateMoralePresenter).NonLazy();
            container.RegisterAsSingle(CreateSubtitlePresenter).NonLazy();
            container.RegisterAsSingle(CreateHintPresenter).NonLazy();
            container.RegisterAsSingle(CreateScreenTintPresenter).NonLazy();
            container.RegisterAsSingle(CreateRestOverlayPresenter).NonLazy();
            container.RegisterAsSingle(CreateVisionController);
            container.RegisterAsSingle(CreateHudSloshController);
            container.RegisterAsSingle(CreateDirectionHintController);

            container.RegisterAsSingle(c => new VictoryService(
                c.Resolve<AppStateService>(),
                c.Resolve<MonologueService>(),
                c.Resolve<SceneSwitcherService>(),
                c.Resolve<ICoroutinesPerformer>(),
                c.Resolve<HeroLifeService>()));

            container.RegisterAsSingle(c => new StoryDirector(
                c.Resolve<MonologueService>(),
                c.Resolve<StoryConfig>(),
                c.Resolve<HeroInventory>(),
                c.Resolve<HeroResources>(),
                c.Resolve<HeroMoraleService>(),
                c.Resolve<HeroBag>(),
                c.Resolve<HeroLandingEvents>(),
                c.Resolve<DustCollectionState>(),
                c.Resolve<TentService>(),
                c.Resolve<DayScheduleService>(),
                c.Resolve<GameTimeService>(),
                c.Resolve<AppStateService>(),
                c.Resolve<HintService>(),
                c.Resolve<HeroContext>())).NonLazy();

            container.RegisterAsSingle(c => new GameplayAudioDirector(
                c.Resolve<GameplayAudioConfig>(),
                c.Resolve<AudioService>(),
                c.Resolve<SceneUnityObjects>(),
                c.Resolve<HeroContext>(),
                c.Resolve<HeroBag>(),
                c.Resolve<HeroInventory>(),
                c.Resolve<HeroLandingEvents>(),
                c.Resolve<DustCollectionState>(),
                c.Resolve<TentService>(),
                c.Resolve<DayScheduleService>(),
                c.Resolve<MonologueService>(),
                c.Resolve<VictoryService>())).NonLazy();

            container.RegisterAsSingle(CreateGameplayPresentersFactory);

            container.RegisterAsSingle(CreatePopupService);

            container.RegisterAsSingle(CreateGameplayPauseService).NonLazy();
        }
        
        private static T FindInUI<T>(DIContainer container) where T : UnityEngine.Component
        {
            T view = container.Resolve<GameplayUIRoot>().GetComponentInChildren<T>(true);
            if (view == null)
                throw new System.InvalidOperationException($"GameplayUIRoot requires {typeof(T).Name}.");
            return view;
        }

        private static MoralePresenter CreateMoralePresenter(DIContainer container)
            => new MoralePresenter(container.Resolve<HeroMoraleService>(), FindInUI<MoraleView>(container));

        private static SubtitlePresenter CreateSubtitlePresenter(DIContainer container)
            => new SubtitlePresenter(container.Resolve<MonologueService>(), FindInUI<SubtitleView>(container));

        private static HintPresenter CreateHintPresenter(DIContainer container)
            => new HintPresenter(container.Resolve<HintService>(), FindInUI<HintView>(container));

        private static ScreenTintPresenter CreateScreenTintPresenter(DIContainer container)
            => new ScreenTintPresenter(container.Resolve<DayScheduleService>(), FindInUI<ScreenTintView>(container),
                container.Resolve<HeroEnvironment>());

        private static RestOverlayPresenter CreateRestOverlayPresenter(DIContainer container)
            => new RestOverlayPresenter(container.Resolve<TentService>(), container.Resolve<GameTimeService>(),
                FindInUI<RestOverlayView>(container));

        private static VisionController CreateVisionController(DIContainer container)
            => new VisionController(container.Resolve<HeroEnvironment>(), container.Resolve<HeroContext>(),
                FindInUI<VisionHoleGraphic>(container));

        private static HudSloshController CreateHudSloshController(DIContainer container)
            => new HudSloshController(container.Resolve<HeroContext>(), FindInUI<HeroResourcesView>(container));

        private static DirectionHintController CreateDirectionHintController(DIContainer container)
            => new DirectionHintController(container.Resolve<HeroContext>(), container.Resolve<HeroInventory>(),
                FindInUI<DirectionHintView>(container), container.Resolve<TentService>());

        private static DeathScreenPresenter CreateDeathScreenPresenter(DIContainer container)
        {
            var view = container.Resolve<GameplayUIRoot>().GetComponentInChildren<DeathScreenView>(true);
            if (view == null) throw new System.InvalidOperationException("GameplayUIRoot requires DeathScreenView.");
            return new DeathScreenPresenter(container.Resolve<HeroLifeService>(), view,
                container.Resolve<ConfigsProviderService>().GetConfig<HeroDeathConfig>(),
                container.Resolve<SceneSwitcherService>(), container.Resolve<ICoroutinesPerformer>());
        }

        private static InteractionPresenter CreateInteractionPresenter(DIContainer container)
        {
            var view = container.Resolve<GameplayUIRoot>().GetComponentInChildren<InteractionView>(true);
            if (view == null) throw new System.InvalidOperationException("GameplayUIRoot requires InteractionView.");
            return new InteractionPresenter(container.Resolve<InteractionState>(), view,
                container.Resolve<InteractionConfig>(), container.Resolve<ICommonInput>());
        }

        private static DialoguePresenter CreateDialoguePresenter(DIContainer container)
        {
            var view = container.Resolve<GameplayUIRoot>().GetComponentInChildren<DialogueView>(true);
            if (view == null) throw new System.InvalidOperationException("GameplayUIRoot requires DialogueView.");
            return new DialoguePresenter(container.Resolve<DialogueState>(), view);
        }

        private static HeroResourcesPresenter CreateHeroResourcesPresenter(DIContainer container)
        {
            HeroResourcesView view = container.Resolve<GameplayUIRoot>()
                .GetComponentInChildren<HeroResourcesView>(true);
            if (view == null)
                throw new System.InvalidOperationException("GameplayUIRoot requires a HeroResourcesView.");
            return new HeroResourcesPresenter(
                container.Resolve<HeroResources>(), view);
        }

        private static GameClockPresenter CreateGameClockPresenter(DIContainer container)
        {
            GameClockView view = container.Resolve<GameplayUIRoot>()
                .GetComponentInChildren<GameClockView>(true);
            if (view == null)
                throw new System.InvalidOperationException("GameplayUIRoot requires a GameClockView.");
            return new GameClockPresenter(container.Resolve<GameTimeService>(), view);
        }

        private static GameTimeService CreateGameTimeService(DIContainer container)
        {
            GameTimeConfig config = container.Resolve<ConfigsProviderService>().GetConfig<GameTimeConfig>();

            return new GameTimeService(config.CreateStartDateTime(), config.GameSecondsPerRealSecond);
        }
        
        private static DayScheduleService CreateDayScheduleService(
            DIContainer container)
        {
            DayScheduleConfig config = container
                .Resolve<ConfigsProviderService>()
                .GetConfig<DayScheduleConfig>();

            return new DayScheduleService(
                container.Resolve<GameTimeService>(),
                config.Periods);
        }

        private static GameplayPauseService CreateGameplayPauseService(DIContainer container)
        {
            return new GameplayPauseService(
                container.Resolve<ICommonInput>(),
                container.Resolve<AppStateService>(),
                container.Resolve<GameplayPopupService>(),
                container.Resolve<SceneSwitcherService>(),
                container.Resolve<ICoroutinesPerformer>());
        }

        private static GameplayPresentersFactory CreateGameplayPresentersFactory(DIContainer container)
            => new GameplayPresentersFactory(container);

        private static GameplayUIRoot CreateUIRoot(DIContainer container)
        {
            GameplayUIRoot prefab = container.Resolve<ResourcesAssetsLoader>()
                .Load<GameplayUIRoot>("UI/Gameplay/GameplayUIRoot");

            return container
                .Resolve<SceneUnityObjects>()
                .Own(Object.Instantiate(prefab));
        }

        private static GameplayPopupService CreatePopupService(
            DIContainer container)
        {
            return new GameplayPopupService(
                container.Resolve<ViewsFactory>(),
                container.Resolve<ProjectPresentersFactory>(),
                container.Resolve<GameplayPresentersFactory>(),
                container.Resolve<GameplayUIRoot>());
        }
    }
}
