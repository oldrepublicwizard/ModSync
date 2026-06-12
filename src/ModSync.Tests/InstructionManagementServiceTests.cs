// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using ModSync.Core;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class InstructionManagementServiceTests
    {
        [Fact(DisplayName = "CreateInstruction adds instruction at requested index")]
        public void CreateInstruction_AddsAtIndex()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateInstruction(component, 0);
            InstructionManagementService.CreateInstruction(component, 0);

            Assert.Equal(2, component.Instructions.Count);
            Assert.NotNull(component.Instructions[0]);
            Assert.NotNull(component.Instructions[1]);
        }

        [Fact(DisplayName = "DeleteInstruction removes instruction at index")]
        public void DeleteInstruction_RemovesAtIndex()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateInstruction(component, 0);
            InstructionManagementService.CreateInstruction(component, 0);

            InstructionManagementService.DeleteInstruction(component, 0);

            Assert.Single(component.Instructions);
        }

        [Fact(DisplayName = "MoveInstruction reorders instructions")]
        public void MoveInstruction_ReordersList()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateInstruction(component, 0);
            InstructionManagementService.CreateInstruction(component, 0);
            Instruction second = component.Instructions[1];

            InstructionManagementService.MoveInstruction(component, second, 0);

            Assert.Same(second, component.Instructions[0]);
        }

        [Fact(DisplayName = "CreateOption adds option at requested index")]
        public void CreateOption_AddsAtIndex()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateOption(component, 0);
            InstructionManagementService.CreateOption(component, 0);

            Assert.Equal(2, component.Options.Count);
        }

        [Fact(DisplayName = "DeleteOption removes option at index")]
        public void DeleteOption_RemovesAtIndex()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateOption(component, 0);
            InstructionManagementService.CreateOption(component, 0);

            InstructionManagementService.DeleteOption(component, 0);

            Assert.Single(component.Options);
        }

        [Fact(DisplayName = "MoveOption reorders options")]
        public void MoveOption_ReordersList()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateOption(component, 0);
            InstructionManagementService.CreateOption(component, 0);
            Option second = component.Options[1];

            InstructionManagementService.MoveOption(component, second, 0);

            Assert.Same(second, component.Options[0]);
        }

        [Fact(DisplayName = "Service methods reject null component")]
        public void Methods_NullComponent_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => InstructionManagementService.CreateInstruction(null, 0));
            Assert.Throws<ArgumentNullException>(() => InstructionManagementService.DeleteInstruction(null, 0));
            Assert.Throws<ArgumentNullException>(() => InstructionManagementService.CreateOption(null, 0));
            Assert.Throws<ArgumentNullException>(() => InstructionManagementService.DeleteOption(null, 0));
        }

        [Fact(DisplayName = "Move helpers reject null instruction or option")]
        public void MoveHelpers_NullItem_Throw()
        {
            ModComponent component = CreateComponent();
            InstructionManagementService.CreateInstruction(component, 0);
            InstructionManagementService.CreateOption(component, 0);

            Assert.Throws<ArgumentNullException>(() =>
                InstructionManagementService.MoveInstruction(component, null, 0));
            Assert.Throws<ArgumentNullException>(() =>
                InstructionManagementService.MoveOption(component, null, 0));
        }

        private static ModComponent CreateComponent()
        {
            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = "Instruction Host",
            };
        }
    }
}
