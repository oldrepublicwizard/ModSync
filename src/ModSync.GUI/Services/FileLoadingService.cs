// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Parsing;
using ModSync.Dialogs;

namespace ModSync.Services
{
    public class FileLoadingService
    {
        private const int MaxInstructionSizeBytes = 524288000;

        private readonly MainConfig _mainConfig;
        private readonly Window _parentWindow;

        public string LastLoadedFileName { get; set; }

        public FileLoadingService(MainConfig mainConfig, Window parentWindow)
        {
            _mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
        }

        /// <summary>
        /// Loads a config file with auto-format detection (TOML, JSON, YAML, or embedded Markdown).
        /// Uses the Core FileLoadingService which auto-detects format based on content.
        /// </summary>
        public async Task<bool> LoadInstructionFileAsync(
            [NotNull] string filePath,
            bool editorMode,
            [NotNull] Func<Task> onComponentsLoaded,
            [NotNull] string fileType = "instruction file")
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MaxInstructionSizeBytes)
                {
#pragma warning disable MA0004 // Use Task.
                    await Logger.LogAsync($"Invalid {fileType} selected: '{fileInfo.Name}' - file too large");
#pragma warning restore MA0004 // Use Task.
                    return false;
                }

                // Auto-detect format (TOML/JSON/YAML/embedded-Markdown)
#pragma warning disable MA0004 // Use Task.
                List<ModComponent> newComponents = await Core.Services.FileLoadingService.LoadFromFileAsync(filePath);
#pragma warning restore MA0004 // Use Task.

                ProcessModLinks(newComponents);

#pragma warning disable MA0004 // Use Task.
                return await ApplyLoadedComponentsAsync(newComponents, editorMode, onComponentsLoaded, fileType, Path.GetFileName(filePath));
#pragma warning restore MA0004 // Use Task.
            }
            catch (Exception ex)
            {
#pragma warning disable MA0004 // Use Task.
                await Logger.LogExceptionAsync(ex);
#pragma warning restore MA0004 // Use Task.
                return false;
            }
        }

        /// <summary>
        /// Imports an instruction set or mod-build guide from raw text (e.g. pasted from the clipboard).
        /// Routes the text through the same content-sniffing cascade as file loading
        /// (TOML → Markdown → YAML → XML/JSON). Markdown guides additionally get draft instructions
        /// parsed from their natural-language prose (flagged for review, never auto-trusted).
        /// Unrecognized text degrades gracefully: nothing is imported and false is returned.
        /// </summary>
        public async Task<bool> LoadInstructionTextAsync(
            [CanBeNull] string content,
            bool editorMode,
            [NotNull] Func<Task> onComponentsLoaded,
            [NotNull] Func<List<ModComponent>, Task> tryAutoGenerate,
            [NotNull] string sourceDescription = "pasted text")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
#pragma warning disable MA0004 // Use Task.
                    await Logger.LogWarningAsync($"No text found in {sourceDescription}; nothing was imported.");
#pragma warning restore MA0004 // Use Task.
                    return false;
                }

                if (content.Length > MaxInstructionSizeBytes)
                {
#pragma warning disable MA0004 // Use Task.
                    await Logger.LogWarningAsync($"Text from {sourceDescription} is too large to import.");
#pragma warning restore MA0004 // Use Task.
                    return false;
                }

#pragma warning disable MA0004 // Use Task.
                string detectedFormat = await Task.Run(() => Core.Services.ModComponentSerializationService.DetectFormatFromContent(content));
#pragma warning restore MA0004 // Use Task.

                if (detectedFormat is null)
                {
#pragma warning disable MA0004 // Use Task.
                    await Logger.LogWarningAsync($"Could not detect a known instruction format (TOML, Markdown, YAML, XML, JSON) in {sourceDescription}; nothing was imported.");
#pragma warning restore MA0004 // Use Task.
                    return false;
                }

#pragma warning disable MA0004 // Use Task.
                await Logger.LogAsync($"Detected {detectedFormat} content in {sourceDescription}.");
#pragma warning restore MA0004 // Use Task.

                if (string.Equals(detectedFormat, "markdown", StringComparison.Ordinal))
                {
#pragma warning disable MA0004 // Use Task.
                    return await LoadMarkdownContentAsync(content, editorMode, onComponentsLoaded, tryAutoGenerate, profile: null, draftInstructionsFromProse: true);
#pragma warning restore MA0004 // Use Task.
                }

