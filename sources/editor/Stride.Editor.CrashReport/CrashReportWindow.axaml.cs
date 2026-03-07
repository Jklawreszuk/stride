// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Stride.Editor.CrashReport;

public partial class CrashReportWindow : Window
{
    public const string PrivacyPolicyUrl = "https://stride3d.net/legal/privacy-policy";
    private const string GithubIssuesUrl = "https://github.com/stride3d/stride/issues/new?labels=bug&template=bug_report.md";
    private readonly CrashReportData currentData;
    public string ApplicationName { get; }

    public CrashReportWindow(CrashReportData crashReport, string applicationName)
    {
        InitializeComponent();
        currentData = crashReport;
        textBoxLog.Text = crashReport.ToString();
        ApplicationName = applicationName;
        DataContext = this;
    }

    private bool Expanded { get; set { field = value; RefreshSize(); } } = false;

    private void RefreshSize()
    {
        if (!Expanded)
        {
            buttonViewLog.Content = "View report";
            textBoxLog.IsVisible = false;
        }
        else
        {
            buttonViewLog.Content = "Hide report";
            textBoxLog.IsVisible = true;
        }
    }

    private void RefreshReport()
    {
        textBoxLog.Text = currentData.ToString();
    }

    private void ButtonOpenGithubIssues_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process browser = new();
            browser.StartInfo.FileName = GithubIssuesUrl;
            browser.StartInfo.UseShellExecute = true;
            browser.Start();
        }
        catch (Exception)
        {
            var error = "An error occurred while opening the browser. You can access Github Issues at the following url:"
                        + Environment.NewLine + Environment.NewLine + GithubIssuesUrl;

            //MessageBox.Show(error, "Stride", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //DialogResult = true;
    }

    private void ButtonViewLog_Click(object sender, RoutedEventArgs e)
    {
        Expanded = !Expanded;
    }

    private async void ButtonCopyReport_Click(object sender, RoutedEventArgs e)
    {
        RefreshReport();
        await Clipboard.SetTextAsync(currentData.ToString());
    }

    private async void ButtonSaveReport_Click(object sender, RoutedEventArgs e)
    {
        RefreshReport();

        var fileDialog = new SaveFileDialog()
        {
            
            InitialFileName = "Report.txt",
            DefaultExtension = "txt",
            Filters = [ 
                new FileDialogFilter()
                {
                    Name = "Text files (*.txt)",
                    Extensions = [".txt"],
                },
                new FileDialogFilter()
                {
                    Name = "All files (*.*)",
                    Extensions = ["*.*"],
                },
            ]
        };

        if (await fileDialog.ShowAsync(this) == "true")
        {
            await File.WriteAllTextAsync(fileDialog.InitialFileName, currentData.ToString());
        }
    }
}
