using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    // Простая покадровая анимация декораций.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFrameLoop : MonoBehaviour
    {
        [SerializeField] private Sprite[] _frames = System.Array.Empty<Sprite>();
        [SerializeField, Min(0.1f)] private float _framesPerSecond = 6f;

        private SpriteRenderer _renderer;
        private float _time;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _time = transform.position.x;
        }

        private void Update()
        {
            if (_frames.Length == 0) return;
            _time += Time.deltaTime;
            _renderer.sprite = _frames[(int)Mathf.Repeat(_time * _framesPerSecond, _frames.Length)];
        }
    }
}
