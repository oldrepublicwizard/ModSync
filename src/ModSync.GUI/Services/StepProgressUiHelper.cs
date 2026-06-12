// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using ModSync.Core;

namespace ModSync.Services
{
    public static class StepProgressUiHelper
    {
        public static (bool Step1Complete, bool Step2Complete, bool Step3Complete, bool Step4Complete) ComputePreparationSteps(
            IList<ModComponent> allComponents)
        {
            bool step1Complete = ValidationService.IsStep1Complete();
            bool step2Complete = step1Complete && allComponents?.Count > 0;
            bool step3Complete = allComponents?.Any(component => component.IsSelected) == true;

            bool step4Complete = false;
            if (step3Complete && allComponents != null)
            {
                List<ModComponent> selectedComponents = allComponents.Where(component => component.IsSelected).ToList();
                if (selectedComponents.Count > 0)
                {
                    step4Complete = selectedComponents.All(component => component.IsDownloaded);
                }
            }

            return (step1Complete, step2Complete, step3Complete, step4Complete);
        }

        public static int GetCurrentIncompleteStep(
            bool step1Complete,
            bool step2Complete,
            bool step3Complete,
            bool step4Complete)
        {
            if (!step1Complete)
            {
                return 1;
            }

            if (!step2Complete)
            {
                return 2;
            }

            if (!step3Complete)
            {
                return 3;
            }

            if (!step4Complete)
            {
                return 4;
            }

            return 5;
        }

        public static bool ComputeStep5Complete(
            bool step4Complete,
            IList<ModComponent> allComponents,
            Func<ModComponent, bool> isComponentValid,
            bool validationCheckboxChecked)
        {
            if (!step4Complete || allComponents is null || isComponentValid is null)
            {
                return false;
            }

            List<ModComponent> selectedComponents = allComponents.Where(component => component.IsSelected).ToList();
            if (selectedComponents.Count == 0)
            {
                return false;
            }

            bool realTimeValidationPassed = selectedComponents.All(isComponentValid);
            return realTimeValidationPassed && validationCheckboxChecked;
        }

        public static int CountCompletedSteps(
            bool step1Complete,
            bool step2Complete,
            bool step3Complete,
            bool step4Complete,
            bool step5Complete)
        {
            return (step1Complete ? 1 : 0)
                + (step2Complete ? 1 : 0)
                + (step3Complete ? 1 : 0)
                + (step4Complete ? 1 : 0)
                + (step5Complete ? 1 : 0);
        }

        public static string FormatGettingStartedProgressMessage(int completedSteps)
        {
            string[] messages =
            {
                "Complete the steps above to get started",
                "Great start! Continue with the next steps",
                "Almost there! Just a few more steps",
                "Excellent progress! You're almost ready",
                "🎉 All preparation steps completed! You're ready to install mods",
            };

            int messageIndex = Math.Min(completedSteps, messages.Length - 1);
            return messages[messageIndex];
        }
    }
}