#pragma warning disable MA0004 // Use Task.
                IReadOnlyList<ModComponent> parsed = await Core.Services.ModComponentSerializationService.DeserializeModComponentFromStringAsync(content, detectedFormat);
#pragma warning restore MA0004 // Use Task.

                var newComponents = parsed.ToList();
                ProcessModLinks(newComponents);

#pragma warning disable MA0004 // Use Task.
                return await ApplyLoadedComponentsAsync(newComponents, editorMode, onComponentsLoaded, sourceDescription, loadedFileName: null);
#pragma warning restore MA0004 // Use Task.
            }
            catch (Exception ex)
            {
#pragma warning disable MA0004 // Use Task.
                await Logger.LogExceptionAsync(ex);
#pragma warning restore MA0004 // Use Task.
                return false;
            }
        }

        private async Task<bool> ApplyLoadedComponentsAsync(
            [NotNull][ItemNotNull] List<ModComponent> newComponents,
            bool editorMode,
            [NotNull] Func<Task> onComponentsLoaded,
            [NotNull] string fileType,
            [CanBeNull] string loadedFileName)
        {
            if (_mainConfig.allComponents.Count == 0)
            {
                _mainConfig.allComponents = newComponents;
                SetLastLoadedFileName(loadedFileName);

#pragma warning disable MA0004 // Use Task.
                await Logger.LogAsync($"Loaded {newComponents.Count} components from {fileType}.");
#pragma warning restore MA0004 // Use Task.
#pragma warning disable MA0004 // Use Task.
                await onComponentsLoaded();
#pragma warning restore MA0004 // Use Task.
                return true;
            }

#pragma warning disable MA0004 // Use Task.
            bool? result = await ShowConfigLoadConfirmationAsync(fileType, editorMode);
#pragma warning restore MA0004 // Use Task.

            switch (result)
            {
                case true:
                    {
                        var conflictDialog = new ComponentMergeConflictDialog(
                            _mainConfig.allComponents,
                            newComponents,
                            "Currently Loaded Components",
                            fileType,
                            (existing, incoming) =>
                                existing.Guid == incoming.Guid || FuzzyMatcher.FuzzyMatchComponents(existing, incoming));

#pragma warning disable MA0004 // Use Task.
                        await conflictDialog.ShowDialog(_parentWindow);
#pragma warning restore MA0004 // Use Task.

                        if (conflictDialog.UserConfirmed && conflictDialog.MergedComponents != null)
                        {
                            int originalCount = _mainConfig.allComponents.Count;
                            _mainConfig.allComponents = conflictDialog.MergedComponents;
                            int newCount = _mainConfig.allComponents.Count;
                            SetLastLoadedFileName(loadedFileName);

#pragma warning disable MA0004 // Use Task.
                            await Logger.LogAsync($"Merged {newComponents.Count} components from {fileType} with existing {originalCount} components using hybrid matching (GUID then Name/Author). Total components now: {newCount}");
#pragma warning restore MA0004 // Use Task.
                        }
                        else
                        {
#pragma warning disable MA0004 // Use Task.
                            await Logger.LogAsync("Merge cancelled by user.");
#pragma warning restore MA0004 // Use Task.
                            return false;
                        }
                        break;
                    }
                case false:
                    _mainConfig.allComponents = newComponents;
                    SetLastLoadedFileName(loadedFileName);
#pragma warning disable MA0004 // Use Task.
                    await Logger.LogAsync($"Overwrote existing config with {newComponents.Count} components from {fileType}.");
#pragma warning restore MA0004 // Use Task.
                    break;
                default:
                    return false;
            }

#pragma warning disable MA0004 // Use Task.
            await onComponentsLoaded();
#pragma warning restore MA0004 // Use Task.

            return true;
        }

        /// <summary>
        /// Loads a Markdown file with special parsing dialog (for regex configuration).
        /// For standard embedded-Markdown TOML files, use LoadConfigFileAsync instead.
        /// </summary>
        public async Task<bool> LoadMarkdownFileAsync(
            [NotNull] string filePath,
            bool editorMode,
            [NotNull] Func<Task> onComponentsLoaded,
            [NotNull] Func<List<ModComponent>, Task> tryAutoGenerate,
            [CanBeNull] MarkdownImportProfile profile = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                string fileContents;
                using (var reader = new StreamReader(filePath))
                {
#pragma warning disable MA0004 // Use Task.
                    fileContents = await reader.ReadToEndAsync();
#pragma warning restore MA0004 // Use Task.
                }

#pragma warning disable MA0004 // Use Task.
                return await LoadMarkdownContentAsync(fileContents, editorMode, onComponentsLoaded, tryAutoGenerate, profile, draftInstructionsFromProse: false);
#pragma warning restore MA0004 // Use Task.
            }
            catch (Exception ex)
            {
#pragma warning disable MA0004 // Use Task.
                await Logger.LogExceptionAsync(ex);
#pragma warning restore MA0004 // Use Task.
                return false;
            }
        }

        /// <summary>
        /// Loads Markdown guide content (from a file or pasted text) into components.
        /// When <paramref name="draftInstructionsFromProse"/> is true, natural-language Directions prose is
        /// additionally parsed into draft instructions that are flagged for review (never auto-trusted).
        /// </summary>
        public async Task<bool> LoadMarkdownContentAsync(
            [NotNull] string fileContents,
            bool editorMode,
            [NotNull] Func<Task> onComponentsLoaded,
            [NotNull] Func<List<ModComponent>, Task> tryAutoGenerate,
            [CanBeNull] MarkdownImportProfile profile = null,
            bool draftInstructionsFromProse = false)
        {
            try
            {
                MarkdownParserResult parseResult = null;
                    MarkdownImportProfile configuredProfile;

                    if (editorMode)
                    {
                        // UI elements must be created and shown on the UI thread
#pragma warning disable MA0004 // Use Task.
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            var dialog = new RegexImportDialog(fileContents, profile ?? MarkdownImportProfile.CreateDefault());

                            dialog.Closed += (_, __) =>
                            {
                                if (!dialog.LoadSuccessful || !(dialog.DataContext is RegexImportDialogViewModel vm))
                                {
                                    return;
                                }

                                configuredProfile = vm.ConfiguredProfile;
                                parseResult = vm.ConfirmLoad();
                                ProcessModLinks(parseResult.Components);

                                // Log asynchronously without blocking the UI
                                _ = Task.Run(async () =>
                                {
#pragma warning disable MA0004 // Use Task.
                                    await Logger.LogAsync($"Markdown parsing completed using {(configuredProfile.Mode == RegexMode.Raw ? "raw" : "individual")} regex mode.");
#pragma warning restore MA0004 // Use Task.
#pragma warning disable MA0004 // Use Task.
                                    await Logger.LogAsync($"Found {parseResult.Components?.Count ?? 0} components with {parseResult.Components?.Sum(c => c.ResourceRegistry.Count) ?? 0} total links.");
#pragma warning restore MA0004 // Use Task.

                                    if (parseResult.Warnings?.Count > 0)
                                    {
#pragma warning disable MA0004 // Use Task.
                                        await Logger.LogWarningAsync($"Markdown parsing completed with {parseResult.Warnings.Count} warnings.");
#pragma warning restore MA0004 // Use Task.
                                        foreach (string warning in parseResult.Warnings)
                                        {
#pragma warning disable MA0004 // Use Task.
                                            await Logger.LogWarningAsync($"  - {warning}");
                                        }
#pragma warning restore MA0004 // Use Task.
                                    }
                                });
                            };

#pragma warning disable MA0004 // Use Task.
                            await dialog.ShowDialog(_parentWindow);
#pragma warning restore MA0004 // Use Task.
                        });
#pragma warning restore MA0004 // Use Task.

                        if (parseResult is null)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        configuredProfile = profile ?? MarkdownImportProfile.CreateDefault();
                        var parser = new MarkdownParser(configuredProfile,
                            logInfo => Logger.Log(logInfo),
                            logVerbose => Logger.LogVerbose(logVerbose));
                        parseResult = parser.Parse(fileContents);

                        ProcessModLinks(parseResult.Components);

#pragma warning disable MA0004 // Use Task.
                        await Logger.LogAsync("Markdown parsing completed using default profile.");
#pragma warning restore MA0004 // Use Task.
#pragma warning disable MA0004 // Use Task.
                        await Logger.LogAsync($"Found {parseResult.Components?.Count ?? 0} components with {parseResult.Components?.Sum(c => c.ResourceRegistry.Count) ?? 0} total links.");
#pragma warning restore MA0004 // Use Task.
                        if (parseResult.Warnings?.Count > 0)
                        {
#pragma warning disable MA0004 // Use Task.
                            await Logger.LogWarningAsync($"Markdown parsing completed with {parseResult.Warnings.Count} warnings.");
#pragma warning restore MA0004 // Use Task.
                            foreach (string warning in parseResult.Warnings)
                            {
#pragma warning disable MA0004 // Use Task.
                                await Logger.LogWarningAsync($"  - {warning}");
                            }
#pragma warning restore MA0004 // Use Task.
                        }
                    }


                if (draftInstructionsFromProse && parseResult.Components != null)
                {
#pragma warning disable MA0004 // Use Task.
                    await GenerateDraftInstructionsFromProseAsync(parseResult.Components);
#pragma warning restore MA0004 // Use Task.
                }

                    _mainConfig.preambleContent = parseResult.PreambleContent ?? string.Empty;
                    _mainConfig.epilogueContent = parseResult.EpilogueContent ?? string.Empty;
                    _mainConfig.widescreenWarningContent = parseResult.WidescreenWarningContent ?? string.Empty;
                    _mainConfig.aspyrExclusiveWarningContent = parseResult.AspyrExclusiveWarningContent ?? string.Empty;
