using _Project.Develop.Runtime.Utilities.Input.Common;

namespace _Project.Develop.Runtime.Utilities.Input.Contracts
{
    public interface ICharacterInput
    {
        CommonInputMode CurrentMode { get; }
        
        float Move { get; }
        
        bool PlaceTentPressed { get; }
        
        bool PackTentPressed { get; }

        bool JumpPressed { get; }

        bool DeviceHeld { get; }

        bool BagPressed { get; }

        bool TryConsumeInteract();
    }
}
