// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// Provides a way to define several cursor override on a <see cref="Control"/>.
    /// </summary>
    /// <seealso cref="CursorOverrideRule"/>

    public class MultiOverrideCursorBehavior : Behavior<Control>, IAddChild
    {
        public MultiOverrideCursorBehavior()
        {
            Rules = [];
        }
        private readonly List<IDisposable> subscriptions = [];

        [Content] public CursorOverrideRuleCollection Rules { get; }

        void IAddChild.AddChild([NotNull] object value)
        {
            if (value is CursorOverrideRule rule)
                Rules.Add(rule);
            else
                throw new ArgumentException($"Child has wrong type: {value.GetType().FullName} instead of {nameof(CursorOverrideRule)}.");
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            
            foreach (var rule in Rules)
            {
                subscriptions.Add(rule.GetObservable(CursorOverrideRule.CursorProperty)
                        .Subscribe(new AnonymousObserver<object>((_) => UpdateCursorOverride())));

                subscriptions.Add(rule.GetObservable(CursorOverrideRule.ForceCursorProperty)
                        .Subscribe(new AnonymousObserver<object>((_) => UpdateCursorOverride())));

                subscriptions.Add(rule.GetObservable(CursorOverrideRule.WhenProperty)
                        .Subscribe(new AnonymousObserver<object>((_) => UpdateCursorOverride())));
            }
            UpdateCursorOverride();
        }

        protected override void OnDetaching()
        {
            foreach (var s in subscriptions)
                s.Dispose();

            subscriptions.Clear();

            AssociatedObject?.Cursor = null;
            base.OnDetaching();
        }

        private void UpdateCursorOverride()
        {
            if (AssociatedObject == null)
                return;

            if (Rules.Count == 0 || !Rules.Any(r => r.When))
            {
                AssociatedObject.Cursor = null;
                return;
            }

            var firstRule = Rules.First(r => r.When);
            AssociatedObject.Cursor = firstRule.Cursor;
        }
    }

    /// <summary>
    /// Collection of <see cref="CursorOverrideRule"/>.
    /// </summary>
    public class CursorOverrideRuleCollection : Collection<CursorOverrideRule>
    { }

    public class CursorOverrideRule : AvaloniaObject
    {
        public static readonly AvaloniaProperty CursorProperty = AvaloniaProperty.Register<CursorOverrideRule, Cursor>("Cursor");

        public static readonly AvaloniaProperty ForceCursorProperty = AvaloniaProperty.Register<CursorOverrideRule, bool>("ForceCursor");

        public static readonly AvaloniaProperty WhenProperty = AvaloniaProperty.Register<CursorOverrideRule, bool>("When");

        public Cursor Cursor { get { return (Cursor)GetValue(CursorProperty); } set { SetValue(CursorProperty, value); } }

        public bool ForceCursor { get { return (bool)GetValue(ForceCursorProperty); } set { SetValue(ForceCursorProperty, value.Box()); } }

        public bool When { get { return (bool)GetValue(WhenProperty); } set { SetValue(WhenProperty, value.Box()); } }

        
    }
}
