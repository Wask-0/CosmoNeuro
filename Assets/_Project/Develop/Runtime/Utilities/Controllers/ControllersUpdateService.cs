using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Utilities.Controllers
{
    public class ControllersUpdateService : IDisposable
    {
        private List<ControllersToReason> _controllers = new();

        public void Add(Controller controller, Func<bool> removeReason)
        {
            _controllers.Add(new ControllersToReason(controller, removeReason));
        }

        public void Update (float deltaTime)
        {
            _controllers.RemoveAll(item =>
            {
                if (!item.RemoveReason.Invoke())
                    return false;

                item.Controller.Disable();
                return true;
            });

            foreach (ControllersToReason item in _controllers)
                item.Controller?.Update(deltaTime);
        }

        public void Dispose()
        {
            foreach (ControllersToReason item in _controllers)
                item.Controller.Disable();

            _controllers.Clear();
        }
    }

    public class ControllersToReason
    {
        public ControllersToReason(Controller controller, Func<bool> removeReason)
        {
            Controller = controller;
            RemoveReason = removeReason;
        }

        public Controller Controller { get; }

        public Func<bool> RemoveReason { get; }
    }
    
}