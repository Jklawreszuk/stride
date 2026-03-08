// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Core;

namespace Stride.Core.Presentation.Behaviors
{
    public class ItemsControlCollectionViewBehavior : Behavior<ItemsControl>
    {
        private readonly DependencyPropertyWatcher propertyWatcher = new DependencyPropertyWatcher();

        public static readonly StyledProperty<string> GroupingPropertyNameProperty = AvaloniaProperty.Register<ItemsControlCollectionViewBehavior, string>("GroupingPropertyName");

        public static readonly StyledProperty<Predicate<object>> FilterPredicateProperty = AvaloniaProperty.Register<ItemsControlCollectionViewBehavior, Predicate<object>>("FilterPredicate");

        public string GroupingPropertyName { get { return GetValue(GroupingPropertyNameProperty); } set { SetValue(GroupingPropertyNameProperty, value); } }
  
        public Predicate<object> FilterPredicate { get { return GetValue(FilterPredicateProperty); } set { SetValue(FilterPredicateProperty, value); } }

        public IValueConverter GroupingPropertyConverter { get; set; }

        static ItemsControlCollectionViewBehavior()
        {
            GroupingPropertyNameProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string>>(GroupingPropertyNameChanged));
            FilterPredicateProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<Predicate<object>>>(FilterPredicateChanged));
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            propertyWatcher.Attach(AssociatedObject);
            propertyWatcher.RegisterValueChangedHandler(ItemsControl.ItemsSourceProperty, ItemsSourceChanged);
            UpdateCollectionView();
        }

        protected override void OnDetaching()
        {
            propertyWatcher.Detach();
            base.OnDetaching();
        }

        private void UpdateCollectionView()
        {
            if (AssociatedObject?.ItemsSource != null)
            {
                var collectionView = (CollectionView)CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);
                if (collectionView == null) throw new InvalidOperationException("CollectionViewSource.GetDefaultView returned null for the items source of the associated object.");
                using (collectionView.DeferRefresh())
                {
                    bool removeGrouping = string.IsNullOrWhiteSpace(GroupingPropertyName);
                    if (collectionView.CanGroup)
                    {
                        if (collectionView.GroupDescriptions == null) throw new InvalidOperationException("CollectionView does not have a group description collection.");
                        collectionView.GroupDescriptions.Clear();

                        if (!removeGrouping)
                        {
                            var groupDescription = new PropertyGroupDescription(GroupingPropertyName, GroupingPropertyConverter);
                            collectionView.GroupDescriptions.Add(groupDescription);
                        }
                    }
                    if (collectionView.CanFilter)
                    {
                        collectionView.Filter = FilterPredicate;
                    }
                }
            }
        }

        private void ItemsSourceChanged(object sender, EventArgs e)
        {
            UpdateCollectionView();
        }

        private static void GroupingPropertyNameChanged(AvaloniaPropertyChangedEventArgs<string> obj)
        {
            var behavior = (ItemsControlCollectionViewBehavior)obj.Sender;
            behavior.UpdateCollectionView();
        }

        private static void FilterPredicateChanged(AvaloniaPropertyChangedEventArgs<Predicate<object>> obj)
        {
            var behavior = (ItemsControlCollectionViewBehavior)obj.Sender;
            behavior.UpdateCollectionView();
        }
    }
}

