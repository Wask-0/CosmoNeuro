using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Scanning
{
    public sealed class CircleInteractionScanner
    {
        private readonly Transform _origin;
        private readonly InteractionConfig _config;
        private readonly List<Collider2D> _hits = new();
        private readonly HashSet<Interactable> _visited = new();
        private Interactable _previous;
        public CircleInteractionScanner(Transform origin, InteractionConfig config)
        { _origin = origin; _config = config; }

        public Interactable FindNearest(InteractionContext context)
        {
            if (_origin == null) return null;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(_config.DetectionMask);
            filter.useTriggers = true;
            _hits.Clear();
            _visited.Clear();
            Physics2D.OverlapCircle(_origin.position, _config.Radius, filter, _hits);
            Interactable nearest = null;
            float best = _config.Radius * _config.Radius;
            foreach (Collider2D hit in _hits)
            {
                if (hit == null || hit.transform.IsChildOf(_origin)) continue;
                Interactable candidate = hit.GetComponentInParent<Interactable>();
                if (candidate == null || !_visited.Add(candidate) || candidate.InteractionPoint == null)
                    continue;
                if (!candidate.GetInteractionInfo(context).IsAvailable) continue;
                float distance = ((Vector2)(candidate.InteractionPoint.position - _origin.position)).sqrMagnitude;
                // Stable selection when two targets are equally close.
                if (distance > _config.Radius * _config.Radius) continue;
                if (nearest == null || distance < best ||
                    (distance == best && candidate == _previous))
                { nearest = candidate; best = distance; }
            }
            _previous = nearest;
            return nearest;
        }
    }
}
