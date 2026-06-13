// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services.Fomod;
using ModSync.Core.Services.Fomod;

namespace ModSync.Dialogs
{
    public partial class FomodInstallerDialog : Window
    {
        private FomodInstallerSession _session;
        private IReadOnlyList<int> _visibleStepIndices;
        private int _visibleStepCursor;

        public FomodInstallerDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void InitializeSession([NotNull] FomodInstallerSession session)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            _session = session;
            _visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(session);
            _visibleStepCursor = 0;
            Title = string.IsNullOrWhiteSpace(session.Component.Name)
                ? "FOMOD Installer"
                : $"FOMOD Installer — {session.Component.Name}";
            RenderCurrentStep();
        }

        /// <summary>
        /// Shows the wizard for an extracted archive folder and returns the configured component when the user finishes.
        /// </summary>
        public static async Task<ModComponent> ShowForExtractedArchiveAsync(
            [NotNull] Window parentWindow,
            [NotNull] string extractedArchiveDirectory)
        {
            if (parentWindow is null)
            {
                throw new ArgumentNullException(nameof(parentWindow));
            }

            if (extractedArchiveDirectory is null)
            {
                throw new ArgumentNullException(nameof(extractedArchiveDirectory));
            }

            string moduleConfigPath = FomodArchiveDiscovery.FindModuleConfigPath(extractedArchiveDirectory);
            if (moduleConfigPath is null)
            {
                await InformationDialog.ShowInformationDialogAsync(
                    parentWindow,
                    "No fomod/ModuleConfig.xml was found in the selected folder.");
                return null;
            }

            FomodModuleConfig config = FomodParser.ParseModuleConfigXmlFile(moduleConfigPath);
            FomodInfo info = null;
            string infoPath = FomodArchiveDiscovery.FindInfoPath(extractedArchiveDirectory);
            if (infoPath != null)
            {
                info = FomodParser.ParseInfoXmlFile(infoPath);
            }

            string archiveFileName = Path.GetFileName(extractedArchiveDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) + ".zip";
            ModComponent component = FomodToComponentMapper.Map(info, config, archiveFileName);
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, component);

            var dialog = new FomodInstallerDialog();
            dialog.InitializeSession(session);
            bool? accepted = await dialog.ShowDialog<bool?>(parentWindow).ConfigureAwait(true);
            return accepted == true ? component : null;
        }

        private void RenderCurrentStep()
        {
            ValidationText.IsVisible = false;
            ValidationText.Text = string.Empty;
            StepContentPanel.Children.Clear();

            if (_visibleStepIndices.Count == 0)
            {
                StepTitleText.Text = "This FOMOD package has no optional install steps.";
                BackButton.IsEnabled = false;
                NextButton.Content = "Finish";
                return;
            }

            int stepIndex = _visibleStepIndices[_visibleStepCursor];
            FomodInstallerStepModel step = _session.Steps[stepIndex];
            StepTitleText.Text = string.IsNullOrWhiteSpace(step.Name)
                ? $"Step {_visibleStepCursor + 1} of {_visibleStepIndices.Count}"
                : $"{step.Name} — step {_visibleStepCursor + 1} of {_visibleStepIndices.Count}";

            for (int groupIndex = 0; groupIndex < step.Groups.Count; groupIndex++)
            {
                FomodInstallerGroupModel group = step.Groups[groupIndex];
                var groupPanel = new StackPanel { Spacing = 6 };
                groupPanel.Children.Add(new TextBlock { Text = group.Name, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

                for (int pluginIndex = 0; pluginIndex < group.Plugins.Count; pluginIndex++)
                {
                    FomodInstallerPluginModel plugin = group.Plugins[pluginIndex];
                    var checkBox = new CheckBox
                    {
                        Content = plugin.Name,
                        IsChecked = plugin.IsSelected,
                        IsEnabled = !plugin.IsRequired,
                    };
                    checkBox.Tag = new PluginSelectionTag(stepIndex, groupIndex, pluginIndex);
                    checkBox.IsCheckedChanged += PluginCheckBox_IsCheckedChanged;

                    groupPanel.Children.Add(checkBox);
                    if (!string.IsNullOrWhiteSpace(plugin.Description))
                    {
                        groupPanel.Children.Add(
                            new TextBlock
                            {
                                Text = plugin.Description,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                Margin = new Avalonia.Thickness(24, 0, 0, 0),
                            });
                    }
                }

                StepContentPanel.Children.Add(groupPanel);
            }

            BackButton.IsEnabled = _visibleStepCursor > 0;
            NextButton.Content = _visibleStepCursor >= _visibleStepIndices.Count - 1 ? "Finish" : "Next";
        }

        private void PluginCheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkBox) || !(checkBox.Tag is PluginSelectionTag tag))
            {
                return;
            }

            bool isChecked = checkBox.IsChecked == true;
            if (!FomodInstallerPresenter.TrySetPluginSelected(_session, tag.StepIndex, tag.GroupIndex, tag.PluginIndex, isChecked))
            {
                checkBox.IsChecked = true;
                return;
            }

            _visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(_session);
            if (_visibleStepCursor >= _visibleStepIndices.Count)
            {
                _visibleStepCursor = Math.Max(0, _visibleStepIndices.Count - 1);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_visibleStepCursor > 0)
            {
                _visibleStepCursor--;
                RenderCurrentStep();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_visibleStepIndices.Count == 0)
            {
                Close(true);
                return;
            }

            int stepIndex = _visibleStepIndices[_visibleStepCursor];
            string validationMessage = FomodInstallerPresenter.ValidateStep(_session, stepIndex);
            if (validationMessage != null)
            {
                ValidationText.Text = validationMessage;
                ValidationText.IsVisible = true;
                return;
            }

            if (_visibleStepCursor < _visibleStepIndices.Count - 1)
            {
                _visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(_session);
                _visibleStepCursor++;
                RenderCurrentStep();
                return;
            }

            foreach (int visibleStepIndex in _visibleStepIndices)
            {
                validationMessage = FomodInstallerPresenter.ValidateStep(_session, visibleStepIndex);
                if (validationMessage != null)
                {
                    ValidationText.Text = validationMessage;
                    ValidationText.IsVisible = true;
                    return;
                }
            }

            FomodInstallerPresenter.ApplySelectionsToComponent(_session);
            Close(true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private sealed class PluginSelectionTag
        {
            public PluginSelectionTag(int stepIndex, int groupIndex, int pluginIndex)
            {
                StepIndex = stepIndex;
                GroupIndex = groupIndex;
                PluginIndex = pluginIndex;
            }

            public int StepIndex { get; }

            public int GroupIndex { get; }

            public int PluginIndex { get; }
        }
    }
}
