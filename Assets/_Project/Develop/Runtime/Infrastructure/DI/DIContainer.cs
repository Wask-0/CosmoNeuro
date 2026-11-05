using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Infrastructure.DI
{
    public class DIContainer
    {
        private readonly Dictionary<Type, Registration> _container = new();

        private readonly List<Type> _requests = new();

        private readonly DIContainer _parent;
        
        
        private readonly List<Registration> _registrationsInOrder = new();
        private bool _isDisposed;
        

        public DIContainer() : this(null)
        {
        }

        public DIContainer(DIContainer parent) => _parent = parent;

        public IRegistrationOptions RegisterAsSingle<T>(Func<DIContainer, T> creator)
        {
            if (IsAlreadyRegister<T>())
                throw new InvalidOperationException($"{typeof(T)} already register");

            Registration registration = new Registration(container => creator.Invoke(container));
            _container.Add(typeof(T), registration);
            
            _registrationsInOrder.Add(registration);
            
            return registration;
        }

        public bool IsAlreadyRegister<T>()
        {
            if (_container.ContainsKey(typeof(T)))
                return true;

            if (_parent != null)
                return _parent.IsAlreadyRegister<T>();

            return false;
        }

        public T Resolve<T>()
        {
            if (_requests.Contains(typeof(T)))
                throw new InvalidOperationException($"Cycle resolve for {typeof(T)}");

            _requests.Add(typeof(T));

            try
            {
                if (_container.TryGetValue(typeof(T), out Registration registration))
                    return (T)registration.CreateInstanceFrom(this);

                if (_parent != null)
                    return _parent.Resolve<T>();
            }
            finally
            {
                _requests.Remove(typeof(T));
            }

            throw new InvalidOperationException($"Registration for {typeof(T)} not exists");
        }

        public void Initialize()
        {
            foreach (Registration registration in _container.Values)
            {
                if (registration.IsNonLazy)
                    registration.CreateInstanceFrom(this);

                registration.OnInitialize();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            List<Exception> errors = null;

            for (int index = _registrationsInOrder.Count - 1; index >= 0; index--)
            {
                try
                {
                    _registrationsInOrder[index].OnDispose();
                }
                catch (Exception exception)
                {
                    errors ??= new List<Exception>();
                    errors.Add(exception);
                }
            }

            if (errors != null)
                throw new AggregateException("Some services could not be disposed.", errors);
        }
    }
}
