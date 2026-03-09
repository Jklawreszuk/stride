// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Stride.Core.Annotations;
using Stride.Core.Extensions;

namespace Stride.Core.Presentation.Core
{
    public class DependencyPropertyWatcher
    {
        private readonly List<(AvaloniaProperty, EventHandler)> handlers = [];
        private readonly Dictionary<AvaloniaProperty, List<IDisposable>> subscriptions = new();
        private Control frameworkElement;
        private bool handlerRegistered;

        public DependencyPropertyWatcher()
        {
        }

        public DependencyPropertyWatcher([NotNull] Control attachTo)
        {
            Attach(attachTo);
        }

        public AvaloniaObject AssociatedObject => frameworkElement;

        public void Attach([NotNull] AvaloniaObject dependencyObject)
        {
            if (dependencyObject == null) throw new ArgumentNullException(nameof(dependencyObject));
            if (ReferenceEquals(dependencyObject, frameworkElement))
                return;

            if (frameworkElement != null)
                throw new InvalidOperationException("A dependency object is already attached to this instance of DependencyPropertyWatcher.");
            frameworkElement = dependencyObject as Control;

            if (frameworkElement == null)
                throw new ArgumentException("The dependency object to attach to the DependencyPropertyWatcher must be a Control.");

            frameworkElement.Loaded += ElementLoaded;
            frameworkElement.Unloaded += ElementUnloaded;
            AttachHandlers();
        }

        public void Detach()
        {
            frameworkElement.Loaded -= ElementLoaded;
            frameworkElement.Unloaded -= ElementUnloaded;
            DetachHandlers();
            handlers.Clear();
            subscriptions.Clear();
            frameworkElement = null;
        }

        public void RegisterValueChangedHandler(AvaloniaProperty property, EventHandler handler)
        {
            handlers.Add((property, handler));
            if (handlerRegistered)
            {
                AttachHandler(property, handler);
            }
        }

        public void UnregisterValueChangedHander(AvaloniaProperty property, EventHandler handler)
        {
            handlers.RemoveWhere(x => x.Item1 == property && x.Item2 == handler);
            if (handlerRegistered)
            {
                DetachHandler(property, handler);
            }
        }

        private void AttachHandlers()
        {
            if (!handlerRegistered)
            {
                foreach (var handler in handlers)
                {
                    AttachHandler(handler.Item1, handler.Item2);
                }
                handlerRegistered = true;
            }
        }

        private void DetachHandlers()
        {
            if (handlerRegistered)
            {
                foreach (var handler in handlers)
                {
                    DetachHandler(handler.Item1, handler.Item2);
                }
                handlerRegistered = false;
            }
        }

        private void AttachHandler([NotNull] AvaloniaProperty property, [NotNull] EventHandler handler)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (frameworkElement == null) throw new InvalidOperationException("A dependency object must be attached in order to register a handler.");

            var observable = frameworkElement.GetObservable(property);

            var subscription = observable.Subscribe(new AnonymousObserver<object>(_ =>
            {
                handler(frameworkElement, EventArgs.Empty);
            }));

            if (!subscriptions.TryGetValue(property, out var list))
            {
                list = [];
                subscriptions[property] = list;
            }

            list.Add(subscription);
        }

        private void DetachHandler([NotNull] AvaloniaProperty property, [NotNull] EventHandler handler)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (frameworkElement == null) throw new InvalidOperationException("A dependency object must be attached in order to unregister a handler.");

            if (!subscriptions.TryGetValue(property, out var list))
                return;

            foreach (var sub in list)
                sub.Dispose();

            subscriptions.Remove(property);
        }

        private void ElementLoaded(object sender, RoutedEventArgs e)
        {
            AttachHandlers();
        }

        private void ElementUnloaded(object sender, RoutedEventArgs e)
        {
            DetachHandlers();
        }
    }
}
