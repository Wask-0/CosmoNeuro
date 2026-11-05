using _Project.Develop.Runtime.Gameplay.Characters.Controllers.Movement;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.Interaction;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Interaction.Scanning;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Dust;
using _Project.Develop.Runtime.Gameplay.Story;

namespace _Project.Develop.Runtime.Gameplay.Characters.Factories
{
    public class CharacterControllerFactory
    {
        private readonly ICharacterInput _input;
        private readonly TentService _tentService;
        private readonly DialogueState _dialogue;
        private readonly InteractionState _interaction;
        private readonly InteractionConfig _config;
        private readonly HeroResources _resources;
        private readonly GameTimeService _time;
        private readonly HeroMobilityService _mobility;
        private readonly HeroInventory _inventory;
        private readonly HeroBag _bag;
        private readonly HeroMoraleService _morale;
        private readonly HeroLandingEvents _landing;
        private readonly HeroSurvivalConfig _survival;
        private readonly DayScheduleService _schedule;
        private readonly DustCollectionState _dust;
        private readonly HintService _hints;

        public System.Action HeroJumped { get; set; }

        public CharacterControllerFactory(
            ICharacterInput input,
            TentService tentService,
            DialogueState dialogue,
            InteractionState interaction,
            InteractionConfig config,
            HeroResources resources,
            GameTimeService time,
            HeroMobilityService mobility,
            HeroInventory inventory,
            HeroBag bag,
            HeroMoraleService morale,
            HeroLandingEvents landing,
            HeroSurvivalConfig survival,
            DayScheduleService schedule,
            DustCollectionState dust,
            HintService hints)
        {
            _input = input;
            _tentService = tentService;
            _dialogue = dialogue;
            _interaction = interaction;
            _config = config;
            _resources = resources;
            _time = time;
            _mobility = mobility;
            _inventory = inventory;
            _bag = bag;
            _morale = morale;
            _landing = landing;
            _survival = survival;
            _schedule = schedule;
            _dust = dust;
            _hints = hints;
        }

        public CompositeController Create(Character character)
        {
            var movement = new SideScrollerMovementController(
                character, _input, _dialogue, _time, _mobility, _tentService);
            movement.Jumped += () => HeroJumped?.Invoke();

            var bag = new HeroBagController(character, _bag, _resources, _morale, _survival);
            _landing.Bind(bag);

            return new CompositeController(
                new DialogueController(_dialogue, _input, character.transform, _config),
                new InteractionController(_input,
                    new CircleInteractionScanner(character.transform, _config),
                    new CharacterInteractor(character.transform, _dialogue, _resources, _inventory),
                    _interaction, _dialogue, _time),
                movement,
                bag,
                new MagneticDeviceController(character, _input, _inventory, _resources, _survival,
                    _schedule, _time, _dust, _hints),

                new TentPlacementController(
                    character,
                    _input,
                    _tentService,
                    _dialogue));
        }
    }
}
