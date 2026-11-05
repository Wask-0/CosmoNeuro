using System;

namespace _Project.Develop.Runtime.Utilities.Foundation.Reactive
{
    public interface IReadOnlyVariable<T>
    {
        T Value { get; }

        IDisposable Subscribe(Action<T, T> action);
    }
}