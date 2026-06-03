// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using JetBrains.Annotations;
using KOTORModSync.Core;
using KOTORModSync.Core.Services;
using KOTORModSync.Core.Services.FileSystem;
using KOTORModSync.Core.Services.Validation;
using KOTORModSync.Services;

namespace KOTORModSync.Dialogs.WizardPages
{
    public partial class ValidatePage : WizardPageBase
    {
        private readonly List<ModComponent> _allComponents;
        private readonly MainConfig _mainConfig;
        private StackPanel _resultsPanel;
        private ScrollViewer _resultsScrollViewer;
        private ProgressBar _validationProgress;
        private TextBlock _statusText;
        private TextBlock _summaryText;
        private TextBlock _summaryDetails;
        private TextBlock _errorCountBadge;
        private TextBlock _warningCountBadge;
        private TextBlock _passedCountBadge;
        private Button _validateButton;
        private Expander _logExpander;
        private ScrollViewer _logScrollViewer;
        private TextBlock _logText;
        private TextBlock _logProgressText;
        private Button _copyReportButton;
        private readonly List<(string Title, string Message)> _resultEntries = new List<(string Title, string Message)>();
        private bool _hasValidated;
        private bool _hasCriticalErrors;
        private int _errorCount;
        private int _warningCount;
        private int _passedCount;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private readonly Queue<string> _logQueue = new Queue<string>();
        private readonly object _logLock = new object();
        private DispatcherTimer _logUpdateTimer;
        private DateTime _validationStartTime;
        private int _currentStep;
        private int _totalSteps;
        private string _currentOperation = string.Empty;

        public ValidatePage()
            : this(new List<ModComponent>(), new MainConfig())
        {
        }

        public ValidatePage([NotNull][ItemNotNull] List<ModComponent> allComponents, [NotNull] MainConfig mainConfig)
        {
            _allComponents = allComponents ?? throw new ArgumentNullException(nameof(allComponents));
            _mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));

