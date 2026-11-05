using System;

namespace _Project.Develop.Runtime.Utilities.Input.Common
{
    public interface ICommonInput
    {
        CommonInputMode CurrentMode { get; }

        event Action ModeChanged;
        event Action PausePressed;
        event Action CancelPressed;

        void SetMode(CommonInputMode mode);
    }
}