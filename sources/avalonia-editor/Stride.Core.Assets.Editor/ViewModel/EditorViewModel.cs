// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Stride.Core.MostRecentlyUsedFiles;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Core.Assets.Editor.ViewModel
{
    public abstract class EditorViewModel : ViewModelBase
    {
        public const string PackageFileExtension = Package.PackageFileExtension;
        public const string SolutionFileExtension = ".sln";
        public MostRecentlyUsedFileCollection MRU { get; }
    }
}
