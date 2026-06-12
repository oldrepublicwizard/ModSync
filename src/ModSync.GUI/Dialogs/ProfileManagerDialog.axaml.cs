// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services.Profiles;
using ModSync.Services;

namespace ModSync.Dialogs
{
    public partial class ProfileManagerDialog : Window
    {
        [NotNull]
        private readonly ProfileService _profileService;

        public ProfileManagerDialog()
        {
            InitializeComponent();
            ThemeManager.ApplyCurrentToWindow(this);

            // Same settings directory that SettingsManager uses for settings.json;
            // ProfileService stores its files in a "profiles" subdirectory of it.
            string settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModSync"
            );
            _profileService = new ProfileService(settingsDirectory);

            RefreshProfiles();
            UpdateButtonStates();
        }

        public static async Task ShowProfileManagerDialogAsync([NotNull] Window parentWindow)
        {
            if (parentWindow is null)
            {
                throw new ArgumentNullException(nameof(parentWindow));
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new ProfileManagerDialog();
                await dialog.ShowDialog(parentWindow);
            });
        }

        [CanBeNull]
        private Profile SelectedProfile => ProfilesListBox?.SelectedItem as Profile;

        private void RefreshProfiles()
        {
            try
            {
                List<Profile> profiles = _profileService.ListProfiles();
                ProfilesListBox.ItemsSource = profiles;
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to list profiles: {ex.Message}");
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = SelectedProfile != null;
            ActivateButton.IsEnabled = hasSelection;
            CloneButton.IsEnabled = hasSelection;
            RenameButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        }

        private void SetStatus([CanBeNull] string message) => StatusTextBlock.Text = message ?? string.Empty;

        private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateButtonStates();

        private async void SaveCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = await TextInputDialog.ShowTextInputDialogAsync(
                    this,
                    prompt: "Enter a name for the new profile:",
                    title: "Save Current As Profile"
                );
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                string instructionFilePath = (Owner as MainWindow)?.LastLoadedInstructionFileName;
                Profile profile = _profileService.CaptureFromCurrentState(name, MainConfig.AllComponents, instructionFilePath);
                RefreshProfiles();
                SetStatus($"Saved profile '{profile.Name}' from current state.");
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[ProfileManagerDialog] Failed to save current state as profile");
                SetStatus($"Failed to save profile: {ex.Message}");
            }
        }

        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = SelectedProfile;
            if (profile is null)
            {
                return;
            }

            try
            {
                _profileService.ApplyProfile(profile, MainConfig.AllComponents);
                RefreshProfiles();
                SetStatus($"Activated profile '{profile.Name}'. Directories and mod selections were applied.");
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[ProfileManagerDialog] Failed to activate profile");
                SetStatus($"Failed to activate profile: {ex.Message}");
            }
        }

        private async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = SelectedProfile;
            if (profile is null)
            {
                return;
            }

            try
            {
                string newName = await TextInputDialog.ShowTextInputDialogAsync(
                    this,
                    prompt: $"Enter a name for the copy of '{profile.Name}':",
                    title: "Clone Profile",
                    defaultText: profile.Name + " (copy)"
                );
                if (string.IsNullOrWhiteSpace(newName))
                {
                    return;
                }

                Profile clone = _profileService.CloneProfile(profile, newName);
                RefreshProfiles();
                SetStatus($"Cloned '{profile.Name}' to '{clone.Name}'.");
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[ProfileManagerDialog] Failed to clone profile");
                SetStatus($"Failed to clone profile: {ex.Message}");
            }
        }

        private async void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = SelectedProfile;
            if (profile is null)
            {
                return;
            }

            try
            {
                string newName = await TextInputDialog.ShowTextInputDialogAsync(
                    this,
                    prompt: $"Enter a new name for '{profile.Name}':",
                    title: "Rename Profile",
                    defaultText: profile.Name
                );
                if (string.IsNullOrWhiteSpace(newName) || string.Equals(newName, profile.Name, StringComparison.Ordinal))
                {
                    return;
                }

                Profile renamed = _profileService.RenameProfile(profile.Name, newName);
                RefreshProfiles();
                SetStatus($"Renamed profile to '{renamed.Name}'.");
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[ProfileManagerDialog] Failed to rename profile");
                SetStatus($"Failed to rename profile: {ex.Message}");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = SelectedProfile;
            if (profile is null)
            {
                return;
            }

            try
            {
                bool? confirm = await ConfirmationDialog.ShowConfirmationDialogAsync(
                    this,
                    confirmText: $"Delete profile '{profile.Name}'? This cannot be undone.",
                    yesButtonText: "Delete",
                    noButtonText: "Cancel"
                );
                if (confirm != true)
                {
                    return;
                }

                _ = _profileService.DeleteProfile(profile.Name);
                RefreshProfiles();
                UpdateButtonStates();
                SetStatus($"Deleted profile '{profile.Name}'.");
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[ProfileManagerDialog] Failed to delete profile");
                SetStatus($"Failed to delete profile: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
