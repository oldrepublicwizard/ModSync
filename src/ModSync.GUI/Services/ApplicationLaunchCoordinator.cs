// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

namespace ModSync.Services
{
    /// <summary>
    /// Decides how a ModSync process should behave when coordinating with other instances.
    /// </summary>
    public enum SecondaryLaunchAction
    {
        /// <summary>Start a new GUI even though another instance is running (dev escape hatch).</summary>
        StartNewInstance,

        /// <summary>Forward an nxm:// or modsync:// URL to the primary instance and exit.</summary>
        ForwardProtocolUrlAndExit,

        /// <summary>Ask the primary instance to activate and exit.</summary>
        ForwardActivateAndExit,
    }

    /// <summary>
    /// Pure launch-policy helpers used by <see cref="Program"/> and unit tests.
    /// </summary>
    public static class ApplicationLaunchCoordinator
    {
        /// <summary>Named-pipe message sent when a second non-protocol launch should focus the primary window.</summary>
        public const string ActivateMessage = "__MODSYNC_ACTIVATE__";

        /// <summary>
        /// Determines what a secondary process should do after failing to claim the single-instance pipe.
        /// </summary>
        public static SecondaryLaunchAction DecideSecondaryAction(bool hasProtocolHandoffUrl, bool allowMultipleInstances)
        {
            if (allowMultipleInstances)
            {
                return SecondaryLaunchAction.StartNewInstance;
            }

            if (hasProtocolHandoffUrl)
            {
                return SecondaryLaunchAction.ForwardProtocolUrlAndExit;
            }

            return SecondaryLaunchAction.ForwardActivateAndExit;
        }
    }
}
