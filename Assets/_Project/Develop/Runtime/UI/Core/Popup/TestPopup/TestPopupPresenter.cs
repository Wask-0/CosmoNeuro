using _Project.Develop.Runtime.Utilities.CoroutinesManagement;

namespace _Project.Develop.Runtime.UI.Core.Popup.TestPopup
{
    public class TestPopupPresenter : PopupPresenterBase
    {
        private readonly TestPopupView _view;

        public TestPopupPresenter(TestPopupView view, ICoroutinesPerformer coroutinesPerformer) : base (coroutinesPerformer)
        {
            _view = view;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetText("TEST TITLE");
        }
    }
}