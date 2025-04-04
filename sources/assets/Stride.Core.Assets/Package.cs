// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics;
using Stride.Core.Assets.Analysis;
using Stride.Core.Assets.Diagnostics;
using Stride.Core.Assets.Templates;
using Stride.Core.Assets.Yaml;
using Stride.Core.Annotations;
using Stride.Core.Diagnostics;
using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Reflection;
using Stride.Core.Serialization;
using Stride.Core.Yaml;

namespace Stride.Core.Assets;

public enum PackageState
{
    /// <summary>
    /// Package has been deserialized. References and assets are not ready.
    /// </summary>
    Raw,

    /// <summary>
    /// Dependencies have all been resolved and are also in <see cref="DependenciesReady"/> state.
    /// </summary>
    DependenciesReady,

    /// <summary>
    /// Package upgrade has been failed (either error or denied by user).
    /// Dependencies are ready, but not assets.
    /// Should be manually switched back to DependenciesReady to try upgrade again.
    /// </summary>
    UpgradeFailed,

    /// <summary>
    /// Assembly references and assets have all been loaded.
    /// </summary>
    AssetsReady,
}

/// <summary>
/// A package managing assets.
/// </summary>
[DataContract("Package")]
[NonIdentifiableCollectionItems]
[AssetDescription(PackageFileExtension)]
[DebuggerDisplay("Name: {Meta.Name}, Version: {Meta.Version}, Assets [{Assets.Count}]")]
[AssetFormatVersion("Assets", PackageFileVersion, "0.0.0.4")]
[AssetUpgrader("Assets", "0.0.0.4", "3.1.0.0", typeof(MovePackageInsideProject))]
public sealed partial class Package : IFileSynchronizable, IAssetFinder
{
    private const string PackageFileVersion = "3.1.0.0";

    internal readonly List<UFile> FilesToDelete = [];

    private UFile packagePath;
    internal UFile PreviousPackagePath;
    private bool isDirty;
    private readonly Lazy<PackageUserSettings> settings;

    /// <summary>
    /// Occurs when package dirty changed occurred.
    /// </summary>
    public event DirtyFlagChangedDelegate<Package> PackageDirtyChanged;

    /// <summary>
    /// Occurs when an asset dirty changed occurred.
    /// </summary>
    public event DirtyFlagChangedDelegate<AssetItem> AssetDirtyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Package"/> class.
    /// </summary>
    public Package()
    {
        // Initializse package with default versions (same code as in Asset..ctor())
        var defaultPackageVersion = AssetRegistry.GetCurrentFormatVersions(GetType());
        if (defaultPackageVersion is not null)
        {
            SerializedVersion = new Dictionary<string, PackageVersion>(defaultPackageVersion);
        }

        Assets = new PackageAssetCollection(this);
        Bundles = new BundleCollection(this);
        IsDirty = true;
        settings = new Lazy<PackageUserSettings>(() => new PackageUserSettings(this));
    }

    // Note: Please keep this code in sync with Asset class
    /// <summary>
    /// Gets or sets the version number for this asset, used internally when migrating assets.
    /// </summary>
    /// <value>The version.</value>
    [DataMember(-8000, DataMemberMode.Assign)]
    [DataStyle(DataStyle.Compact)]
    [Display(Browsable = false)]
    [DefaultValue(null)]
    [NonOverridable]
    [NonIdentifiableCollectionItems]
    public Dictionary<string, PackageVersion>? SerializedVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this package is a system package.
    /// </summary>
    /// <value><c>true</c> if this package is a system package; otherwise, <c>false</c>.</value>
    [DataMemberIgnore]
    public bool IsSystem => Container is not SolutionProject;

    /// <summary>
    /// Gets or sets the metadata associated with this package.
    /// </summary>
    /// <value>The meta.</value>
    [DataMember(10)]
    public PackageMeta Meta { get; set; } = new PackageMeta();

    /// <summary>
    /// Gets the asset directories to lookup.
    /// </summary>
    /// <value>The asset directories.</value>
    [DataMember(40, DataMemberMode.Assign)]
    public AssetFolderCollection AssetFolders { get; set; } = [];

    /// <summary>
    /// Gets the resource directories to lookup.
    /// </summary>
    /// <value>The resource directories.</value>
    [DataMember(45, DataMemberMode.Assign)]
    public List<UDirectory> ResourceFolders { get; set; } = [];

