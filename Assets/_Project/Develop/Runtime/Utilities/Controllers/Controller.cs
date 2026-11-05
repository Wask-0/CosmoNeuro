namespace _Project.Develop.Runtime.Utilities.Controllers
{
    public abstract class Controller
    {
        private bool _isEnabled;

        protected bool IsEnabled => _isEnabled;

        public void Enable()
        {
            if (_isEnabled)
                return;

            _isEnabled = true;
            OnEnable();
        }

        public void Disable()
        {
            if (!_isEnabled)
                return;

            _isEnabled = false;
            OnDisable();
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        public void Update(float deltaTime)
        {
            if (!_isEnabled)
                return;

            UpdateLogic(deltaTime);
        }

        protected abstract void UpdateLogic(float deltaTime);
    }
}