#pragma warning disable MA0004 // Use Task.
                    await Logger.LogAsync($"Stored {_mainConfig.preambleContent.Length} characters in preamble and {_mainConfig.epilogueContent.Length} characters in epilogue.");
#pragma warning restore MA0004 // Use Task.

                    if (_mainConfig.allComponents.Count == 0)
                    {
                        _mainConfig.allComponents = new List<ModComponent>(
                            parseResult.Components
                            ?? throw new InvalidOperationException("[LoadMarkdownFileAsync] parseResult.Components is null")
                        );
#pragma warning disable MA0004 // Use Task.
                        await Logger.LogAsync($"Loaded {parseResult.Components.Count} components from markdown.");
#pragma warning restore MA0004 // Use Task.
#pragma warning disable MA0004 // Use Task.
                        await tryAutoGenerate(parseResult.Components.ToList());
#pragma warning restore MA0004 // Use Task.
                    }
                    else
                    {
#pragma warning disable MA0004 // Use Task.
                        bool? confirmResult = await ShowConfigLoadConfirmationAsync("markdown file", editorMode);
#pragma warning restore MA0004 // Use Task.

                        if (confirmResult == true)
                        {
                            // Create the dialog on the UI thread
                            ComponentMergeConflictDialog conflictDialog = await Dispatcher.UIThread.InvokeAsync(() => new ComponentMergeConflictDialog(
                                _mainConfig.allComponents,
                                new List<ModComponent>(
                                    parseResult.Components
                                    ?? throw new InvalidOperationException("[LoadMarkdownFileAsync] parseResult.Components is null")
                                ),
                                "Currently Loaded Components",
                                "Markdown File",
                                FuzzyMatcher.FuzzyMatchComponents));

                            // Show the dialog on the UI thread as well
                            await Dispatcher.UIThread.InvokeAsync(() => conflictDialog.ShowDialog(_parentWindow));

                            if (conflictDialog.UserConfirmed && conflictDialog.MergedComponents != null)
                            {
                                int originalCount = _mainConfig.allComponents.Count;
                                _mainConfig.allComponents = conflictDialog.MergedComponents;
                                int newCount = _mainConfig.allComponents.Count;
#pragma warning disable MA0004 // Use Task.
                                await Logger.LogAsync($"Merged {parseResult.Components.Count} parsed components with existing {originalCount} components. Total components now: {newCount}");
#pragma warning restore MA0004 // Use Task.
#pragma warning disable MA0004 // Use Task.
                                await tryAutoGenerate(_mainConfig.allComponents);
#pragma warning restore MA0004 // Use Task.
                            }
                            else
                            {
#pragma warning disable MA0004 // Use Task.
                                await Logger.LogAsync("Merge cancelled by user.");
#pragma warning restore MA0004 // Use Task.
                                return false;
                            }
                        }
                        else if (confirmResult == false)
                        {
                            _mainConfig.allComponents = new List<ModComponent>(
                                parseResult.Components
                                ?? throw new InvalidOperationException("[LoadMarkdownFileAsync] parseResult.Components is null")
                            );
#pragma warning disable MA0004 // Use Task.
                            await Logger.LogAsync($"Overwrote existing config with {parseResult.Components.Count} components from markdown.");
#pragma warning restore MA0004 // Use Task.
#pragma warning disable MA0004 // Use Task.
                            await tryAutoGenerate(parseResult.Components.ToList());
#pragma warning restore MA0004 // Use Task.
                        }
                        else
                        {
                            return false;
                        }
                    }

