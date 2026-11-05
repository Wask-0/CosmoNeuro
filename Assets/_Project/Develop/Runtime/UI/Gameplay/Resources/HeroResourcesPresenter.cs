using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.Utilities.Foundation.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Resources
{
    public sealed class HeroResourcesPresenter : IPresenter
    {
        private readonly HeroResources _resources;
        private readonly HeroResourcesView _view;
        private readonly List<IDisposable> _subscriptions = new();
        private bool _initialized;
        private bool _disposed;

        public HeroResourcesPresenter(HeroResources resources, HeroResourcesView view)
        {
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HeroResourcesPresenter));
            if (_initialized) return;
            if (_view.Food == null || _view.Energy == null || _view.Oxygen == null || _view.WarmDust == null)
                throw new InvalidOperationException("Assign all gauges in HeroResourcesView.");

            Bind(_resources.Food, _view.Food, _resources.MaximumFood, value => value, "ЕДА");
            Bind(_resources.Energy.Current, _view.Energy, _resources.Energy.Maximum, value => value, "ЭНЕРГИЯ");
            Bind(_resources.Oxygen.Current, _view.Oxygen, _resources.Oxygen.Maximum, value => value, "КИСЛОРОД");
            Bind(_resources.WarmDust.Current, _view.WarmDust, _resources.WarmDust.Maximum, value => value, "ТЕПЛО");
            _initialized = true;
        }

        private void Bind<T>(IReadOnlyVariable<T> source, ResourceGaugeView gauge,
            float maximum, Func<T, float> getAmount, string label)
        {
            void Refresh(T value)
                => gauge.SetValue(maximum > 0f ? Mathf.Clamp01(getAmount(value) / maximum) : 0f);

            gauge.SetLabel(label);
            _subscriptions.Add(source.Subscribe((previous, current) => Refresh(current)));
            Refresh(source.Value);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (IDisposable subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();
        }
    }
}