            InitializeComponent();
            CacheControls();
            HookEvents();
            InitializeLogUpdateTimer();
        }

        public override string Title => "Validation";

        public override string Subtitle => "Validating your mod selection and installation environment";

        public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
        {
            int selected = _allComponents.Count(c => c.IsSelected);
            int total = _allComponents.Count;

            if (_summaryDetails != null)
            {
                _summaryDetails.Text = selected == 0
                    ? "No mods are selected. Go back to Mod Selection and choose mods to install."
                    : $"Ready to validate {selected} of {total} mod(s). Click Run Validation to check your setup.";
            }

            if (_summaryText != null)
            {
                _summaryText.Text = selected == 0
                    ? "Select mods before validating"
                    : "Click 'Run Validation' to begin";
            }

            if (_validateButton != null)
            {
                _validateButton.IsEnabled = selected > 0;
            }

            return Task.CompletedTask;
        }

        public override Task<(bool isValid, string errorMessage)> ValidateAsync(CancellationToken cancellationToken)
        {
            int selectedCount = _allComponents.Count(c => c.IsSelected);
            if (selectedCount == 0)
            {
                return Task.FromResult((false, "No mods are selected. Go back to Mod Selection and choose mods to install."));
            }

            if (!_hasValidated)
            {
                return Task.FromResult((false, "Please run validation before continuing."));
            }

            if (_hasCriticalErrors)
            {
                return Task.FromResult((false, "Critical errors detected. Please resolve them before continuing."));
            }

            return Task.FromResult((true, (string)null));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private void CacheControls()
        {
            _resultsPanel = this.FindControl<StackPanel>("ResultsPanel");
            _resultsScrollViewer = this.FindControl<ScrollViewer>("ResultsScrollViewer");
            _validationProgress = this.FindControl<ProgressBar>("ValidationProgress");
            _statusText = this.FindControl<TextBlock>("StatusText");
            _summaryText = this.FindControl<TextBlock>("SummaryText");
            _summaryDetails = this.FindControl<TextBlock>("SummaryDetails");
            _errorCountBadge = this.FindControl<TextBlock>("ErrorCountBadge");
            _warningCountBadge = this.FindControl<TextBlock>("WarningCountBadge");
            _passedCountBadge = this.FindControl<TextBlock>("PassedCountBadge");
            _validateButton = this.FindControl<Button>("ValidateButton");
            _logExpander = this.FindControl<Expander>("LogExpander");
            _logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
            _logText = this.FindControl<TextBlock>("LogText");
            _logProgressText = this.FindControl<TextBlock>("LogProgressText");
            _copyReportButton = this.FindControl<Button>("CopyReportButton");
        }

        private void HookEvents()
        {
            if (_validateButton != null)
            {
                _validateButton.Click += async (_, __) => await RunValidation();
            }

            if (_copyReportButton != null)
            {
                _copyReportButton.Click += CopyReportButton_Click;
            }
        }

        private void InitializeLogUpdateTimer()
        {
            _logUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100),
            };
            _logUpdateTimer.Tick += LogUpdateTimer_Tick;
            _logUpdateTimer.Start();
        }

        private void LogUpdateTimer_Tick(object sender, EventArgs e)
        {
            FlushLogQueue();
            UpdateLogHeader();
        }

        private void FlushLogQueue()
        {
            lock (_logLock)
            {
                if (_logQueue.Count == 0 || _logText == null)
                {
                    return;
                }

                while (_logQueue.Count > 0)
                {
                    string message = _logQueue.Dequeue();
                    _logBuilder.AppendLine(message);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    if (_logText != null)
                    {
                        _logText.Text = _logBuilder.ToString();
                    }

                    if (_logScrollViewer != null)
                    {
                        _logScrollViewer.ScrollToEnd();
                    }
                }, DispatcherPriority.Normal);
            }
        }

        private void UpdateLogHeader()
        {
            if (_logProgressText == null)
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                if (_logProgressText != null)
                {
                    var parts = new List<string>();

                    if (_totalSteps > 0 && _currentStep > 0)
                    {
                        double percentage = (_currentStep / (double)_totalSteps) * 100.0;
                        parts.Add($"{percentage:F0}%");
                    }

                    if (!string.IsNullOrEmpty(_currentOperation))
                    {
                        parts.Add(_currentOperation);
                    }

                    if (_validationStartTime != default && _currentStep > 0 && _totalSteps > 0)
                    {
                        TimeSpan elapsed = DateTime.UtcNow - _validationStartTime;
                        if (elapsed.TotalSeconds > 0 && _currentStep > 0)
                        {
                            double avgTimePerStep = elapsed.TotalSeconds / _currentStep;
                            int remainingSteps = _totalSteps - _currentStep;
                            if (remainingSteps > 0)
                            {
                                TimeSpan eta = TimeSpan.FromSeconds(avgTimePerStep * remainingSteps);
                                string etaText = eta.TotalSeconds < 60 ? $"{eta.TotalSeconds:F0}s" : $"{eta.TotalMinutes:F1}m";
                                parts.Add($"ETA: {etaText}");
                            }
                        }
                    }

                    _logProgressText.Text = parts.Count > 0 ? string.Join(" • ", parts) : string.Empty;
                }
            }, DispatcherPriority.Normal);
        }

        private void AppendLog(string message)
        {
            lock (_logLock)
            {
                _logQueue.Enqueue($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
            }
        }

        private void ClearLog()
        {
            lock (_logLock)
            {
                _logBuilder.Clear();
                _logQueue.Clear();
                Dispatcher.UIThread.Post(() =>
                {
                    if (_logText != null)
                    {
                        _logText.Text = string.Empty;
                    }
                }, DispatcherPriority.Normal);
            }
        }

        private async Task RunValidation()
        {
            int selectedCount = _allComponents.Count(c => c.IsSelected);
            if (selectedCount == 0)
            {
                if (_summaryText != null)
                {
                    _summaryText.Text = "No mods selected";
                }
                if (_summaryDetails != null)
                {
                    _summaryDetails.Text = "Go back to Mod Selection and choose at least one mod.";
                }
                return;
            }

            if (_validateButton != null)
            {
                _validateButton.IsEnabled = false;
            }

            if (_validationProgress != null)
            {
                _validationProgress.IsVisible = true;
            }

            if (_statusText != null)
            {
                _statusText.Text = "Running validation...";
            }

            if (_logExpander != null)
            {
                _logExpander.IsVisible = true;
            }

            _resultsPanel?.Children.Clear();
            _resultEntries.Clear();
            if (_copyReportButton != null)
            {
                _copyReportButton.IsVisible = false;
            }

            _hasCriticalErrors = false;
            _errorCount = 0;
            _warningCount = 0;
            _passedCount = 0;
            _currentStep = 0;
            _totalSteps = 0;
            _currentOperation = string.Empty;
            _validationStartTime = DateTime.UtcNow;

            ClearLog();
            AppendLog("Starting validation...");
            UpdateBadges();

            await Task.Delay(100);

            try
            {
                var selectedMods = _allComponents.Where(c => c.IsSelected).ToList();

                var pipelineOptions = ValidationPipelineOptions.WizardFull;
                pipelineOptions.MainConfig = _mainConfig;
                pipelineOptions.ConfirmationCallback = async msg =>
                {
                    await Task.CompletedTask;
                    AppendLog($"  {msg}");
                    return true;
                };

                ValidationPipelineResult pipelineResult = await InstallationValidationPipeline.RunAsync(
                    _allComponents,
                    pipelineOptions,
                    (stage, step, total, message) =>
                    {
                        _currentStep = step;
                        _totalSteps = total;
                        _currentOperation = message ?? string.Empty;
                        UpdateLogHeader();
                    }).ConfigureAwait(true);

                ApplyPipelineResultToWizardUi(pipelineResult, selectedMods);

                _currentStep = _totalSteps;
                _currentOperation = string.Empty;
                TimeSpan totalTime = DateTime.UtcNow - _validationStartTime;
                AppendLog($"Validation completed in {totalTime.TotalSeconds:F1} seconds");
                AppendLog(_hasCriticalErrors ? "❌ Validation failed" : "✅ Validation passed");
                UpdateLogHeader();

                UpdateBadges();
                UpdateSummary();

                if (_logExpander != null)
                {
                    _logExpander.IsExpanded = _hasCriticalErrors || _warningCount > 0;
                }

                if (_errorCount > 0 || _warningCount > 0)
                {
                    Dispatcher.UIThread.Post(ScrollToFirstIssueCard, DispatcherPriority.Loaded);
                }

                _hasValidated = true;
            }
            finally
            {
                if (_validationProgress != null)
                {
                    _validationProgress.IsVisible = false;
                }

                if (_validateButton != null)
                {
                    _validateButton.IsEnabled = true;
                }

                if (_copyReportButton != null && _hasValidated)
                {
                    _copyReportButton.IsVisible = true;
                }

                _currentOperation = string.Empty;
                UpdateLogHeader();
            }
        }

        private void ApplyPipelineResultToWizardUi(
            ValidationPipelineResult pipelineResult,
            List<ModComponent> selectedMods)
        {
            _errorCount = pipelineResult.ErrorCount;
            _warningCount = pipelineResult.WarningCount;
            _passedCount = pipelineResult.PassedCount;
            _hasCriticalErrors = pipelineResult.HasCriticalErrors;

            WizardValidationStagePresenter.ApplyStages(
                pipelineResult,
                selectedMods.Count,
                AppendLog,
                AddResult);
        }

        private void ScrollToFirstIssueCard()
        {
            if (_resultsPanel is null)
            {
                return;
            }

            Control target = FindFirstResultCard("❌") ?? (_warningCount > 0 ? FindFirstResultCard("⚠️") : null);
            target?.BringIntoView();
        }

        private Control FindFirstResultCard(string titlePrefix)
        {
            if (_resultsPanel is null)
            {
                return null;
            }

            foreach (Control child in _resultsPanel.Children)
            {
                if (!(child is Border border)
                    || !(border.Child is StackPanel panel)
                    || panel.Children.Count == 0)
                {
                    continue;
                }

                if (panel.Children[0] is TextBlock titleBlock
                    && titleBlock.Text?.StartsWith(titlePrefix, StringComparison.Ordinal) == true)
                {
                    return border;
                }
            }

            return null;
        }

        private void AddResult(string title, string message)
        {
            _resultEntries.Add((title, message));

            if (_resultsPanel is null)
            {
                return;
            }

            var border = new Border
            {
                Padding = new Avalonia.Thickness(16, 12),
                CornerRadius = new Avalonia.CornerRadius(8),
                Margin = new Avalonia.Thickness(0, 0, 0, 8),
            };

            var panel = new StackPanel
            {
                Spacing = 6,
            };

            panel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
            });

            panel.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 13,
                Opacity = 0.85,
                TextWrapping = TextWrapping.Wrap,
            });

            border.Child = panel;
            _resultsPanel.Children.Add(border);
        }

        private void UpdateBadges()
        {
            if (_errorCountBadge != null)
            {
                _errorCountBadge.Text = _errorCount.ToString();
            }

            if (_warningCountBadge != null)
            {
                _warningCountBadge.Text = _warningCount.ToString();
            }

            if (_passedCountBadge != null)
            {
                _passedCountBadge.Text = _passedCount.ToString();
            }
        }

        private void UpdateSummary()
        {
            if (_summaryText != null)
            {
                _summaryText.Text = _hasCriticalErrors
                    ? "❌ Validation Failed"
                    : "✅ Validation Passed";
            }

            if (_summaryDetails != null)
            {
                if (_hasCriticalErrors)
                {
                    _summaryDetails.Text = "Please resolve all critical errors before continuing. Check the results above for details.";
                }
                else if (_warningCount > 0)
                {
                    _summaryDetails.Text = $"Validation passed with {_warningCount} warning(s). These will be handled automatically during installation.";
                }
                else
                {
                    _summaryDetails.Text = "Everything looks good! You're ready to proceed with the installation.";
                }
            }

            if (_statusText != null)
            {
                _statusText.Text = _hasCriticalErrors
                    ? "Validation failed"
                    : "Validation complete";
            }
        }

        private async void CopyReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FlushLogQueue();
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard is null)
                {
                    return;
                }

                await topLevel.Clipboard.SetTextAsync(BuildValidationReportText());
                if (_copyReportButton != null)
                {
                    string original = _copyReportButton.Content?.ToString() ?? "Copy report";
                    _copyReportButton.Content = "Copied!";
                    await Task.Delay(1500);
                    _copyReportButton.Content = original;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to copy validation report: {ex.Message}");
            }
        }

        private string BuildValidationReportText()
        {
            var report = new StringBuilder();
            report.AppendLine("KOTORModSync — Validation Report");
            report.AppendLine($"Generated (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            if (_summaryText != null)
            {
                report.AppendLine(_summaryText.Text);
            }

            if (_summaryDetails != null && !string.IsNullOrWhiteSpace(_summaryDetails.Text))
            {
                report.AppendLine(_summaryDetails.Text);
            }

            report.AppendLine();
            report.AppendLine($"Errors: {_errorCount}  Warnings: {_warningCount}  Passed checks: {_passedCount}");

            if (_resultEntries.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("--- Results ---");
                foreach ((string title, string message) in _resultEntries)
                {
                    report.AppendLine(title);
                    report.AppendLine(message);
                    report.AppendLine();
                }
            }

            lock (_logLock)
            {
                if (_logBuilder.Length > 0)
                {
                    report.AppendLine("--- Log ---");
                    report.Append(_logBuilder.ToString());
                }
            }

            return report.ToString().TrimEnd();
        }
    }
}


