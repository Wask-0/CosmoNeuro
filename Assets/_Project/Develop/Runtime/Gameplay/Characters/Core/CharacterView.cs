using System;
using DG.Tweening;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Characters.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CharacterView : MonoBehaviour
    {
        [SerializeField] private Character _character;
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite[] _walkSprites = Array.Empty<Sprite>();
        [SerializeField, Min(0.1f)] private float _walkFramesPerSecond = 8f;
        [SerializeField] private bool _spritesFaceRight = true;
        [SerializeField, Min(0f)] private float _movementThreshold = 0.05f;
        [SerializeField] private Sprite[] _deviceSprites = Array.Empty<Sprite>();
        [SerializeField] private bool _deviceSpritesFaceRight = true;
        [SerializeField, Min(0.1f)] private float _deviceFramesPerSecond = 6f;
        [SerializeField] private Sprite _climbSprite;
        [SerializeField, Range(0f, 0.2f)] private float _breathingAmount = 0.03f;
        [SerializeField, Min(0.1f)] private float _breathingPeriod = 2f;

        private SpriteRenderer _renderer;
        private Vector3 _baseScale;
        private float _breathingTime;
        private float _walkFrame;
        private bool _wasMoving;
        private float _deviceFrame;

        public bool IsUsingDevice { get; set; }
        public bool IsClimbing { get; set; }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            if (_idleSprite == null)
                _idleSprite = _renderer.sprite;
            _renderer.sprite = _idleSprite;
        }

        // Герой приходит в себя и встаёт с земли после падения.
        public void PlayGetUp(float delay, float duration, Action completed)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 84f);
            transform.DOLocalRotate(Vector3.zero, duration)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject)
                .OnComplete(() => completed?.Invoke());
        }

        private void LateUpdate()
        {
            if (IsClimbing)
            {
                _renderer.sprite = _climbSprite != null ? _climbSprite : _idleSprite;
                transform.localScale = _baseScale;
                return;
            }

            if (IsUsingDevice && _deviceSprites.Length > 0)
            {
                ShowDevice(Time.deltaTime);
                return;
            }

            float speed = _character.CurrentVelocity.x;
            bool isWalking = _character.HasMoveInput && Mathf.Abs(speed) > _movementThreshold;

            if (isWalking)
                ShowWalk(speed, Time.deltaTime);
            else
                ShowIdle(Time.deltaTime);

            _wasMoving = isWalking;
        }

        private void ShowDevice(float deltaTime)
        {
            _renderer.flipX = (_character.Facing > 0f) != _deviceSpritesFaceRight;
            transform.localScale = _baseScale;
            _deviceFrame = Mathf.Repeat(_deviceFrame + deltaTime * _deviceFramesPerSecond, _deviceSprites.Length);
            Sprite frame = _deviceSprites[(int)_deviceFrame];
            _renderer.sprite = frame != null ? frame : _idleSprite;
        }

        private void ShowWalk(float speed, float deltaTime)
        {
            _renderer.flipX = (speed > 0f) != _spritesFaceRight;
            transform.localScale = _baseScale;
            _breathingTime = 0f;

            if (_walkSprites.Length == 0)
                return;

            _walkFrame = _wasMoving
                ? Mathf.Repeat(_walkFrame + deltaTime * _walkFramesPerSecond, _walkSprites.Length)
                : 0f;
            Sprite frame = _walkSprites[(int)_walkFrame];
            _renderer.sprite = frame != null ? frame : _idleSprite;
        }

        private void ShowIdle(float deltaTime)
        {
            _renderer.sprite = _idleSprite;
            _renderer.flipX = (_character.Facing > 0f) != _spritesFaceRight;
            float period = Mathf.Max(0.1f, _breathingPeriod);
            _breathingTime = Mathf.Repeat(_breathingTime + deltaTime, period);
            float stretch = Mathf.Sin(_breathingTime / period * 2f * Mathf.PI) * _breathingAmount;
            transform.localScale = Vector3.Scale(_baseScale, new Vector3(1f - stretch, 1f + stretch, 1f));
        }
    }
}