#pragma warning disable MA0004 // Use Task.
                await onComponentsLoaded();
#pragma warning restore MA0004 // Use Task.
                return true;
            }
            catch (Exception ex)
            {
#pragma warning disable MA0004 // Use Task.
                await Logger.LogExceptionAsync(ex);
#pragma warning restore MA0004 // Use Task.
                return false;
            }
        }

        public async Task<bool> SaveTomlFileAsync(string filePath, List<ModComponent> components)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }



#pragma warning disable MA0004 // Use Task.
                await Logger.LogVerboseAsync($"Saving TOML config to {filePath}");
#pragma warning restore MA0004 // Use Task.

#pragma warning disable MA0004 // Use Task.
                await Core.Services.FileLoadingService.SaveToFileAsync(components, filePath);
#pragma warning restore MA0004 // Use Task.

                LastLoadedFileName = Path.GetFileName(filePath);
                return true;
            }
            catch (Exception ex)
            {
#pragma warning disable MA0004 // Use Task.
                await Logger.LogExceptionAsync(ex);
#pragma warning restore MA0004 // Use Task.
                return false;
            }
        }

        #region Private Helper Methods

        private void SetLastLoadedFileName([CanBeNull] string loadedFileName)
        {
            if (loadedFileName != null)
            {
                LastLoadedFileName = loadedFileName;
            }
        }

        private static async Task GenerateDraftInstructionsFromProseAsync([NotNull][ItemNotNull] IEnumerable<ModComponent> components)
        {
#pragma warning disable MA0004 // Use Task.
            IReadOnlyList<DraftInstructionResult> draftResults = await Task.Run(() => DraftInstructionService.GenerateDraftInstructions(
                components,
                logInfo: message => Logger.Log(message),
                logVerbose: message => Logger.LogVerbose(message)));
#pragma warning restore MA0004 // Use Task.

            foreach (DraftInstructionResult draftResult in draftResults)
            {
                // ApplyReviewFlag runs inside GenerateDraftInstructions; re-apply is idempotent and keeps
                // InstallationWarning aligned with CLI ReviewFlagMessage / validation-issue text.
                DraftInstructionService.ApplyReviewFlag(draftResult.Component);
#pragma warning disable MA0004 // Use Task.
                await Logger.LogWarningAsync(
                    $"'{draftResult.Component.Name}': {draftResult.DraftInstructionCount} draft instruction(s). {DraftInstructionService.ReviewFlagMessage}");
#pragma warning restore MA0004 // Use Task.
            }
        }

        private static void ProcessModLinks(IList<ModComponent> components)
        {
            if (components is null)
            {
                return;
            }

            const string baseUrl = "";

            foreach (ModComponent component in components)
            {
                var urlsToFix = component.ResourceRegistry.Keys
                    .Where(url => !string.IsNullOrEmpty(url) && url[0] == '/')
                    .ToList();

                foreach (string relativeUrl in urlsToFix)
                {
                    string fixedUrl = baseUrl + relativeUrl;
                    ResourceMetadata resourceMeta = component.ResourceRegistry[relativeUrl];
                    component.ResourceRegistry.Remove(relativeUrl);
                    component.ResourceRegistry[fixedUrl].Files = resourceMeta.Files ?? new Dictionary<string, bool?>(StringComparer.OrdinalIgnoreCase);
                    component.ResourceRegistry[fixedUrl].HandlerMetadata = resourceMeta.HandlerMetadata ?? new Dictionary<string, object>(StringComparer.Ordinal);
                }
            }
        }

        private async Task<bool?> ShowConfigLoadConfirmationAsync(string fileType, bool editorMode)
        {
            if (_mainConfig.allComponents.Count == 0)
            {
                return true;
            }

            if (!editorMode)
            {
                return false;
            }

            string confirmText = $"You already have a config loaded. Do you want to merge the {fileType} with existing components or load it as a new config?";
#pragma warning disable MA0004 // Use Task.
            return await ConfirmationDialog.ShowConfirmationDialogAsync(
                _parentWindow,
                confirmText: confirmText,
                yesButtonText: "Merge",
                noButtonText: "Load as New"
            );
#pragma warning restore MA0004 // Use Task.
        }

        #endregion
    }
}
