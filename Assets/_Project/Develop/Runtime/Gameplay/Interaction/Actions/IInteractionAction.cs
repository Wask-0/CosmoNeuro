using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public interface IInteractionAction
    {
        Task ExecuteAsync(
            InteractionContext context,
            CancellationToken cancellationToken);
    }
}