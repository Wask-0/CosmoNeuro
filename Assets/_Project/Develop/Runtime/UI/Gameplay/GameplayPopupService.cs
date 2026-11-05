using System;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.UI.Core.Popup;
using _Project.Develop.Runtime.UI.Gameplay.Pause;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPopupService : PopupService
    {
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        private readonly GameplayUIRoot _root;

        public GameplayPopupService(
            ViewsFactory viewsFactory, ProjectPresentersFactory projectPresentersFactory,
            GameplayPresentersFactory gameplayPresentersFactory, GameplayUIRoot root)
            : base(viewsFactory, projectPresentersFactory)
        {
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _root = root;
        }

        protected override Transform PopupLayer => _root.PopupsLayer;

        public PausePopupPresenter OpenPause(Action closed = null)
        {
            PausePopupView view = ViewsFactory.Create<PausePopupView>(ViewIDs.PausePopup, PopupLayer);

            PausePopupPresenter presenter = _gameplayPresentersFactory.CreatePausePopupPresenter(view);

            OnPopupCreated(presenter, view, closed);

            return presenter;
        }
    }
}