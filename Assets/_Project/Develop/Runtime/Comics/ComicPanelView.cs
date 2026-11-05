using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.Comics
{
    public sealed class ComicPanelView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _caption;

        public void Hide()
        {
            _group.alpha = 0f;
            transform.localScale = Vector3.one;
        }

        public void Show(Sprite image, string caption)
        {
            _image.sprite = image;
            _caption.text = caption;
            transform.localScale = Vector3.one * 0.96f;
            _group.DOFade(1f, 0.45f).SetUpdate(true).SetLink(gameObject);
            transform.DOScale(1f, 0.45f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
        }
    }
}
