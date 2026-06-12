// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModSync.Core;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class ValidationServiceTests
    {
        [Fact(DisplayName = "Constructor rejects null MainConfig")]
        public void Constructor_NullMainConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ValidationService(null));
        }

        [Fact(DisplayName = "IsComponentValidForInstallation rejects null component")]
        public void IsComponentValidForInstallation_NullComponent_ReturnsFalse()
        {
            var service = new ValidationService(new MainConfig());

            Assert.False(service.IsComponentValidForInstallation(null, editorMode: false));
        }

        [Fact(DisplayName = "IsComponentValidForInstallation requires name and instructions")]
        public void IsComponentValidForInstallation_MissingNameOrInstructions_ReturnsFalse()
        {
            var service = new ValidationService(new MainConfig());
            var missingName = new ModComponent { Guid = Guid.NewGuid(), Name = string.Empty };
            var missingInstructions = new ModComponent { Guid = Guid.NewGuid(), Name = "Valid Name" };

            Assert.False(service.IsComponentValidForInstallation(missingName, editorMode: false));
            Assert.False(service.IsComponentValidForInstallation(missingInstructions, editorMode: false));
        }

        [Fact(DisplayName = "IsComponentValidForInstallation requires selected dependencies")]
        public void IsComponentValidForInstallation_UnselectedDependency_ReturnsFalse()
        {
            ModComponent dependency = CreateComponent("Dependency");
            dependency.IsSelected = false;
            ModComponent component = CreateComponent("Main Mod");
            component.Dependencies = new List<Guid> { dependency.Guid };
            component.Instructions.Add(new Instruction());

            var service = new ValidationService(CreateConfig(dependency, component));

            Assert.False(service.IsComponentValidForInstallation(component, editorMode: false));
        }

        [Fact(DisplayName = "IsComponentValidForInstallation rejects conflicting selected restrictions")]
        public void IsComponentValidForInstallation_ConflictingRestriction_ReturnsFalse()
        {
            ModComponent restricted = CreateComponent("Restricted");
            restricted.IsSelected = true;
            ModComponent component = CreateComponent("Main Mod");
            component.Restrictions = new List<Guid> { restricted.Guid };
            component.Instructions.Add(new Instruction());

            var service = new ValidationService(CreateConfig(restricted, component));

            Assert.False(service.IsComponentValidForInstallation(component, editorMode: false));
        }

        [Fact(DisplayName = "IsComponentValidForInstallation passes when dependencies and instructions are satisfied")]
        public void IsComponentValidForInstallation_ValidComponent_ReturnsTrue()
        {
            ModComponent dependency = CreateComponent("Dependency");
            dependency.IsSelected = true;
            ModComponent component = CreateComponent("Main Mod");
            component.Dependencies = new List<Guid> { dependency.Guid };
            component.Instructions.Add(new Instruction());

            var service = new ValidationService(CreateConfig(dependency, component));

            Assert.True(service.IsComponentValidForInstallation(component, editorMode: false));
        }

        [Fact(DisplayName = "GetComponentErrorDetails surfaces dependency errors with auto-fix hint")]
        public void GetComponentErrorDetails_MissingDependency_ReturnsAutoFixableError()
        {
            ModComponent dependency = CreateComponent("Dependency");
            dependency.IsSelected = false;
            ModComponent component = CreateComponent("Main Mod");
            component.Dependencies = new List<Guid> { dependency.Guid };

            var service = new ValidationService(CreateConfig(dependency, component));
            (string errorType, string description, bool canAutoFix) = service.GetComponentErrorDetails(component);

            Assert.Contains("Missing required dependencies", description, StringComparison.OrdinalIgnoreCase);
            Assert.True(canAutoFix);
            Assert.False(string.IsNullOrWhiteSpace(errorType));
        }

        private static ModComponent CreateComponent(string name)
        {
            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = name,
            };
        }

        private static MainConfig CreateConfig(params ModComponent[] components)
        {
            return new MainConfig
            {
                allComponents = new List<ModComponent>(components),
            };
        }
    }
}
