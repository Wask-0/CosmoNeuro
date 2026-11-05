using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.UI.Core.Popup;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.MainScene
{
    public class MainScenePopupService : PopupService
    {
        private readonly MainSceneUIRoot _root;

        public MainScenePopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory presentersFactory,
            MainSceneUIRoot root)
            : base(viewsFactory, presentersFactory)
        {
            _root = root;
        }

        protected override Transform PopupLayer => _root.PopupsLayer;
    }
}