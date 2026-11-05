using _Project.Develop.Runtime.Gameplay.GameTime;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    internal sealed class RotatingStarfield
    {
        private const int TextureSize = 1024;
        private const int StarCount = 1700;
        private const float DiscRadius = TextureSize / 2f - 4f;

        private readonly Camera _camera;
        private readonly Transform _target;
        private readonly GameTimeService _time;
        private readonly Transform _disc;
        private readonly Texture2D _texture;
        private readonly Sprite _sprite;
        private float _radius;

        public RotatingStarfield(Camera camera, Transform target, GameTimeService time)
        {
            _camera = camera;
            _target = target;
            _time = time;
            _texture = CreateTexture();
            _sprite = Sprite.Create(_texture, new Rect(0, 0, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f), DiscRadius, 0, SpriteMeshType.FullRect);

            var background = new GameObject("Rotating starfield");
            _disc = background.transform;
            _disc.SetParent(camera.transform, false);
            var renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = _sprite;
            renderer.sortingOrder = short.MinValue;
            Refresh();
        }

        public void Refresh()
        {
            if (_disc == null)
                return;

            Vector3 center = _disc.localPosition;
            if (_target != null)
                center = _camera.transform.InverseTransformPoint(_target.position);

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            var farthestCorner = new Vector2(halfWidth + Mathf.Abs(center.x), halfHeight + Mathf.Abs(center.y));

            // Reserve space for movement and keep the scale stable during ordinary play.
            if (farthestCorner.magnitude > _radius)
                _radius = farthestCorner.magnitude + halfHeight;

            center.z = Mathf.Lerp(_camera.nearClipPlane, _camera.farClipPlane, 0.9f);
            _disc.localPosition = center;
            _disc.localScale = Vector3.one * _radius;
            _disc.localRotation = Quaternion.Euler(0f, 0f, -360f * (float)_time.Current.DayProgress);
        }

        public void Dispose()
        {
            if (_disc != null)
                Object.Destroy(_disc.gameObject);
            Object.Destroy(_sprite);
            Object.Destroy(_texture);
        }

        private static Texture2D CreateTexture()
        {
            var random = new System.Random(24601);
            var pixels = new Color32[TextureSize * TextureSize];

            for (int i = 0; i < StarCount; i++)
            {
                float angle = (float)random.NextDouble() * (2f * Mathf.PI);
                float distance = Mathf.Sqrt((float)random.NextDouble()) * DiscRadius;
                int x = Mathf.RoundToInt(TextureSize / 2f + Mathf.Cos(angle) * distance);
                int y = Mathf.RoundToInt(TextureSize / 2f + Mathf.Sin(angle) * distance);

                int brightness = random.Next(45, 221);
                var color = new Color32(208, 225, 255, (byte)brightness);
                pixels[y * TextureSize + x] = color;

                if (brightness >= 200)
                {
                    color.a = (byte)(brightness / 3);
                    pixels[y * TextureSize + x - 1] = color;
                    pixels[y * TextureSize + x + 1] = color;
                    pixels[(y - 1) * TextureSize + x] = color;
                    pixels[(y + 1) * TextureSize + x] = color;
                }
            }

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = "Starfield texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
