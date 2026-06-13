// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;

namespace ModSync.Core.CLI
{
    public static class ConsoleInteractionCapabilities
    {
        public static bool IsInteractiveTerminal(bool forceInteractive, bool forceNonInteractive)
        {
            if (forceNonInteractive)
            {
                return false;
            }

            if (forceInteractive)
            {
                return true;
            }

            try
            {
                return !Console.IsInputRedirected
                    && !Console.IsOutputRedirected
                    && Environment.UserInteractive;
            }
            catch
            {
                return false;
            }
        }
    }
}
