using _Project.Develop.Runtime.Gameplay.Characters.Core;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    // Герой создаётся после регистрации сервисов сцены; остальные берут ссылку отсюда.
    public sealed class HeroContext
    {
        public Character Character { get; private set; }
        public CharacterView View { get; private set; }

        public void Bind(Character character)
        {
            Character = character;
            View = character.GetComponentInChildren<CharacterView>(true);
        }
    }
}
