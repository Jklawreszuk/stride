// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Stride.Core.Annotations;
using Stride.Core.Diagnostics;
using Stride.Core.Extensions;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Controls
{
    /// <summary>
    /// This control displays a collection of <see cref="ILogMessage"/>.
    /// </summary>
    [TemplatePart(Name = "PART_LogTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ClearLog", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PreviousResult", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NextResult", Type = typeof(Button))]
    public class TextLogViewer : TemplatedControl
    {
        private readonly List<TextRange> searchMatches = [];
        private int currentResult;

        /// <summary>
        /// The <see cref="TextBox"/> in which the log messages are actually displayed.
        /// </summary>
        private TextBox logTextBox;

        /// <summary>
        /// Identifies the <see cref="LogMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty LogMessagesProperty = AvaloniaProperty.Register<TextLogViewer, ICollection<ILogMessage>>("LogMessages");

        /// <summary>
        /// Identifies the <see cref="AutoScroll"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty AutoScrollProperty = AvaloniaProperty.Register<TextLogViewer, bool>("AutoScroll");

        /// <summary>
        /// Identifies the <see cref="IsToolBarVisible"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsToolBarVisibleProperty = AvaloniaProperty.Register<TextLogViewer, bool>("IsToolBarVisible");

        /// <summary>
        /// Identifies the <see cref="CanClearLog"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CanClearLogProperty = AvaloniaProperty.Register<TextLogViewer, bool>("CanClearLog", true);

        /// <summary>
        /// Identifies the <see cref="CanFilterLog"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CanFilterLogProperty = AvaloniaProperty.Register<TextLogViewer, bool>("CanFilterLog", true);

        /// <summary>
        /// Identifies the <see cref="CanSearchLog"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CanSearchLogProperty = AvaloniaProperty.Register<TextLogViewer, bool>("CanSearchLog", true);

        /// <summary>
        /// Identifies the <see cref="SearchToken"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SearchTokenProperty = AvaloniaProperty.Register<TextLogViewer, string>("SearchToken", "");

        /// <summary>
        /// Identifies the <see cref="SearchMatchCase"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SearchMatchCaseProperty = AvaloniaProperty.Register<TextLogViewer, bool>("SearchMatchCase");

        /// <summary>
        /// Identifies the <see cref="SearchMatchWord"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SearchMatchWordProperty = AvaloniaProperty.Register<TextLogViewer, bool>("SearchMatchWord");

        /// <summary>
        /// Identifies the <see cref="SearchMatchBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SearchMatchBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("SearchMatchBrush", Brushes.LightSteelBlue);

        /// <summary>
        /// Identifies the <see cref="DebugBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty DebugBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("DebugBrush", Brushes.White);

        /// <summary>
        /// Identifies the <see cref="VerboseBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty VerboseBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("VerboseBrush", Brushes.White);

        /// <summary>
        /// Identifies the <see cref="InfoBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty InfoBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("InfoBrush", Brushes.White);

        /// <summary>
        /// Identifies the <see cref="WarningBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WarningBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("WarningBrush", Brushes.White);

        /// <summary>
        /// Identifies the <see cref="ErrorBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ErrorBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("ErrorBrush", Brushes.White);

        /// <summary>
        /// Identifies the <see cref="FatalBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty FatalBrushProperty = AvaloniaProperty.Register<TextLogViewer, IBrush>("FatalBrush", Brushes.White);

        /// <summary>
        /// Identifies the <see cref="ShowDebugMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowDebugMessagesProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowDebugMessages", true);

        /// <summary>
        /// Identifies the <see cref="ShowVerboseMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowVerboseMessagesProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowVerboseMessages", true);

        /// <summary>
        /// Identifies the <see cref="ShowInfoMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowInfoMessagesProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowInfoMessages", true);

        /// <summary>
        /// Identifies the <see cref="ShowWarningMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowWarningMessagesProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowWarningMessages", true);

        /// <summary>
        /// Identifies the <see cref="ShowErrorMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowErrorMessagesProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowErrorMessages", true);

        /// <summary>
        /// Identifies the <see cref="ShowFatalMessages"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowFatalMessagesProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowFatalMessages", true);

        /// <summary>
        /// Identifies the <see cref="ShowStacktrace"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ShowStacktraceProperty = AvaloniaProperty.Register<TextLogViewer, bool>("ShowStacktrace", true);

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLogViewer"/> class.
        /// </summary>
        public TextLogViewer()
        {
            Loaded += (s, e) =>
            {
                try
                {
                    if (AutoScroll)
                    {
                        var scrollViewer = logTextBox?.GetVisualDescendants()
                            .OfType<ScrollViewer>()
                            .FirstOrDefault();
                        scrollViewer?.ScrollToEnd();
                    }
                }
                catch (Exception ex)
                {
                    // It happened a few times that ScrollToEnd throws an exception that crashes the whole application.
                    // Let's ignore it if this happens again.
                    ex.Ignore();
                }
            };
            LogMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(LogMessagesPropertyChanged);
            SearchTokenProperty.Changed.AddClassHandler<AvaloniaObject>(SearchTokenChanged);
            SearchMatchCaseProperty.Changed.AddClassHandler<AvaloniaObject>(SearchTokenChanged);
            SearchMatchBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            DebugBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            VerboseBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            InfoBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            WarningBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ErrorBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            FatalBrushProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowDebugMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowVerboseMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowInfoMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowWarningMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowErrorMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowFatalMessagesProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
            ShowStacktraceProperty.Changed.AddClassHandler<AvaloniaObject>(TextPropertyChanged);
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="ILogMessage"/> to display.
        /// </summary>
        public ICollection<ILogMessage> LogMessages { get { return (ICollection<ILogMessage>)GetValue(LogMessagesProperty); } set { SetValue(LogMessagesProperty, value); } }

        /// <summary>
        /// Gets or sets whether the control should automatically scroll when new lines are added when the scrollbar is already at the bottom.
        /// </summary>
        public bool AutoScroll { get { return (bool)GetValue(AutoScrollProperty); } set { SetValue(AutoScrollProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the tool bar should be visible.
        /// </summary>
        public bool IsToolBarVisible { get { return (bool)GetValue(IsToolBarVisibleProperty); } set { SetValue(IsToolBarVisibleProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether it is possible to clear the log text.
        /// </summary>
        public bool CanClearLog { get { return (bool)GetValue(CanClearLogProperty); } set { SetValue(CanClearLogProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether it is possible to filter the log text.
        /// </summary>
        public bool CanFilterLog { get { return (bool)GetValue(CanFilterLogProperty); } set { SetValue(CanFilterLogProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether it is possible to search the log text.
        /// </summary>
        public bool CanSearchLog { get { return (bool)GetValue(CanSearchLogProperty); } set { SetValue(CanSearchLogProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets the current search token.
        /// </summary>
        public string SearchToken { get { return (string)GetValue(SearchTokenProperty); } set { SetValue(SearchTokenProperty, value); } }

        /// <summary>
        /// Gets or sets whether the search result should match the case.
        /// </summary>
        public bool SearchMatchCase { get { return (bool)GetValue(SearchMatchCaseProperty); } set { SetValue(SearchMatchCaseProperty, value); } }

        /// <summary>
        /// Gets or sets whether the search result should match whole words only.
        /// </summary>
        public bool SearchMatchWord { get { return (bool)GetValue(SearchMatchWordProperty); } set { SetValue(SearchMatchWordProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize search results.
        /// </summary>
        public Brush SearchMatchBrush { get { return (Brush)GetValue(SearchMatchBrushProperty); } set { SetValue(SearchMatchBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize debug messages.
        /// </summary>
        public Brush DebugBrush { get { return (Brush)GetValue(DebugBrushProperty); } set { SetValue(DebugBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize verbose messages.
        /// </summary>
        public Brush VerboseBrush { get { return (Brush)GetValue(VerboseBrushProperty); } set { SetValue(VerboseBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize info messages.
        /// </summary>
        public Brush InfoBrush { get { return (Brush)GetValue(InfoBrushProperty); } set { SetValue(InfoBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize warning messages.
        /// </summary>
        public Brush WarningBrush { get { return (Brush)GetValue(WarningBrushProperty); } set { SetValue(WarningBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize error messages.
        /// </summary>
        public Brush ErrorBrush { get { return (Brush)GetValue(ErrorBrushProperty); } set { SetValue(ErrorBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the brush used to emphasize fatal messages.
        /// </summary>
        public Brush FatalBrush { get { return (Brush)GetValue(FatalBrushProperty); } set { SetValue(FatalBrushProperty, value); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display debug messages.
        /// </summary>
        public bool ShowDebugMessages { get { return (bool)GetValue(ShowDebugMessagesProperty); } set { SetValue(ShowDebugMessagesProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display verbose messages.
        /// </summary>
        public bool ShowVerboseMessages { get { return (bool)GetValue(ShowVerboseMessagesProperty); } set { SetValue(ShowVerboseMessagesProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display info messages.
        /// </summary>
        public bool ShowInfoMessages { get { return (bool)GetValue(ShowInfoMessagesProperty); } set { SetValue(ShowInfoMessagesProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display warning messages.
        /// </summary>
        public bool ShowWarningMessages { get { return (bool)GetValue(ShowWarningMessagesProperty); } set { SetValue(ShowWarningMessagesProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display error messages.
        /// </summary>
        public bool ShowErrorMessages { get { return (bool)GetValue(ShowErrorMessagesProperty); } set { SetValue(ShowErrorMessagesProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display fatal messages.
        /// </summary>
        public bool ShowFatalMessages { get { return (bool)GetValue(ShowFatalMessagesProperty); } set { SetValue(ShowFatalMessagesProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the log viewer should display fatal messages.
        /// </summary>
        public bool ShowStacktrace { get { return (bool)GetValue(ShowStacktraceProperty); } set { SetValue(ShowStacktraceProperty, value.Box()); } }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            logTextBox =  e.NameScope.Find<TextBox>("PART_LogTextBox");
            if (logTextBox == null)
                throw new InvalidOperationException("A part named 'PART_LogTextBox' must be present in the ControlTemplate, and must be of type 'TextBox'.");

            var clearLogButton =  e.NameScope.Find<Button>("PART_ClearLog");
            if (clearLogButton != null)
            {
                clearLogButton.Click += ClearLog;
            }

            var previousResultButton =  e.NameScope.Find<Button>("PART_PreviousResult");
            if (previousResultButton != null)
            {
                previousResultButton.Click += PreviousResultClicked;
            }
            var nextResultButton =  e.NameScope.Find<Button>("PART_NextResult");
            if (nextResultButton != null)
            {
                nextResultButton.Click += NextResultClicked;
            }

            ResetText();
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            LogMessages.Clear();
        }

        private void ResetText()
        {
            if (logTextBox == null)
            {
                return;
            }
            
            logTextBox.Clear();
            ClearSearchResults();
            if (LogMessages != null)
            {
                var logMessages = LogMessages.ToList();
                AppendText(logMessages);
            }
        }

        private void AppendText([NotNull] IEnumerable<ILogMessage> logMessages)
        {
            if (logMessages == null) throw new ArgumentNullException(nameof(logMessages)); 
            if (logTextBox == null) return;

            var stringComparison = SearchMatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var searchToken = SearchToken;
            var sb = new StringBuilder();
            foreach (var message in logMessages.Where(x => ShouldDisplayMessage(x.Type)))
            {
                sb.Clear();

                if (message.Module != null)
                {
                    sb.AppendFormat("[{0}]: ", message.Module);
                }

                sb.AppendFormat("{0}: {1}", message.Type, message.Text);
                    
                var ex = message.ExceptionInfo;
                if (ex != null)
                {
                    if (ShowStacktrace)
                    {
                        sb.AppendFormat("{0}{1}{0}", Environment.NewLine, ex);
                    }
                    else
                    {
                        sb.Append(" (...)");
                    }
                }

                sb.AppendLine();

                var lineText = sb.ToString();
                var logColor = GetLogColor(message.Type);
                
                if (string.IsNullOrEmpty(searchToken))
                {
                    logTextBlock.Inlines.Add(new Run(lineText) { Foreground = logColor });
                }
                else
                {
                    do
                    {
                        var tokenIndex = lineText.IndexOf(searchToken, stringComparison);
                        if (tokenIndex == -1)
                        {
                            logTextBlock.Inlines.Add(new Run(lineText) { Foreground = logColor });
                            break;
                        }
                        var acceptResult = true;
                        if (SearchMatchWord && lineText.Length > 1)
                        {
                            if (tokenIndex > 0)
                            {
                                var c = lineText[tokenIndex - 1];
                                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                                    acceptResult = false;
                            }
                            if (tokenIndex + searchToken.Length < lineText.Length)
                            {
                                var c = lineText[tokenIndex + searchToken.Length];
                                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                                    acceptResult = false;
                            }
                        }

                        if (acceptResult)
                        {
                            if (tokenIndex > 0)
                                logTextBlock.Inlines.Add(new Run(lineText.Substring(0, tokenIndex)) { Foreground = logColor });

                            var tokenRun = new Run(lineText.Substring(tokenIndex, searchToken.Length)) { Background = SearchMatchBrush, Foreground = logColor };
                            logTextBlock.Inlines.Add(tokenRun);
                            var tokenRange = new TextRange(tokenIndex, searchToken.Length);
                            searchMatches.Add(tokenRange);
                            lineText = lineText.Substring(tokenIndex + searchToken.Length);
                        }
                    } while (lineText.Length > 0);
                }
            }
        }


        private void ClearSearchResults()
        {
            searchMatches.Clear();
        }

        private void SelectFirstOccurrence()
        {
            if (searchMatches.Count > 0)
            {
                SelectSearchResult(0);
            }
        }

        private void SelectPreviousOccurrence()
        {
            if (searchMatches.Count > 0)
            {
                var previousResult = (searchMatches.Count + currentResult - 1) % searchMatches.Count;
                SelectSearchResult(previousResult);
            }
        }

        private void SelectNextOccurrence()
        {
            if (searchMatches.Count > 0)
            {
                var nextResult = (currentResult + 1) % searchMatches.Count;
                SelectSearchResult(nextResult);
            }
        }

        private void SelectSearchResult(int resultIndex)
        {
            var result = searchMatches[resultIndex];
            logTextBlock.Selection.Select(result.Start, result.End);
            var selectionRect = logTextBlock.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
            var offset = selectionRect.Top + logTextBlock.VerticalOffset;
            logTextBlock.ScrollToVerticalOffset(offset - logTextBlock.Bounds.Height / 2);
            logTextBlock.BringIntoView();
            currentResult = resultIndex;
        }

        private bool ShouldDisplayMessage(LogMessageType type)
        {
            return type switch
            {
                LogMessageType.Debug => ShowDebugMessages,
                LogMessageType.Verbose => ShowVerboseMessages,
                LogMessageType.Info => ShowInfoMessages,
                LogMessageType.Warning => ShowWarningMessages,
                LogMessageType.Error => ShowErrorMessages,
                LogMessageType.Fatal => ShowFatalMessages,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        private Brush GetLogColor(LogMessageType type)
        {
            return type switch
            {
                LogMessageType.Debug => DebugBrush,
                LogMessageType.Verbose => VerboseBrush,
                LogMessageType.Info => InfoBrush,
                LogMessageType.Warning => WarningBrush,
                LogMessageType.Error => ErrorBrush,
                LogMessageType.Fatal => FatalBrush,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        private static void TextPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var logViewer = (TextLogViewer)d;
            logViewer.ResetText();
            var scrollViewer = logViewer.logTextBlock?
                .GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();
            
            scrollViewer?.ScrollToEnd();
        }

        /// <summary>
        /// Raised when the <see cref="LogMessages"/> dependency property is changed.
        /// </summary>
        private static void LogMessagesPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var logViewer = (TextLogViewer)d;
            var newValue = e.NewValue as ICollection<ILogMessage>;
            if (e.OldValue is ICollection<ILogMessage> oldValue)
            {
                if (oldValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged -= logViewer.LogMessagesCollectionChanged;
                }
            }
            if (e.NewValue != null)
            {
                if (newValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged += logViewer.LogMessagesCollectionChanged;
                }
            }
            logViewer.ResetText();
        }

        /// <summary>
        /// Raised when the <see cref="SearchToken"/> property is changed.
        /// </summary>
        private static void SearchTokenChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var logViewer = (TextLogViewer)d;
            logViewer.ResetText();
            logViewer.SelectFirstOccurrence();
        }

        /// <summary>
        /// Raised when the collection of log messages is observable and changes.
        /// </summary>
        private void LogMessagesCollectionChanged(object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            var shouldScroll = AutoScroll && logTextBlock != null && logTextBlock.ExtentHeight - logTextBlock.ViewportHeight - logTextBlock.VerticalOffset < 1.0;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    if (logTextBlock != null)
                    {
                        AppendText(e.NewItems.Cast<ILogMessage>());
                    }
                }
            }
            else
            {
                ResetText();
            }

            if (shouldScroll)
            {
                // Sometimes crashing with ExecutionEngineException in Window.GetWindowMinMax() if not ran with a dispatcher low priority.
                // Note: priority should still be higher than DispatcherPriority.Input so that user input have a chance to scroll.
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var scrollViewer = logTextBlock?.GetVisualDescendants()
                        .OfType<ScrollViewer>()
                        .FirstOrDefault();
                    scrollViewer?.ScrollToEnd();
                }, DispatcherPriority.DataBind);
            }
        }

        private void PreviousResultClicked(object sender, RoutedEventArgs e)
        {
            SelectPreviousOccurrence();
        }

        private void NextResultClicked(object sender, RoutedEventArgs e)
        {
            SelectNextOccurrence();
        }
    }
}
