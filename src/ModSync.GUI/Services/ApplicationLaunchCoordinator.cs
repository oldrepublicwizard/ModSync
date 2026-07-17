// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

namespace ModSync.Services
{
    public enum SecondaryLaunchAction
    {
        StartNewInstance,
        ForwardProtocolUrlAndExit,
        ForwardActivateAndExit,
    }

    public static class ApplicationLaunchCoordinator
    {
        public const string ActivateMessage = "__MODSYNC_ACTIVATE__";

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
