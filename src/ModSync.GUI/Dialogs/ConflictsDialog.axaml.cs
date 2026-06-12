// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services.Conflicts;
using ModSync.Services;

namespace ModSync.Dialogs
{
    public partial class ConflictsDialog : Window
    {
        public ConflictsDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetSummary([NotNull] ConflictsDialogSummary summary)
        {
            if (summary is null)
            {
                throw new ArgumentNullException(nameof(summary));
            }

            SummaryText.Text = summary.SummaryLine;
            ConflictsList.ItemsSource = summary.ConflictRows;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Runs <see cref="FileConflictAnalyzer"/> for the given install order and shows results.
        /// </summary>
        public static async Task ShowAnalysisAsync(
            [NotNull] Window parentWindow,
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> componentsInInstallOrder)
        {
            if (parentWindow is null)
            {
                throw new ArgumentNullException(nameof(parentWindow));
            }

            if (componentsInInstallOrder is null)
            {
                throw new ArgumentNullException(nameof(componentsInInstallOrder));
            }

            ProgressDialog progressDialog = null;
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    progressDialog = new ProgressDialog(
                        "Analyzing file conflicts",
                        "Simulating install order to detect overlapping game-directory writes...");
                    progressDialog.Show(parentWindow);
                });

                ConflictAnalysisResult result = await new FileConflictAnalyzer()
                    .AnalyzeAsync(componentsInInstallOrder, CancellationToken.None)
                    .ConfigureAwait(true);

                Dictionary<Guid, string> nameLookup = new Dictionary<Guid, string>();
                foreach (ModComponent component in componentsInInstallOrder)
                {
                    if (component == null || component.Guid == Guid.Empty)
                    {
                        continue;
                    }

                    nameLookup[component.Guid] = component.Name ?? string.Empty;
                }

                ConflictsDialogSummary summary = ConflictsDialogPresenter.BuildSummary(result, nameLookup);

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    progressDialog?.Close();
                    var dialog = new ConflictsDialog();
                    dialog.SetSummary(summary);
                    await dialog.ShowDialog(parentWindow);
                });
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() => progressDialog?.Close());
            }
        }
    }
}
