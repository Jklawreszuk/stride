// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Setup.Configuration;
using Stride.Core.Extensions;
using Stride.Core.IO;

namespace Stride.Core.VisualStudio
{
    public class IDEInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IDEInfo"/> class.
        /// </summary>
        /// <param name="currentVersion">The version of the VS instance.</param>
        /// <param name="displayName">The display name of the VS instance</param>
        /// <param name="path">The path to the installation root of the IDE instance.</param>
        /// <param name="instanceId">The unique identifier for this installation instance.</param>
        /// <param name="isComplete">Indicates whehter the VS instance is complete.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public IDEInfo(Version currentVersion, string displayName, string path, string instanceId, bool isComplete = true)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            IsComplete = isComplete;

            var idePath = System.IO.Path.Combine(Path, "Common7", "IDE");
            ExecutablePath = System.IO.Path.Combine(idePath, "devenv.exe");
            if (!File.Exists(ExecutablePath))
            {
                ExecutablePath = null;
            }

            VsixInstallerPath = System.IO.Path.Combine(idePath, "VSIXInstaller.exe");
            if (!File.Exists(VsixInstallerPath))
            {
                VsixInstallerPath = null;
            }
        }

        public async Task<bool> StartOrToggle(UFile solutionPath)
        {
            if (!await CheckCanOpenSolution(solutionPath))
                return false;

            var process = await GetVisualStudio(solutionPath, true) ?? await StartVisualStudio(solutionPath);
            return process != null;
        }

        public async Task<Process> GetVisualStudio(UFile solutionPath, bool makeActive)
        {
            if (!await CheckCanOpenSolution(solutionPath))
                return null;

            try
            {
                // Try to find an existing instance of Visual Studio with this solution open.
                var process = FindVisualStudioInstance(solutionPath);

                if (process != null && makeActive)
                {
                    //int style = NativeHelper.GetWindowLong(process.MainWindowHandle, NativeHelper.GWL_STYLE);
                    //// Restore the window if it is minimized
                    //if ((style & NativeHelper.WS_MINIMIZE) == NativeHelper.WS_MINIMIZE)
                    //    NativeHelper.ShowWindow(process.MainWindowHandle, NativeHelper.SW_RESTORE);
                    //NativeHelper.SetForegroundWindow(process.MainWindowHandle);
                }

                return process;
            }
            catch (Exception e)
            {
                // This operation can fail silently
                e.Ignore();
                return null;
            }
        }

        public async Task<Process> StartVisualStudio(UFile solutionPath)
        {
            if (!await CheckCanOpenSolution(solutionPath))
                return null;

            var startInfo = new ProcessStartInfo();
            if (false)
            {
                //var defaultIDEName = EditorSettings.DefaultIDE.GetValue();

                //if (!EditorSettings.DefaultIDE.GetAcceptableValues().Contains(defaultIDEName))
                //    defaultIDEName = EditorSettings.DefaultIDE.DefaultValue;

                // ideInfo = VisualStudioVersions.AvailableVisualStudioInstances.FirstOrDefault(x => x.DisplayName == defaultIDEName) ?? VisualStudioVersions.DefaultIDE;
            }

            // It will be null if either "Default", or if not available anymore (uninstalled?)
            if (ExecutablePath != null && File.Exists(ExecutablePath))
            {
                startInfo.FileName = ExecutablePath;
                startInfo.Arguments = $"\"{solutionPath}\"";
            }
            else
            {
                startInfo.FileName = solutionPath.ToOSPath();
                startInfo.UseShellExecute = true;
            }
            try
            {
                return Process.Start(startInfo);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<bool> CheckCanOpenSolution(UFile solutionPath)
        {
            if (string.IsNullOrEmpty(solutionPath))
            {
                //await session.Dialogs.MessageBoxAsync(Tr._p("Message", "The session currently open is not a Visual Studio session."), MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private static Process FindVisualStudioInstance(UFile solutionPath)
        {
            // NOTE: this code is very hackish and does not 100% ensure that the correct instance of VS will be activated.
            var processes = Process.GetProcessesByName("devenv");
            foreach (var process in processes)
            {
                // Get instances that have a solution with the same name currently open (The solution name is displayed in the title bar).
                if (process.MainWindowTitle.StartsWith(solutionPath.GetFileNameWithoutExtension(), StringComparison.OrdinalIgnoreCase))
                {
                    // If there is a matching instance, get its command line.
                    var query = $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}";
                    using var managementObjectSearcher = new ManagementObjectSearcher(query);

                    var managementObject = managementObjectSearcher.Get().Cast<ManagementObject>().First();
                    var commandLine = managementObject["CommandLine"].ToString();
                    if (commandLine.Replace('/', '\\').Contains(solutionPath.ToString().Replace('/', '\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        return process;
                    }
                }
            }

            return null;
        }

        public static IDEInfo DefaultIDE = new IDEInfo(new Version("0.0"), "Default IDE", string.Empty, string.Empty);

        /// <summary>Gets a value indicating whether the instance is complete.</summary>
        /// <value>Whether the instance is complete.</value>
        /// <remarks>An instance is complete if it had no errors during install, resume, or repair.</remarks>
        public bool IsComplete { get; }

        /// <summary> 
        /// Gets the display name (title) of the product installed in this instance. 
        /// </summary>
        public string DisplayName { get; }

        /// <summary>Gets the version of the product installed in this instance.</summary>
        /// <value>The version of the product installed in this instance.</value>
        public Version CurrentVersion { get; }

        /// <summary>
        /// The path to the development environment executable of this IDE, or <c>null</c>.
        /// </summary>
        public string ExecutablePath { get; }

        /// <summary>The root installation path of this IDE.</summary>
        /// <remarks>Can be empty but not <c>null</c>./remarks>
        public string Path { get; }

        /// <summary>
        /// The hex code for this installation instance. It is used, for example, to create a unique folder in %LocalAppData%
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// The path to the VSIX installer of this IDE, or <c>null</c>.
        /// </summary>
        public string VsixInstallerPath { get; }

        /// <summary>
        /// The package names and versions of packages installed to this instance.
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> PackageVersions { get; } = [];

        /// <summary>
        /// <c>true</c> if this IDE has a development environment; otherwise, <c>false</c>.
        /// </summary>
        public bool HasDevenv => !string.IsNullOrEmpty(ExecutablePath);

        /// <summary>
        /// <c>true</c> if this IDE has a VSIX installer; otherwise, <c>false</c>.
        /// </summary>
        public bool HasVsixInstaller => !string.IsNullOrEmpty(VsixInstallerPath);

        /// <inheritdoc />
        public override string ToString() => DisplayName;
    }

    public static class VSCode
    {
        public static IDEInfo AvailableInstance = GetAvailableInstance();

        private static IDEInfo GetAvailableInstance()
        {
            string pathEnvVar = Environment.GetEnvironmentVariable("PATH");
            var pathDirectories = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? Array.Empty<string>();

            string toolLocation = null;
            foreach (var directory in pathDirectories)
            {
                toolLocation = Path.Combine(directory, "code");
            }
            var ideInfo = new IDEInfo(new(), "VS Code", toolLocation, "");

            return ideInfo;
        }
    }
    public static class VSCodium
    {
        public static IDEInfo AvailableInstance = GetAvailableInstance();

        private static IDEInfo GetAvailableInstance()
        {
            string pathEnvVar = Environment.GetEnvironmentVariable("PATH");
            var pathDirectories = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? Array.Empty<string>();

            string toolLocation = null;
            foreach (var directory in pathDirectories)
            {
                toolLocation = Path.Combine(directory, "codium");
            }
            var ideInfo = new IDEInfo(new(), "VS Codium", toolLocation, "");

            return ideInfo;
        }
    }

    public static class VisualStudioVersions
    {
        private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);
        private static readonly Lazy<List<IDEInfo>> IDEInfos = new Lazy<List<IDEInfo>>(BuildIDEInfos);

        /// <summary>
        /// Only lists VS2019+ (previous versions are not supported due to lack of buildTransitive targets).
        /// </summary>
        public static IEnumerable<IDEInfo> AvailableVisualStudioInstances => IDEInfos.Value.Where(x => x.CurrentVersion.Major >= 16 && x.HasDevenv);

        private static List<IDEInfo> BuildIDEInfos()
        {
            var ideInfos = new List<IDEInfo>();

            // Visual Studio 16.0 (2019) and later
            try
            {
                var setupInstancesEnum = new SetupConfiguration().EnumAllInstances();
                setupInstancesEnum.Reset();
                var inst = new ISetupInstance[1];

                while (true)
                {
                    setupInstancesEnum.Next(1, inst, out int numFetched);
                    if (numFetched <= 0)
                        break;

                    try
                    {
                        if (inst[0] is not ISetupInstance2 setupInstance2)
                            continue;

                        // Only examine VS2019+
                        if (!Version.TryParse(setupInstance2.GetInstallationVersion(), out var installationVersion)
                            || installationVersion.Major < 16)
                            continue;

                        var displayName = setupInstance2.GetDisplayName();
                        // Try to append nickname (if any)
                        try
                        {
                            var nickname = setupInstance2.GetProperties().GetValue("nickname") as string;
                            if (!string.IsNullOrEmpty(nickname))
                                displayName = $"{displayName} ({nickname})";
                            else
                            {
                                var installationName = setupInstance2.GetInstallationName();
                                // In case of Preview, we have:
                                // "installationName": "VisualStudioPreview/16.4.0-pre.6.0+29519.161"
                                // "channelId": "VisualStudio.16.Preview"
                                if (installationName.Contains("Preview"))
                                {
                                    displayName += " (Preview)";
                                }
                            }
                        }
                        catch (COMException)
                        {
                        }

                        try
                        {
                            var minimumRequiredState = InstanceState.Local | InstanceState.Registered;
                            if ((setupInstance2.GetState() & minimumRequiredState) != minimumRequiredState)
                                continue;
                        }
                        catch (COMException)
                        {
                            continue;
                        }

                        var ideInfo = new IDEInfo(installationVersion, displayName,
                            setupInstance2.GetInstallationPath(), setupInstance2.GetInstanceId(), setupInstance2.IsComplete());

                        // Fill packages
                        foreach (var package in setupInstance2.GetPackages())
                        {
                            ideInfo.PackageVersions[package.GetId()] = package.GetVersion();
                        }

                        ideInfos.Add(ideInfo);
                    }
                    catch (Exception)
                    {
                        // Something might have happened inside Visual Studio Setup code (had FileNotFoundException in GetInstallationPath() for example)
                        // Let's ignore this instance
                    }
                }
            }
            catch (COMException comException) when (comException.HResult == REGDB_E_CLASSNOTREG)
            {
                // COM is not registered. Assuming no instances are installed.
            }
            return ideInfos;
        }
    }
}
