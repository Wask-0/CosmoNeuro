using DG.Tweening;

namespace _Project.Develop.Runtime.UI.Core
{
    public interface IShowableView : IView
    {
        Tween Hide();
        
        Tween Show();
    }
}