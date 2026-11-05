using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Morale
{
    public sealed class MoralePresenter : IPresenter
    {
        private readonly HeroMoraleService _morale;
        private readonly MoraleView _view;

        public MoralePresenter(HeroMoraleService morale, MoraleView view)
        {
            _morale = morale;
            _view = view;
        }

        public void Initialize()
        {
            _morale.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            _view.SetLevel(_morale.Level);
            _view.SetRising(_morale.IsRising);
        }

        public void Dispose() => _morale.Changed -= Refresh;
    }
}
