// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ModSync.Core.Services.Download;

namespace ModSync.Dialogs
{
    public partial class SingleUrlDownloadDialog : Window
    {
        public SingleUrlDownloadDialog()
        {
            InitializeComponent();
            ThemeManager.ApplyCurrentToWindow(this);
        }

        public SingleUrlDownloadDialog(string modName, string url)
        {
            InitializeComponent();
            ThemeManager.ApplyCurrentToWindow(this);

            TextBlock modNameText = this.FindControl<TextBlock>("ModNameText");
            if (modNameText != null)
            {
                modNameText.Text = string.IsNullOrWhiteSpace(modName)
                    ? "Nexus Mod Manager Download"
                    : $"Downloading: {modName}";
            }

            TextBlock statusText = this.FindControl<TextBlock>("StatusText");
            if (statusText != null && !string.IsNullOrWhiteSpace(url))
            {
                statusText.Text = url;
            }
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        public void ApplyProgress(DownloadProgress progress)
        {
            if (progress is null)
            {
                return;
            }

            ProgressBar progressBar = this.FindControl<ProgressBar>("DownloadProgressBar");
            TextBlock statusText = this.FindControl<TextBlock>("StatusText");
            TextBlock detailText = this.FindControl<TextBlock>("DetailText");
            TextBlock footerText = this.FindControl<TextBlock>("FooterText");

            if (progressBar != null)
            {
                progressBar.Value = progress.ProgressPercentage;
            }

            if (statusText != null && !string.IsNullOrWhiteSpace(progress.StatusMessage))
            {
                statusText.Text = progress.StatusMessage;
            }

            if (detailText != null)
            {
                detailText.Text = FormatByteDetail(progress);
            }

            if (footerText != null)
            {
                footerText.Text = FormatFooter(progress);
            }
        }

        private static string FormatByteDetail(DownloadProgress progress)
        {
            if (progress.TotalBytes > 0)
            {
                return $"{FormatBytes(progress.BytesDownloaded)} / {FormatBytes(progress.TotalBytes)}";
            }

            if (progress.BytesDownloaded > 0)
            {
                return $"{FormatBytes(progress.BytesDownloaded)} downloaded";
            }

            return string.Empty;
        }

        private static string FormatFooter(DownloadProgress progress)
        {
            switch (progress.Status)
            {
                case DownloadStatus.Completed:
                    return "Download complete";
                case DownloadStatus.Failed:
                    return string.IsNullOrWhiteSpace(progress.ErrorMessage)
                        ? "Download failed"
                        : progress.ErrorMessage;
                case DownloadStatus.InProgress:
                    return "Download in progress…";
                default:
                    return "Preparing download…";
            }
        }

        private static string FormatBytes(long bytes)
        {
            const double kb = 1024d;
            const double mb = kb * 1024d;
            const double gb = mb * 1024d;

            if (bytes >= gb)
            {
                return $"{bytes / gb:0.##} GB";
            }

            if (bytes >= mb)
            {
                return $"{bytes / mb:0.##} MB";
            }

            if (bytes >= kb)
            {
                return $"{bytes / kb:0.##} KB";
            }

            return $"{bytes} B";
        }
    }
}
