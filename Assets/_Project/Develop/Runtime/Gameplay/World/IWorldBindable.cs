using _Project.Develop.Runtime.Infrastructure.DI;

namespace _Project.Develop.Runtime.Gameplay.World
{
    // Объекты сцены получают сервисы от загрузчика после создания героя.
    public interface IWorldBindable
    {
        void Bind(DIContainer container);
    }
}
