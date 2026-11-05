using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Develop.Runtime.UI.LoadingScreen
{
    public class StandardLoadingScreen : MonoBehaviour, ILoadingScreen
    {
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private LocalizedString _loadingString;
        
        [SerializeField] private RectTransform _spinner;
        [SerializeField, Min(0f)] private float _rotationSpeed = 180f;
        
        public bool IsShow => gameObject.activeSelf;


        public void Hide() => gameObject.SetActive(false);

        public void Show() => gameObject.SetActive(true);

        private void Update()
        {
            if (_spinner == null)
                return;

            _spinner.Rotate(0f, 0f, -_rotationSpeed * Time.unscaledDeltaTime);
        }

        private void Awake()
        {
            Hide();
            DontDestroyOnLoad(this);
        }
        
        private void OnEnable() => _loadingString.StringChanged += UpdateLoadingText;

        private void OnDisable() => _loadingString.StringChanged -= UpdateLoadingText;
        
        private void UpdateLoadingText(string value) => _loadingText.text = value;
    }
}
