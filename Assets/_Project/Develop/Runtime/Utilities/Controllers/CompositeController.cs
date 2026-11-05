namespace _Project.Develop.Runtime.Utilities.Controllers
{
    public class CompositeController : Controller
    {
        private readonly Controller[] _controllers;

        public CompositeController(params Controller[] controllers)
        {
            _controllers = controllers;
        }

        protected override void OnEnable()
        {
            foreach (Controller controller in _controllers)
                controller.Enable();
        }

        protected override void OnDisable()
        {
            foreach (Controller controller in _controllers)
                controller.Disable();
        }

        protected override void UpdateLogic(float deltaTime)
        {
            foreach (Controller controller in _controllers)
                controller.Update(deltaTime);
        }
    }
}