    /// <summary>
    /// Gets the output group directories.
    /// </summary>
    /// <value>The output group directories.</value>
    [DataMember(50, DataMemberMode.Assign)]
    public Dictionary<string, UDirectory> OutputGroupDirectories { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of folders that are explicitly created but contains no assets.
    /// </summary>
    [DataMember(70)]
    public List<UDirectory> ExplicitFolders { get; } = [];

    /// <summary>
    /// Gets the bundles defined for this package.
    /// </summary>
    /// <value>The bundles.</value>
    [DataMember(80)]
    public BundleCollection Bundles { get; private set; }

    /// <summary>
    /// Gets the template folders.
    /// </summary>
    /// <value>The template folders.</value>
    [DataMember(90)]
    public List<TemplateFolder> TemplateFolders { get; } = [];

    /// <summary>
    /// Asset references that needs to be compiled even if not directly or indirectly referenced (useful for explicit code references).
    /// </summary>
    [DataMember(100)]
    public RootAssetCollection RootAssets { get; private set; } = [];

    /// <summary>
    /// Gets the loaded templates from the <see cref="TemplateFolders"/>
    /// </summary>
    /// <value>The templates.</value>
    [DataMemberIgnore]
    public List<TemplateDescription> Templates { get; } = [];

    /// <summary>
    /// Gets the assets stored in this package.
    /// </summary>
    /// <value>The assets.</value>
    [DataMemberIgnore]
    public PackageAssetCollection Assets { get; }

    /// <summary>
    /// Gets the temporary assets list loaded from disk before they are going into <see cref="Assets"/>.
    /// </summary>
    /// <value>The temporary assets.</value>
    [DataMemberIgnore]
    // TODO: turn that internal!
    public List<AssetItem> TemporaryAssets { get; } = [];

    /// <summary>
    /// Gets the path to the package file. May be null if the package was not loaded or saved.
    /// </summary>
    /// <value>The package path.</value>
    [DataMemberIgnore]
    public UFile FullPath
    {
        get
        {
            return packagePath;
        }
        set
        {
            SetPackagePath(value, true);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance has been modified since last saving.
    /// </summary>
    /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
    [DataMemberIgnore]
    public bool IsDirty
    {
        get
        {
            return isDirty;
        }
        set
        {
            var oldValue = isDirty;
            isDirty = value;
            OnPackageDirtyChanged(this, oldValue, value);
        }
    }

    [DataMemberIgnore]
    public PackageState State { get; set; }

    /// <summary>
    /// Gets the top directory of this package on the local disk.
    /// </summary>
    /// <value>The top directory.</value>
    [DataMemberIgnore]
    public UDirectory? RootDirectory => FullPath?.GetParent();

    [DataMemberIgnore]
    public PackageContainer Container { get; internal set; }

    /// <summary>
    /// Gets the session.
    /// </summary>
    /// <value>The session.</value>
    /// <exception cref="InvalidOperationException">Cannot attach a package to more than one session</exception>
    [DataMemberIgnore]
    public PackageSession? Session => Container?.Session;

    /// <summary>
    /// Gets the package user settings. Usually stored in a .user file alongside the package. Lazily loaded on first time.
    /// </summary>
    /// <value>
    /// The package user settings.
    /// </value>
    [DataMemberIgnore]
    public PackageUserSettings UserSettings => settings.Value;

    /// <summary>
    /// Gets the list of assemblies loaded by this package.
    /// </summary>
    /// <value>
    /// The loaded assemblies.
    /// </value>
    [DataMemberIgnore]
    public List<PackageLoadedAssembly> LoadedAssemblies { get; } = [];

    [DataMemberIgnore]
    public string? RootNamespace { get; private set; }

    [DataMemberIgnore]
    public bool IsImplicitProject
    {
        get
        {
            // To keep in sync with LoadProject() .csproj
            // Note: Meta is ignored since it is supposedly "read-only" from csproj
            return AssetFolders.Count == 1 && AssetFolders[0].Path == "Assets"
                && ResourceFolders.Count == 1 && ResourceFolders[0] == "Resources"
                && OutputGroupDirectories.Count == 0
                && ExplicitFolders.Count == 0
                && Bundles.Count == 0
                && RootAssets.Count == 0
                && TemplateFolders.Count == 0;
        }
    }

    /// <summary>
    /// Adds an existing project to this package.
    /// </summary>
    /// <param name="pathToMsproj">The path to msproj.</param>
    /// <returns>LoggerResult.</returns>
    public LoggerResult AddExistingProject(UFile pathToMsproj)
    {
        var logger = new LoggerResult();
        AddExistingProject(pathToMsproj, logger);
        return logger;
    }

    /// <summary>
    /// Adds an existing project to this package.
    /// </summary>
    /// <param name="pathToMsproj">The path to msproj.</param>
    /// <param name="logger">The logger.</param>
    public void AddExistingProject(UFile pathToMsproj, LoggerResult logger)
    {
        ArgumentNullException.ThrowIfNull(pathToMsproj);
        ArgumentNullException.ThrowIfNull(logger);
        if (!pathToMsproj.IsAbsolute) throw new ArgumentException("Expecting relative path", nameof(pathToMsproj));

        try
        {
            // Load a project without specifying a platform to make sure we get the correct platform type
            var msProject = VSProjectHelper.LoadProject(pathToMsproj, platform: "NoPlatform");
            try
            {
                var projectType = VSProjectHelper.GetProjectTypeFromProject(msProject);
                if (!projectType.HasValue)
                {
                    logger.Error("This project is not a project created with the editor");
                }
                else
                {
                    var platformType = VSProjectHelper.GetPlatformTypeFromProject(msProject) ?? PlatformType.Shared;
                    var projectReference = new ProjectReference(VSProjectHelper.GetProjectGuid(msProject), pathToMsproj.MakeRelative(RootDirectory), projectType.Value);

                    // TODO CSPROJ=XKPKG
                    throw new NotImplementedException();
                    // Add the ProjectReference only for the compatible profiles (same platform or no platform)
                    //foreach (var profile in Profiles.Where(profile => platformType == profile.Platform))
                    //{
                    //    profile.ProjectReferences.Add(projectReference);
                    //}
                }
            }
            finally
            {
                msProject.ProjectCollection.UnloadAllProjects();
                msProject.ProjectCollection.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected exception while loading project [{pathToMsproj}]", ex);
        }
    }

    /// <inheritdoc />
    /// <remarks>Looks for the asset amongst the current package and its dependencies.</remarks>
    public AssetItem? FindAsset(AssetId assetId)
    {
        return this.GetPackagesWithDependencies().Select(p => p.Assets.Find(assetId)).NotNull().FirstOrDefault();
    }

    /// <inheritdoc />
    /// <remarks>Looks for the asset amongst the current package and its dependencies.</remarks>
    public AssetItem? FindAsset(UFile location)
    {
        return this.GetPackagesWithDependencies().Select(p => p.Assets.Find(location)).NotNull().FirstOrDefault();
    }

    /// <inheritdoc />
    /// <remarks>Looks for the asset amongst the current package and its dependencies.</remarks>
    public AssetItem? FindAssetFromProxyObject(object? proxyObject)
    {
        var attachedReference = AttachedReferenceManager.GetAttachedReference(proxyObject);
        return attachedReference is not null ? this.FindAsset(attachedReference) : null;
    }

    public UDirectory GetDefaultAssetFolder()
    {
        var folder = AssetFolders.FirstOrDefault();
        return folder?.Path ?? ("Assets");
    }

    /// <summary>
    /// Deep clone this package.
    /// </summary>
    /// <returns>The package cloned.</returns>
    public Package Clone()
    {
        // Use a new ShadowRegistry to copy override parameters
        // Clone this asset
        var package = AssetCloner.Clone(this);
        package.FullPath = FullPath;
        foreach (var asset in Assets)
        {
            var newAsset = asset.Asset;
            var assetItem = new AssetItem(asset.Location, newAsset)
            {
                SourceFolder = asset.SourceFolder,
                AlternativePath = asset.AlternativePath,
            };
            package.Assets.Add(assetItem);
        }
        return package;
    }

    /// <summary>
    /// Sets the package path.
    /// </summary>
    /// <param name="newPath">The new path.</param>
    /// <param name="copyAssets">if set to <c>true</c> assets will be copied relatively to the new location.</param>
    public void SetPackagePath(UFile newPath, bool copyAssets = true)
    {
        var previousPath = packagePath;
        var previousRootDirectory = RootDirectory;
        packagePath = newPath;
        if (packagePath?.IsAbsolute == false)
        {
            packagePath = UPath.Combine(Environment.CurrentDirectory, packagePath);
        }

        if (copyAssets && packagePath != previousPath)
        {
            // Update source folders
            var currentRootDirectory = RootDirectory;
            if (previousRootDirectory is not null && currentRootDirectory is not null)
            {
                foreach (var sourceFolder in AssetFolders)
                {
                    if (sourceFolder.Path.IsAbsolute)
                    {
                        var relativePath = sourceFolder.Path.MakeRelative(previousRootDirectory);
                        sourceFolder.Path = UPath.Combine(currentRootDirectory, relativePath);
                    }
                }
            }

            foreach (var asset in Assets)
            {
                asset.IsDirty = true;
            }
            IsDirty = true;
        }
    }

    internal void OnPackageDirtyChanged(Package package, bool oldValue, bool newValue)
    {
        ArgumentNullException.ThrowIfNull(package);
        PackageDirtyChanged?.Invoke(package, oldValue, newValue);
    }

    internal void OnAssetDirtyChanged(AssetItem asset, bool oldValue, bool newValue)
    {
        ArgumentNullException.ThrowIfNull(asset);
        AssetDirtyChanged?.Invoke(asset, oldValue, newValue);
    }

    public static bool SaveSingleAsset(AssetItem asset, ILogger log)
    {
        // Make sure AssetItem.SourceFolder/Project are generated if they were null
        asset.UpdateSourceFolders();
        return SaveSingleAsset_NoUpdateSourceFolder(asset, log);
    }

    internal static bool SaveSingleAsset_NoUpdateSourceFolder(AssetItem asset, ILogger log)
    {
        var assetPath = asset.FullPath;

        try
        {
            // Handle the ProjectSourceCodeAsset differently then regular assets in regards of Path
            if (asset.Asset is IProjectAsset projectAsset)
            {
                assetPath = asset.FullPath;
            }

            // Inject a copy of the base into the current asset when saving
            AssetFileSerializer.Save((string)assetPath, (object)asset.Asset, (AttachedYamlAssetMetadata)asset.YamlMetadata, log);

            // Save generated asset (if necessary)
            if (asset.Asset is IProjectFileGeneratorAsset codeGeneratorAsset)
            {
                codeGeneratorAsset.SaveGeneratedAsset(asset);
            }

            asset.IsDirty = false;
        }
        catch (Exception ex)
        {
            log.Error(asset.Package, asset.ToReference(), AssetMessageCode.AssetCannotSave, ex, assetPath);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the package identifier from file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>Guid.</returns>
    /// <exception cref="ArgumentNullException">
    /// log
    /// or
    /// filePath
    /// </exception>
    public static Guid GetPackageIdFromFile(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        bool hasPackage = false;
        using (var reader = new StreamReader(stream))
        {
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (line.StartsWith("!Package", StringComparison.Ordinal))
                {
                    hasPackage = true;
                }

                if (hasPackage && line.StartsWith("Id:", StringComparison.Ordinal))
                {
                    var id = line["Id:".Length..].Trim();
                    return Guid.Parse(id);
                }
            }
        }
        throw new IOException($"File {filePath} doesn't appear to be a valid package");
    }

    /// <summary>
    /// Loads only the package description but not assets or plugins.
    /// </summary>
    /// <param name="log">The log to receive error messages.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="loadParametersArg">The load parameters argument.</param>
    /// <returns>A package.</returns>
    /// <exception cref="ArgumentNullException">log
    /// or
    /// filePath</exception>
    public static Package? Load(ILogger log, string filePath, PackageLoadParameters? loadParametersArg = null)
    {
        var package = LoadProject(log, filePath)?.Package;

        if (package?.LoadAssembliesAndAssets(log, loadParametersArg) == false)
        {
            package = null;
        }

        return package;
    }

    /// <summary>
    /// Performs first part of the loading sequence, by deserializing the package but without processing anything yet.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">
    /// log
    /// or
    /// filePath
    /// </exception>
    internal static Package LoadRaw(ILogger log, string filePath)
    {
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(filePath);

        filePath = FileUtility.GetAbsolutePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Package file [{filePath}] was not found");
        }

        try
        {
            var packageFile = new PackageLoadingAssetFile(filePath, Path.GetDirectoryName(filePath)) { CachedFileSize = filePath.Length };
            var context = new AssetMigrationContext(null, null, filePath, log);
            AssetMigration.MigrateAssetIfNeeded(context, packageFile, "Assets");

            var loadResult = packageFile.AssetContent is not null
                ? AssetFileSerializer.Load<Package>(new MemoryStream(packageFile.AssetContent), filePath, log)
                : AssetFileSerializer.Load<Package>(filePath, log);
            var package = loadResult.Asset;
            package.FullPath = packageFile.FilePath;
            package.PreviousPackagePath = packageFile.OriginalFilePath;
            package.IsDirty = packageFile.AssetContent is not null || loadResult.AliasOccurred;

            return package;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error while pre-loading package [{filePath}]", ex);
        }
    }

    public static PackageContainer LoadProject(ILogger log, string filePath)
    {
        if (SupportedProgrammingLanguages.IsProjectExtensionSupported(Path.GetExtension(filePath).ToLowerInvariant()))
        {
            var projectPath = filePath;
            var packagePath = Path.ChangeExtension(filePath, Package.PackageFileExtension);
            var packageExists = File.Exists(packagePath);

            // Xenko to Stride migration
            if (!packageExists)
            {
                var oldPackagePath = Path.ChangeExtension(filePath, ".xkpkg");
                if (File.Exists(oldPackagePath))
                {
                    packageExists = true;
                    XenkoToStrideRenameHelper.RenameStrideFile(oldPackagePath, XenkoToStrideRenameHelper.StrideContentType.Package);
                }
            }

            var package = packageExists
                ? LoadRaw(log, packagePath)
                : new Package
                {
                    Meta = { Name = Path.GetFileNameWithoutExtension(packagePath) },
                    AssetFolders = { new AssetFolder("Assets") },
                    ResourceFolders = { "Resources" },
                    FullPath = packagePath,
                    IsDirty = false,
                };
            return new SolutionProject(package, Guid.NewGuid(), projectPath) { IsImplicitProject = !packageExists };
        }
        else
        {
            var package = LoadRaw(log, filePath);

            // Find the .csproj next to .sdpkg (if any)
            // Note that we use package.FullPath since we must first perform package upgrade from 3.0 to 3.1+ (might move package in .csproj folder)
            var projectPath = Path.ChangeExtension(package.FullPath.ToOSPath(), ".csproj");
            if (File.Exists(projectPath))
            {
                return new SolutionProject(package, Guid.NewGuid(), projectPath);
            }
            else
            {
                // Try to get version from NuGet folder
                var path = new UFile(filePath);
                var nuspecPath = UPath.Combine(path.GetFullDirectory().GetParent(), new UFile(path.GetFileNameWithoutExtension() + ".nuspec"));
                if (path.GetFullDirectory().GetDirectoryName() == "stride" && File.Exists(nuspecPath)
                    && PackageVersion.TryParse(path.GetFullDirectory().GetParent().GetDirectoryName(), out var packageVersion))
                {
                    package.Meta.Version = packageVersion;
                }
                return new StandalonePackage(package);
            }
        }
    }

    private static PackageVersion? TryGetPackageVersion(string projectPath)
    {
        try
        {
            // Load a project without specifying a platform to make sure we get the correct platform type
            var msProject = VSProjectHelper.LoadProject(projectPath, platform: "NoPlatform");
            try
            {
                var packageVersion = msProject.GetPropertyValue("PackageVersion");
                return !string.IsNullOrEmpty(packageVersion) ? new PackageVersion(packageVersion) : null;
            }
            finally
            {
                msProject.ProjectCollection.UnloadAllProjects();
                msProject.ProjectCollection.Dispose();
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Second part of the package loading process, when references, assets and package analysis is done.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="loadParametersArg">The load parameters argument.</param>
    /// <returns></returns>
    internal bool LoadAssembliesAndAssets(ILogger log, PackageLoadParameters? loadParametersArg)
    {
        return LoadAssemblies(log, loadParametersArg) && LoadAssets(log, loadParametersArg);
    }

    /// <summary>
    /// Load only assembly references
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="loadParametersArg">The load parameters argument.</param>
    /// <returns></returns>
    internal bool LoadAssemblies(ILogger log, PackageLoadParameters? loadParametersArg)
    {
        var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

        try
        {
            // Load assembly references
            if (loadParameters.LoadAssemblyReferences)
            {
                LoadAssemblyReferencesForPackage(log, loadParameters);
            }
            return true;
        }
        catch (Exception ex)
        {
            log.Error($"Error while pre-loading package [{FullPath}]", ex);

            return false;
        }
    }

    /// <summary>
    /// Load assets and perform package analysis.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="loadParametersArg">The load parameters argument.</param>
    /// <returns></returns>
    internal bool LoadAssets(ILogger log, PackageLoadParameters? loadParametersArg)
    {
        var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

        try
        {
            // Load assets
            if (loadParameters.AutoLoadTemporaryAssets)
            {
                LoadTemporaryAssets(log, loadParameters.AssetFiles, loadParameters.TemporaryAssetsInMsbuild, loadParameters.TemporaryAssetFilter, loadParameters.CancelToken ?? default);
            }

            // Convert UPath to absolute
            if (loadParameters.ConvertUPathToAbsolute)
            {
                var analysis = new PackageAnalysis(this, new PackageAnalysisParameters()
                {
                    ConvertUPathTo = UPathType.Absolute,
                    IsProcessingUPaths = true, // This is done already by Package.Load
                    SetDirtyFlagOnAssetWhenFixingAbsoluteUFile = true // When loading tag attributes that have an absolute file
                });
                analysis.Run(log);
            }

            // Load templates
            LoadTemplates(log);

            return true;
        }
        catch (Exception ex)
        {
            log.Error($"Error while pre-loading package [{FullPath}]", ex);

            return false;
        }
    }

    public void ValidateAssets(bool alwaysGenerateNewAssetId, bool removeUnloadableObjects, ILogger log)
    {
        if (TemporaryAssets.Count == 0)
        {
            return;
        }

        try
        {
            // Make sure we are suspending notifications before updating all assets
            Assets.SuspendCollectionChanged();

            Assets.Clear();

            // Get generated output items
            var outputItems = new List<AssetItem>();

            // Create a resolver from the package
            var resolver = AssetResolver.FromPackage(this);
            resolver.AlwaysCreateNewId = alwaysGenerateNewAssetId;

            // Clean assets
            AssetCollision.Clean(this, TemporaryAssets, outputItems, resolver, true, removeUnloadableObjects);

            // Add them back to the package
            foreach (var item in outputItems)
            {
                Assets.Add(item);

                // Fix collection item ids
                AssetCollectionItemIdHelper.GenerateMissingItemIds(item.Asset);
                CollectionItemIdsAnalysis.FixupItemIds(item, log);

                // Fix duplicate identifiable objects
                var hasBeenModified = IdentifiableObjectAnalysis.Visit(item.Asset, true, log);
                if (hasBeenModified)
                    item.IsDirty = true;
            }

            // Don't delete SourceCodeAssets as their files are handled by the package upgrader
            var dirtyAssets = outputItems.Where(static o => o.IsDirty && o.Asset is not SourceCodeAsset)
                .Join(TemporaryAssets, o => o.Id, t => t.Id, (_, t) => t)
                .ToList();
            // Dirty assets (except in system package) should be mark as deleted so that are properly saved again later.
            if (!IsSystem && dirtyAssets.Count > 0)
            {
                IsDirty = true;

                lock (FilesToDelete)
                {
                    FilesToDelete.AddRange(dirtyAssets.Select(a => a.FullPath));
                }
            }

            TemporaryAssets.Clear();
        }
        finally
        {
            // Restore notification on assets
            Assets.ResumeCollectionChanged();
        }
    }

    /// <summary>
    /// Refreshes this package from the disk by loading or reloading all assets.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="assetFiles">The asset files (loaded from <see cref="ListAssetFiles"/> if null).</param>
    /// <param name="listAssetsInMsbuild">Specifies if we need to evaluate MSBuild files for assets.</param>
    /// <param name="filterFunc">A function that will filter assets loading</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A logger that contains error messages while refreshing.</returns>
    /// <exception cref="InvalidOperationException">Package RootDirectory is null
    /// or
    /// Package RootDirectory [{0}] does not exist.ToFormat(RootDirectory)</exception>
    public void LoadTemporaryAssets(ILogger log, List<PackageLoadingAssetFile>? assetFiles = null, bool listAssetsInMsbuild = true, Func<PackageLoadingAssetFile, bool>? filterFunc = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(log);

        // If FullPath is null, then we can't load assets from disk, just return
        if (FullPath is null)
        {
            log.Warning("Fullpath not set on this package");
            return;
        }

        // Clears the assets already loaded and reload them
        TemporaryAssets.Clear();

        // List all package files on disk
        if (assetFiles is null)
        {
            assetFiles = ListAssetFiles(this, listAssetsInMsbuild, false);
            // Sort them by size (to improve concurrency during load)
            assetFiles.Sort(PackageLoadingAssetFile.FileSizeComparer.Default);
        }

        var progressMessage = $"Loading Assets from Package [{FullPath.GetFileName()}]";

        // Display this message at least once if the logger does not log progress (And it shouldn't in this case)
        var loggerResult = log as LoggerResult;
        if (loggerResult?.IsLoggingProgressAsInfo != true)
        {
            log.Verbose(progressMessage);
        }

        // Update step counter for log progress
        var tasks = new List<Task>();
        for (int i = 0; i < assetFiles.Count; i++)
        {
            var assetFile = assetFiles[i];

            if (filterFunc is not null && !filterFunc(assetFile))
            {
                continue;
            }

            // Update the loading progress
            loggerResult?.Progress(progressMessage, i, assetFiles.Count);

            var task = Task.Factory.StartNew(() => LoadAsset(new AssetMigrationContext(this, assetFile.ToReference(), assetFile.FilePath.ToOSPath(), log), assetFile), cancellationToken);

            tasks.Add(task);
        }

        Task.WaitAll([.. tasks], cancellationToken);

        // DEBUG
        // StaticLog.Info("[{0}] Assets files loaded in {1}", assetFiles.Count, clock.ElapsedMilliseconds);

        if (cancellationToken.IsCancellationRequested)
        {
            log.Warning("Skipping loading assets. PackageSession.Load cancelled");
        }
    }

    private void LoadAsset(AssetMigrationContext context, PackageLoadingAssetFile assetFile)
    {
        var fileUPath = assetFile.FilePath;
        var sourceFolder = assetFile.SourceFolder;

        // Check if asset has been deleted by an upgrader
        if (assetFile.Deleted)
        {
            IsDirty = true;

            lock (FilesToDelete)
            {
                FilesToDelete.Add(assetFile.FilePath);
            }

            // Don't create temporary assets for files deleted during package upgrading
            return;
        }

        // An exception can occur here, so we make sure that loading a single asset is not going to break 
        // the loop
        try
        {
            AssetMigration.MigrateAssetIfNeeded(context, assetFile, "Stride");

            // Try to load only if asset is not already in the package or assetRef.Asset is null
            var assetPath = assetFile.AssetLocation;

            var assetFullPath = fileUPath.ToOSPath();
            var assetContent = assetFile.AssetContent;

            var asset = LoadAsset(context.Log, Meta.Name, assetFullPath, assetPath.ToOSPath(), assetContent, out var aliasOccurred, out var yamlMetadata);

            // Create asset item
            var assetItem = new AssetItem(assetPath, asset, this)
            {
                IsDirty = assetContent is not null || aliasOccurred,
                SourceFolder = sourceFolder.MakeRelative(RootDirectory),
                AlternativePath = assetFile.Link is not null ? assetFullPath : null,
            };
            yamlMetadata.CopyInto(assetItem.YamlMetadata);

            // Set the modified time to the time loaded from disk
            if (!assetItem.IsDirty)
                assetItem.ModifiedTime = File.GetLastWriteTime(assetFullPath);

            // TODO: Let's review that when we rework import process
            // Not fixing asset import anymore, as it was only meant for upgrade
            // However, it started to make asset dirty, for ex. when we create a new texture, choose a file and reload the scene later
            // since there was no importer id and base.
            //FixAssetImport(assetItem);

            // Add to temporary assets
            lock (TemporaryAssets)
            {
                TemporaryAssets.Add(assetItem);
            }
        }
        catch (Exception ex)
        {
            if (ex is YamlException yamlException)
            {
                var row = yamlException.Start.Line + 1;
                var column = yamlException.Start.Column;
            }

            var assetReference = new AssetReference(AssetId.Empty, fileUPath.FullPath);
            context.Log.Error(this, assetReference, AssetMessageCode.AssetLoadingFailed, ex, fileUPath, ex.Message);
        }
    }

    /// <summary>
    /// Loads the assembly references that were not loaded before.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="loadParametersArg">The load parameters argument.</param>
    public void UpdateAssemblyReferences(ILogger log, PackageLoadParameters? loadParametersArg = null)
    {
        if (State < PackageState.DependenciesReady)
            return;

        var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();
        LoadAssemblyReferencesForPackage(log, loadParameters);
    }

    private static Asset LoadAsset(ILogger log, string packageName, string assetFullPath, string assetPath, byte[] assetContent, out bool assetDirty, out AttachedYamlAssetMetadata yamlMetadata)
    {
        var loadResult = assetContent is not null
            ? AssetFileSerializer.Load<Asset>(new MemoryStream(assetContent), assetFullPath, log)
            : AssetFileSerializer.Load<Asset>(assetFullPath, log);

        assetDirty = loadResult.AliasOccurred;
        yamlMetadata = loadResult.YamlMetadata;

        // Set location on source code asset
        if (loadResult.Asset is SourceCodeAsset sourceCodeAsset)
        {
            // Use an id generated from the location instead of the default id
            sourceCodeAsset.Id = SourceCodeAsset.GenerateIdFromLocation(packageName, assetPath);
        }

        return loadResult.Asset;
    }

    private void LoadAssemblyReferencesForPackage(ILogger log, PackageLoadParameters loadParameters)
    {
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(loadParameters);
        var assemblyContainer = loadParameters.AssemblyContainer ?? AssemblyContainer.Default;

        // Load from package
        if (Container is StandalonePackage standalonePackage)
        {
            foreach (var assemblyPath in standalonePackage.Assemblies)
            {
                LoadAssemblyReferenceInternal(log, loadParameters, assemblyContainer, null, assemblyPath);
            }
        }

        // Load from csproj
        if (Container is SolutionProject project && project.FullPath is not null && project.Type == ProjectType.Library)
        {
            // Check if already loaded
            // TODO: More advanced cases: unload removed references, etc...
            var projectReference = new ProjectReference(project.Id, project.FullPath, Core.Assets.ProjectType.Library);

            LoadAssemblyReferenceInternal(log, loadParameters, assemblyContainer, projectReference, project.TargetPath);
        }
    }

    private void LoadAssemblyReferenceInternal(ILogger log, PackageLoadParameters loadParameters, AssemblyContainer assemblyContainer, ProjectReference? projectReference, string assemblyPath)
    {
        try
        {
            // Check if already loaded
            if (projectReference is not null && LoadedAssemblies.Any(x => x.ProjectReference == projectReference))
                return;
            else if (LoadedAssemblies.Any(x => string.Compare(x.Path, assemblyPath, true) == 0))
                return;

            var forwardingLogger = new ForwardingLoggerResult(log);

            // If csproj, we might need to compile it
            if (projectReference is not null)
            {
                var fullProjectLocation = projectReference.Location.ToOSPath();
                if (loadParameters.AutoCompileProjects || string.IsNullOrWhiteSpace(assemblyPath))
                {
                    assemblyPath = VSProjectHelper.GetOrCompileProjectAssembly(fullProjectLocation, forwardingLogger, "Build", loadParameters.AutoCompileProjects, loadParameters.BuildConfiguration, extraProperties: loadParameters.ExtraCompileProperties, onlyErrors: true);
                    if (string.IsNullOrWhiteSpace(assemblyPath))
                    {
                        log.Error($"Unable to locate assembly reference for project [{fullProjectLocation}]");
                        return;
                    }
                }
            }

            var loadedAssembly = new PackageLoadedAssembly(projectReference, assemblyPath);
            LoadedAssemblies.Add(loadedAssembly);

            if (!File.Exists(assemblyPath) || forwardingLogger.HasErrors)
            {
                log.Error($"Unable to build assembly reference [{assemblyPath}]");
                return;
            }

            // Check if assembly is already loaded in appdomain (for Stride core assemblies that are not plugins)
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => string.Equals(x.GetName().Name, Path.GetFileNameWithoutExtension(assemblyPath), StringComparison.InvariantCultureIgnoreCase));

            // Otherwise, load assembly from its file
            if (assembly is null)
            {
                assembly = assemblyContainer.LoadAssemblyFromPath(assemblyPath, log);

                if (assembly is null)
                {
                    log.Error($"Unable to load assembly reference [{assemblyPath}]");
                }

                // Note: we should investigate so that this can also be done for Stride core assemblies (right now they use module initializers)
                if (assembly is not null)
                {
                    // Register assembly in the registry
                    AssemblyRegistry.Register(assembly, AssemblyCommonCategories.Assets);
                }
            }

            loadedAssembly.Assembly = assembly;
        }
        catch (Exception ex)
        {
            log.Error($"Unexpected error while loading assembly reference [{assemblyPath}]", ex);
        }
    }

    /// <summary>
    /// In case <see cref="AssetItem.SourceFolder"/> was null, generates it.
    /// </summary>
    internal void UpdateSourceFolders(IReadOnlyCollection<AssetItem> assets)
    {
        // If there are not assets, we don't need to update or create an asset folder
        if (assets.Count == 0)
        {
            return;
        }

        // Use by default the first asset folders if not defined on the asset item
        var defaultFolder = AssetFolders.Count > 0 ? AssetFolders[0].Path : UDirectory.This;
        var assetFolders = new HashSet<UDirectory>(GetDistinctAssetFolderPaths());
        foreach (var asset in assets)
        {
            if (asset.Asset is IProjectAsset)
            {
                if (asset.SourceFolder is null)
                {
                    asset.SourceFolder = string.Empty;
                    asset.IsDirty = true;
                }
            }
            else
            {
                if (asset.SourceFolder is null)
                {
                    asset.SourceFolder = defaultFolder.IsAbsolute ? defaultFolder.MakeRelative(RootDirectory) : defaultFolder;
                    asset.IsDirty = true;
                }

                var assetFolderAbsolute = UPath.Combine(RootDirectory, asset.SourceFolder);
                if (assetFolders.Add(assetFolderAbsolute))
                {
                    AssetFolders.Add(new AssetFolder(assetFolderAbsolute));
                    IsDirty = true;
                }
            }
        }
    }

    /// <summary>
    /// Loads the templates.
    /// </summary>
    /// <param name="log">The log result.</param>
    private void LoadTemplates(ILogger log)
    {
        foreach (var templateDir in TemplateFolders)
        {
            foreach (var filePath in templateDir.Files)
            {
                try
                {
                    var file = new FileInfo(filePath);
                    if (!file.Exists)
                    {
                        log.Warning($"Template [{file}] does not exist ");
                        continue;
                    }

                    var templateDescription = YamlSerializer.Load<TemplateDescription>(file.FullName);
                    templateDescription.FullPath = file.FullName;
                    Templates.Add(templateDescription);
                }
                catch (Exception ex)
                {
                    log.Error($"Error while loading template from [{filePath}]", ex);
                }
            }
        }
    }

    private List<UDirectory> GetDistinctAssetFolderPaths()
    {
        var existingAssetFolders = new List<UDirectory>();
        foreach (var folder in AssetFolders)
        {
            var folderPath = RootDirectory is not null ? UPath.Combine(RootDirectory, folder.Path) : folder.Path;
            if (!existingAssetFolders.Contains(folderPath))
            {
                existingAssetFolders.Add(folderPath);
            }
        }
        return existingAssetFolders;
    }

    public static List<PackageLoadingAssetFile> ListAssetFiles(Package package, bool listAssetsInMsbuild, bool listUnregisteredAssets)
    {
        var listFiles = new List<PackageLoadingAssetFile>();

        // TODO Check how to handle refresh correctly as a public API
        if (package.RootDirectory is null)
        {
            throw new InvalidOperationException("Package RootDirectory is null");
        }

        if (!Directory.Exists(package.RootDirectory))
        {
            return listFiles;
        }

        // Iterate on each source folders
        foreach (var sourceFolder in package.GetDistinctAssetFolderPaths())
        {
            // Lookup all files
            foreach (var directory in FileUtility.EnumerateDirectories(sourceFolder, SearchDirection.Down))
            {
                foreach (var filePath in directory.GetFiles())
                {
                    // Don't load package via this method
                    if (filePath.FullName.EndsWith(PackageFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Make an absolute path from the root of this package
                    var fileUPath = new UFile(filePath.FullName);
                    if (fileUPath.GetFileExtension() is null)
                    {
                        continue;
                    }

                    // If this kind of file an asset file?
                    var ext = fileUPath.GetFileExtension();
                    // Adjust extensions for Stride rename
                    ext = ext?.Replace(".xk", ".sd");

                    //make sure to add default shaders in this case, since we don't have a csproj for them
                    if (AssetRegistry.IsProjectCodeGeneratorAssetFileExtension(ext) && (package.Container is not SolutionProject || package.IsSystem))
                    {
                        listFiles.Add(new PackageLoadingAssetFile(fileUPath, sourceFolder) { CachedFileSize = filePath.Length });
                        continue;
                    }

                    //project source code assets follow the csproj pipeline
                    var isAsset = listUnregisteredAssets
                        ? ext?.StartsWith(".sd", StringComparison.InvariantCultureIgnoreCase) ?? false
                        : AssetRegistry.IsAssetFileExtension(ext);
                    if (!isAsset || AssetRegistry.IsProjectAssetFileExtension(ext))
                    {
                        continue;
                    }

                    var loadingAsset = new PackageLoadingAssetFile(fileUPath, sourceFolder) { CachedFileSize = filePath.Length };
                    listFiles.Add(loadingAsset);
                }
            }
        }

        //find also assets in the csproj
        if (listAssetsInMsbuild)
        {
            FindAssetsInProject(listFiles, package);
        }

        // Adjust extensions for Stride rename
        foreach (var loadingAsset in listFiles)
        {
            var originalExt = loadingAsset.FilePath.GetFileExtension() ?? "";
            var ext = originalExt.Replace(".xk", ".sd");
            if (ext != originalExt)
            {
                loadingAsset.FilePath = new UFile(loadingAsset.FilePath.FullPath.Replace(".xk", ".sd"));
            }
        }

        return listFiles;
    }

    public static List<(UFile FilePath, UFile? Link)> FindAssetsInProject(string projectFullPath, out string? nameSpace)
    {
        var realFullPath = new UFile(projectFullPath);
        var project = VSProjectHelper.LoadProject(realFullPath);
        var dir = new UDirectory(realFullPath.GetFullDirectory());

        nameSpace = project.GetPropertyValue("RootNamespace");
        if (nameSpace?.Length == 0)
            nameSpace = null;

        var result = project.Items.Where(x => (x.ItemType == "Compile" || x.ItemType == "None") && string.IsNullOrEmpty(x.GetMetadataValue("AutoGen")))
            // Build full path for Include and Link
            .Select(x => (FilePath: UPath.Combine(dir, new UFile(x.EvaluatedInclude)), Link: x.HasMetadata("Link") ? UPath.Combine(dir, new UFile(x.GetMetadataValue("Link"))) : null))
            // For items outside project, let's pretend they are link
            .Select(x => (x.FilePath, Link: x.Link ?? (!dir.Contains(x.FilePath) ? x.FilePath.GetFileName() : null)))
            // Test both Stride and Xenko extensions
            .Where(x =>
                AssetRegistry.IsProjectAssetFileExtension(x.FilePath.GetFileExtension())
                || AssetRegistry.IsProjectAssetFileExtension(x.FilePath.GetFileExtension()?.Replace(".xk", ".sd")))
            // avoid duplicates otherwise it might save a single file as separte file with renaming
            // had issues with case such as Effect.sdsl being registered twice (with glob pattern) and being saved as Effect.sdsl and Effect (2).sdsl
            .Distinct()
            .ToList();

        project.ProjectCollection.UnloadAllProjects();
        project.ProjectCollection.Dispose();

        return result;
    }

    private static void FindAssetsInProject(ICollection<PackageLoadingAssetFile> list, Package package)
    {
        if (package.IsSystem) return;

        if (package.Container is not SolutionProject project || project.FullPath is null)
            return;

        var projectAssets = FindAssetsInProject(project.FullPath, out var defaultNamespace);
        package.RootNamespace = defaultNamespace;
        var projectDirectory = new UDirectory(project.FullPath.GetFullDirectory());

        foreach (var (FilePath, Link) in projectAssets)
        {
            list.Add(new PackageLoadingAssetFile(FilePath, projectDirectory) { Link = Link });
        }
    }

    private class MovePackageInsideProject : AssetUpgraderBase
    {
        protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
        {
            if (asset.Profiles is not null)
            {
                foreach (var profile in asset.Profiles)
                {
                    if (profile.Platform == "Shared")
                    {
                        if (profile.ProjectReferences?.Count == 1)
                        {
                            var projectLocation = (UFile)(string)profile.ProjectReferences[0].Location;
                            assetFile.FilePath = UPath.Combine(assetFile.OriginalFilePath.GetFullDirectory(), (UFile)(projectLocation.GetFullPathWithoutExtension() + PackageFileExtension));
                            asset.Meta.Name = projectLocation.GetFileNameWithoutExtension();
                        }

                        if (profile.AssetFolders is not null)
                        {
                            for (int i = 0; i < profile.AssetFolders.Count; ++i)
                            {
                                var assetPath = UPath.Combine(assetFile.OriginalFilePath.GetFullDirectory(), (UDirectory)(string)profile.AssetFolders[i].Path);
                                assetPath = assetPath.MakeRelative(assetFile.FilePath.GetFullDirectory());
                                profile.AssetFolders[i].Path = (string)assetPath;
                            }
                        }

                        if (profile.ResourceFolders is not null)
                        {
                            for (int i = 0; i < profile.ResourceFolders.Count; ++i)
                            {
                                var resourcePath = UPath.Combine(assetFile.OriginalFilePath.GetFullDirectory(), (UDirectory)(string)profile.ResourceFolders[i]);
                                resourcePath = resourcePath.MakeRelative(assetFile.FilePath.GetFullDirectory());
                                profile.ResourceFolders[i] = (string)resourcePath;
                            }
                        }

                        asset.AssetFolders = profile.AssetFolders;
                        asset.ResourceFolders = profile.ResourceFolders;
                        asset.OutputGroupDirectories = profile.OutputGroupDirectories;
                    }
                }

                asset.Profiles = DynamicYamlEmpty.Default;
            }
        }
    }
}
