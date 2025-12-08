// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Runtime.CompilerServices;

namespace Stride.Core.Translation.Presentation
{
    public class LanguageChangedWeakEventManager
    {
        private readonly ConditionalWeakTable<ITranslationManager, List<WeakReference<EventHandler>>> _handlers = new();

        public void AddListener(ITranslationManager source, EventHandler handler)
        {
            if (!_handlers.TryGetValue(source, out var list))
            {
                list = new List<WeakReference<EventHandler>>();
                _handlers.Add(source, list);
                source.LanguageChanged += OnLanguageChanged;
            }

            list.Add(new WeakReference<EventHandler>(handler));
        }

        public void RemoveListener(ITranslationManager source, EventHandler handler)
        {
            if (_handlers.TryGetValue(source, out var list))
            {
                list.RemoveAll(w => !w.TryGetTarget(out var h) || h == handler);

                if (list.Count == 0)
                    source.LanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            if (sender is ITranslationManager src &&
                _handlers.TryGetValue(src, out var list))
            {
                foreach (var weak in list.ToArray())
                {
                    if (weak.TryGetTarget(out var handler))
                        handler(src, e);
                }
            }
        }
    }
}
