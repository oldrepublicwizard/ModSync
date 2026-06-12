// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

using ModSync.Core;

namespace ModSync.Services
{

    public class StepNavigationService
    {
        private readonly MainConfig _mainConfig;
        private readonly ValidationService _validationService;

        public StepNavigationService(MainConfig mainConfig, ValidationService validationService)
        {
            _mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        public int GetCurrentIncompleteStep()
        {
            try
            {
                (bool step1Complete, bool step2Complete, bool step3Complete, bool step4Complete) =
                    StepProgressUiHelper.ComputePreparationSteps(_mainConfig.allComponents);

                return StepProgressUiHelper.GetCurrentIncompleteStep(
                    step1Complete,
                    step2Complete,
                    step3Complete,
                    step4Complete);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error determining current step");
                return 1;
            }
        }

        public async Task JumpToCurrentStepAsync(
            ScrollViewer scrollViewer,
            Func<string, Border> findBorder)
        {
            try
            {
                if (scrollViewer is null || findBorder is null)
                {
                    return;
                }

                int currentStep = GetCurrentIncompleteStep();
                Border targetStepBorder = findBorder($"Step{currentStep}Border");

                if (targetStepBorder != null)
                {

                    Rect targetBounds = targetStepBorder.Bounds;
                    double targetOffset = targetBounds.Top - scrollViewer.Viewport.Height / 2 + targetBounds.Height / 2;

                    targetOffset = Math.Max(0, Math.Min(targetOffset, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));

                    scrollViewer.Offset = new Vector(0, targetOffset);

                    await HighlightStepAsync(targetStepBorder);
                }
                else
                {

                    Border progressSection = FindProgressSection(scrollViewer.Content as Panel);
                    if (progressSection is null)
                    {
                        return;
                    }

                    Rect progressBounds = progressSection.Bounds;
                    double targetOffset = progressBounds.Top - scrollViewer.Viewport.Height / 2 + progressBounds.Height / 2;
                    targetOffset = Math.Max(0, Math.Min(targetOffset, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
                    scrollViewer.Offset = new Vector(0, targetOffset);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "Error jumping to current step");
            }
        }

        private static async Task HighlightStepAsync(Border stepBorder)
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is null)
                {
                    return;
                }

                IBrush originalBorderBrush = stepBorder.BorderBrush;
                Thickness originalBorderThickness = stepBorder.BorderThickness;

                stepBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
                stepBorder.BorderThickness = new Thickness(3);

                // Use ConfigureAwait(true) to ensure continuation remains on UI thread
                await Task.Delay(1000);

                stepBorder.BorderBrush = originalBorderBrush;
                stepBorder.BorderThickness = originalBorderThickness;
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "Error highlighting step");
            }
        }

        private static Border FindProgressSection(Panel panel)
        {
            if (panel is null)
            {
                return null;
            }

            foreach (Control child in panel.Children)
            {
                switch (child)
                {
                    case Border border when border.Classes.Contains("progress-section", StringComparer.Ordinal):
                        return border;
                    case Panel childPanel:
                        {
                            Border result = FindProgressSection(childPanel);
                            if (result != null)
                            {
                                return result;
                            }

                            break;
                        }
                }
            }
            return null;
        }
    }
}
