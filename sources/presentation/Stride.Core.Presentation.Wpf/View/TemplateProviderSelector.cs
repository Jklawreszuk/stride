// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using Stride.Core.Annotations;
using Stride.Core.Extensions;

namespace Stride.Core.Presentation.View
{
    /// <summary>
    /// An implementation of <see cref="IDataTemplate"/> that can select a template from a set of statically registered <see cref="ITemplateProvider"/> objects.
    /// </summary>
    public class TemplateProviderSelector : IDataTemplate
    {
        /// <summary>
        /// The list of all template providers registered for the <see cref="TemplateProviderSelector"/>, indexed by their name.
        /// </summary>
        private readonly List<ITemplateProvider> templateProviders = [];

        /// <summary>
        /// A hashset of template provider names, used only to ensure unicity.
        /// </summary>
        private readonly HashSet<string> templateProviderNames = [];

        /// <summary>
        /// A map of all providers that have already been used for each object, indexed by <see cref="Guid"/>.
        /// </summary>
        private readonly ConditionalWeakTable<object, List<string>> usedProviders = new();

        /// <summary>
        /// Registers the given template into the static <see cref="TemplateProviderSelector"/>.
        /// </summary>
        /// <param name="templateProvider"></param>
        public void RegisterTemplateProvider([NotNull] ITemplateProvider templateProvider)
        {
            if (templateProvider == null) throw new ArgumentNullException(nameof(templateProvider));

            if (templateProviderNames.Contains(templateProvider.Name))
                throw new InvalidOperationException("A template provider with the same name has already been registered in this template selector.");

            InsertTemplateProvider(templateProviders, templateProvider, []);
            templateProviderNames.Add(templateProvider.Name);
        }

        /// <summary>
        /// Unregisters the given template into the static <see cref="TemplateProviderSelector"/>.
        /// </summary>
        /// <param name="templateProvider"></param>
        public void UnregisterTemplateProvider([NotNull] ITemplateProvider templateProvider)
        {
            if (templateProviderNames.Remove(templateProvider.Name))
            {
                templateProviders.Remove(templateProvider);
            }
        }

        public Control Build(object item)
        {
            if (item == null)
                return null;

            var provider = FindTemplateProvider(item);
            var template = provider?.Template;
            return template.Build(item);
        }

        public bool Match(object data)
        {
            return data != null;
        }
        
        private static void InsertTemplateProvider([NotNull] List<ITemplateProvider> list, ITemplateProvider templateProvider, [NotNull] List<ITemplateProvider> movedItems)
        {
            movedItems.Add(templateProvider);
            // Find the first index where we can insert
            var insertIndex = 1 + list.LastIndexOf(x => x.CompareTo(templateProvider) < 0);
            list.Insert(insertIndex, templateProvider);
            // Every following providers may have an override rule against the new template provider, we must potentially resort them.
            for (var i = insertIndex + 1; i < list.Count; ++i)
            {
                var followingProvider = list[i];
                if (followingProvider.CompareTo(templateProvider) < 0)
                {
                    if (!movedItems.Contains(followingProvider))
                    {
                        list.RemoveAt(i);
                        InsertTemplateProvider(list, followingProvider, movedItems);
                    }
                }
            }
        }

        [CanBeNull]
        private ITemplateProvider FindTemplateProvider([NotNull] object item)
        {
            var usedProvidersForItem = usedProviders.GetOrCreateValue(item);

            var availableSelectors = templateProviders.Where(x => x.Match(item));

            var result = availableSelectors.FirstOrDefault(x => !usedProvidersForItem.Contains(x.Name));

            if (result != null)
            {
                usedProvidersForItem.Add(result.Name);
            }
            return result;
        }
    }
}
