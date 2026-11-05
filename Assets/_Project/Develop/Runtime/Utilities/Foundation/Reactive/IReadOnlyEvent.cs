using System;

namespace _Project.Develop.Runtime.Utilities.Foundation.Reactive
{
    public interface IReadOnlyEvent
    {
        IDisposable Subscribe(Action action);
    }

    public interface IReadOnlyEvent<T>
    {
        IDisposable Subscribe(Action<T> action);
    }
}