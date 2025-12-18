// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Stride.Core.Assets.Templates;
using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Core.Assets.Editor.Components.TemplateDescriptions.ViewModels
{
    public class TemplateDescriptionViewModel : DispatcherViewModel, ITemplateDescriptionViewModel
    {
        protected readonly TemplateDescription Template;

        public TemplateDescriptionViewModel(IViewModelServiceProvider serviceProvider, TemplateDescription template)
            : base(serviceProvider)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            Template = template;
            var asmName = Assembly.GetExecutingAssembly().GetName().Name;
            if (!string.IsNullOrEmpty(template.Icon))
                Icon = LoadImage(GetPath(template.Icon));
            if (Icon == null)
                Icon = LoadImage($@"avares://{asmName};component/Resources/Images/default-template-icon.png");
            Screenshots = template.Screenshots.Select(GetPath).Select(LoadImage).NotNull().ToList();

        }

        public string Name => Template.Name;

        public string Description => Template.Description;

        public string FullDescription => Template.FullDescription;

        public string Group => Template.Group;

        public int Order => Template.Order;

        public Guid Id => Template.Id;

        public string DefaultOutputName => Template.DefaultOutputName;

        public Bitmap Icon { get; }

        public IEnumerable<Bitmap> Screenshots { get; }

        public TemplateDescription GetTemplate()
        {
            return Template;
        }

        private string GetPath(UFile imagePath)
        {
            return UPath.Combine(Template.FullPath.GetFullDirectory(), imagePath);
        }

        private static Bitmap LoadImage(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                // Embedded Avalonia asset
                if (path.StartsWith("avares://", StringComparison.Ordinal))
                {
                    using var stream = AssetLoader.Open(new Uri(path));
                    return new Bitmap(stream);
                }

                // File system path
                if (!File.Exists(path))
                    return null;

                using var fileStream = File.OpenRead(path);
                return new Bitmap(fileStream);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
