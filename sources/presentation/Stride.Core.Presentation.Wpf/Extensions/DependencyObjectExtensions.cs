// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Stride.Core.Annotations;

namespace Stride.Core.Presentation.Extensions
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Retrieves the public static DependencyProperties.
        /// </summary>
        /// <param name="source">AvaloniaObject that contains the DependencyProperties to be retrieved.</param>
        /// <param name="includingParentProperties">Indicates whether the DependencyProperties declared in the parent classes have to be retrieved too.</param>
        /// <returns>Returns an array of AvaloniaProperty owned by the AvaloniaObject.</returns>
        [NotNull]
        public static AvaloniaProperty[] GetDependencyProperties([NotNull] this AvaloniaObject source, bool includingParentProperties = false)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // there is probably a better way using TypeDescriptor

            var dependencyPropertyType = typeof(AvaloniaProperty);

            var flags = BindingFlags.Public | BindingFlags.Static;
            if (includingParentProperties)
                flags |= BindingFlags.FlattenHierarchy;

            return source.GetType().GetFields(flags)
                .Where(fi => fi.MemberType == MemberTypes.Field && fi.FieldType == dependencyPropertyType)
                .Select(fi => (AvaloniaProperty)fi.GetValue(source))
                .OrderBy(dp => dp.Name)
                .ToArray();
        }

        /// <summary>
        /// Sets the value of a AvaloniaProperty on a AvaloniaObject and all its logical children.
        /// </summary>
        /// <param name="source">Root AvaloniaObject of which to set the AvaloniaProperty value.</param>
        /// <param name="property">AvaloniaProperty to set.</param>
        /// <param name="value">Value to set.</param>
        public static void DeepSetValue([NotNull] this AvaloniaObject source, [NotNull] AvaloniaProperty property, object value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (property == null) throw new ArgumentNullException(nameof(property));

            source.SetValue(property, value);
            if (source is not ILogical logical)
            {
                return;
            }

            foreach (var child in logical.LogicalChildren)
            {
                if (child is AvaloniaObject ao)
                    ao.DeepSetValue(property, value);
            }
        }

        /// <summary>
        /// Find the root parent, along the visual tree.
        /// </summary>
        /// <param name="source">Base node from where to start looking for root.</param>
        /// <returns>Returns the retrieved root, or null otherwise.</returns>
        [CanBeNull]
        public static Visual FindVisualRoot([NotNull] this AvaloniaObject source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Visual root = null;
            while (source != null)
            {
                root = source as Visual;
                source = (source as Visual)?.GetVisualParent();
            }
            return root;
        }

        /// <summary>
        /// Find the first parent that match the given type, along the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of parent to find.</typeparam>
        /// <param name="source">Base node from where to start looking for parent.</param>
        /// <returns>Returns the retrieved parent, or null otherwise.</returns>
        [CanBeNull]
        public static T FindVisualParentOfType<T>([NotNull] this AvaloniaObject source) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return FindParentOfType<T>(source, x => (x as Visual)?.GetVisualParent());;
        }

        /// <summary>
        /// Find the first child that match the given type, along the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of child to find.</typeparam>
        /// <param name="source">Base node from where to start looking for child.</param>
        /// <returns>Returns the retrieved child, or null otherwise.</returns>
        [CanBeNull]
        public static T FindVisualChildOfType<T>([NotNull] this AvaloniaObject source) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return FindChildOfType<T>(source,
                d => (d as Visual)?.GetVisualChildren().Count() ?? 0,
                (d, i) => (d as Visual)?.GetVisualChildren().ElementAt(i));
        }

        /// <summary>
        /// Find all the children that match the given type, along the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of child to find.</typeparam>
        /// <param name="source">Base node from where to start looking for children.</param>
        /// <returns>Returns the retrieved children, or empty otherwise.</returns>
        [ItemNotNull, NotNull]
        public static IEnumerable<T> FindVisualChildrenOfType<T>([NotNull] this AvaloniaObject source) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return FindChildrenOfType<T>(source, d => (d as Visual)?.GetVisualChildren().Count() ?? 0, (d, i) => (d as Visual)?.GetVisualChildren().ElementAt(i));
        }

        /// <summary>
        /// Gets the first visual child of the given object.
        /// </summary>
        /// <param name="source">The object in which to look for a child.</param>
        /// <returns>The child if the given object has children, <c>null</c> otherwise.</returns>
        [CanBeNull]
        public static AvaloniaObject FindFirstVisualChild([NotNull] this AvaloniaObject source)
        {
            if (source is not Visual visual)
                return null;
            
            var childrenCount = visual.GetVisualChildren().Count();
            return childrenCount > 0 ? visual.GetVisualChildren().First() : null;
        }

        /// <summary>
        /// Find the first parent that match the given type, along the logical tree.
        /// </summary>
        /// <typeparam name="T">Type of parent to find.</typeparam>
        /// <param name="source">Base node from where to start looking for parent.</param>
        /// <returns>Returns the retrieved parent, or null otherwise.</returns>
        [CanBeNull]
        public static T FindLogicalParentOfType<T>([NotNull] this AvaloniaObject source) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return FindParentOfType<T>(source, x => (x as ILogical)?.LogicalParent as AvaloniaObject);
        }

        /// <summary>
        /// Finds the first child that match the given type, along the logical tree.
        /// Warning: this function is inefficient, use it for debug but not production.
        /// </summary>
        /// <typeparam name="T">Type of child to find.</typeparam>
        /// <param name="source">Base node from where to start looking for child.</param>
        /// <returns>Returns the retrieved child, or null otherwise.</returns>
        [CanBeNull]
        public static T FindLogicalChildOfType<T>([NotNull] this AvaloniaObject source) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return FindChildOfType<T>(source,
                d => (d as ILogical)?.LogicalChildren.Count ?? 0,
                (d, i) => ((ILogical)d).LogicalChildren.ElementAt(i) as AvaloniaObject);
        }

        /// <summary>
        /// Finds all the children that match the given type, along the logical tree.
        /// Warning: this function is inefficient, use it for debug but not production.
        /// </summary>
        /// <typeparam name="T">Type of child to find.</typeparam>
        /// <param name="source">Base node from where to start looking for children.</param>
        /// <returns>Returns the retrieved children, or empty otherwise.</returns>
        [ItemNotNull, NotNull]
        public static IEnumerable<T> FindLogicalChildrenOfType<T>([NotNull] this AvaloniaObject source) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return FindChildrenOfType<T>(
                source,
                d => (d as ILogical)?.LogicalChildren.Count ?? 0,
                (d, i) => ((ILogical)d).LogicalChildren.ElementAt(i) as AvaloniaObject);
        }

        /// <summary>
        /// Checks that the given dependency object, retrieved as a template part of the calling object using <see cref="Control.GetTemplateChild(string)"/>,
        /// exists and matches the given type.
        /// </summary>
        /// <typeparam name="T">The type expected for the template part.</typeparam>
        /// <param name="templatePart">The template part to evaluate.</param>
        /// <returns>The given template part, cast into the proper type.</returns>
        public static T CheckTemplatePart<T>(AvaloniaObject templatePart) where T : AvaloniaObject
        {
            if (templatePart == null)
                return null;

            if (templatePart is T == false)
            {
                throw new ArgumentException($"Invalid '{templatePart.GetType().FullName}' TemplatePart type. '{typeof(T).FullName}' expected.");
            }

            return (T)templatePart;
        }

        #region Helper methods

        /// <summary>
        /// Find the first parent that match the given type.
        /// </summary>
        /// <typeparam name="T">Type of parent to find.</typeparam>
        /// <param name="source">Base node from where to start looking for parent.</param>
        /// <param name="getParentFunc">Function that provide the parent element.</param>
        /// <returns>Returns the retrieved parent, or null otherwise.</returns>
        [CanBeNull]
        private static T FindParentOfType<T>(AvaloniaObject source, [NotNull] Func<AvaloniaObject, AvaloniaObject> getParentFunc) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (getParentFunc == null) throw new ArgumentNullException(nameof(getParentFunc));

            while (true)
            {
                // try to get visual parent
                var parent = getParentFunc(source);

                if (parent != null)
                {
                    if (parent is T)
                    {
                        // parent is of requested type, returned casted
                        return parent as T;
                    }

                    // there is a parent but not of request type, let's keep traversing the tree up
                    source = parent;
                    continue;
                }

                // failed to find visual parent
                return null;
            }
        }

        /// <summary>
        /// Find the first child that match the given type.
        /// </summary>
        /// <typeparam name="T">Type of child to find.</typeparam>
        /// <param name="source">Base node from where to start looking for child.</param>
        /// <param name="getChildrenCountFunc">Function that provide the number of children in the current element.</param>
        /// <param name="getChildFunc">Function that provide a given child element by its index.</param>
        /// <returns>Returns the retrieved child, or null otherwise.</returns>
        [CanBeNull]
        private static T FindChildOfType<T>([NotNull] AvaloniaObject source, [NotNull] Func<AvaloniaObject, int> getChildrenCountFunc,
            [NotNull] Func<AvaloniaObject, int, AvaloniaObject> getChildFunc) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (getChildrenCountFunc == null) throw new ArgumentNullException(nameof(getChildrenCountFunc));
            if (getChildFunc == null) throw new ArgumentNullException(nameof(getChildFunc));

            var childCount = getChildrenCountFunc(source);
            for (var i = 0; i < childCount; i++)
            {
                var child = getChildFunc(source, i);
                if (child != null)
                {
                    if (child is T)
                        return child as T;
                    child = FindChildOfType<T>(child, getChildrenCountFunc, getChildFunc);
                    if (child != null)
                        return (T)child;
                }
            }
            return null;
        }

        /// <summary>
        /// Find all the children that match the given type.
        /// </summary>
        /// <typeparam name="T">Type of child to find.</typeparam>
        /// <param name="source">Base node from where to start looking for children.</param>
        /// <param name="getChildrenCountFunc">Function that provide the number of children in the current element.</param>
        /// <param name="getChildFunc">Function that provide a given child element by its index.</param>
        /// <returns>Returns the retrieved children, empty otherwise.</returns>
        [ItemNotNull, NotNull]
        private static IEnumerable<T> FindChildrenOfType<T>([NotNull] AvaloniaObject source, [NotNull] Func<AvaloniaObject, int> getChildrenCountFunc,
            [NotNull] Func<AvaloniaObject, int, AvaloniaObject> getChildFunc) where T : AvaloniaObject
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (getChildrenCountFunc == null) throw new ArgumentNullException(nameof(getChildrenCountFunc));
            if (getChildFunc == null) throw new ArgumentNullException(nameof(getChildFunc));

            var childCount = getChildrenCountFunc(source);
            for (var i = 0; i < childCount; i++)
            {
                var child = getChildFunc(source, i);
                if (child == null)
                    continue;

                if (child is T)
                    yield return child as T;

                foreach (var subChild in FindChildrenOfType<T>(child, getChildrenCountFunc, getChildFunc))
                {
                    yield return subChild;
                }
            }
        }

        #endregion
    }
}
