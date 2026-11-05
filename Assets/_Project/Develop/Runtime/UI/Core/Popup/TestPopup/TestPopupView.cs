using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Core.Popup.TestPopup
{
    public class TestPopupView : PopupViewBase
    {
        [SerializeField] private TMP_Text _text;

        public void SetText(string text) => _text.text = text;

        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            animation
                .Append(_text
                    .DOFade(1, 0.2f)
                    .From(0));
        }
    }
}