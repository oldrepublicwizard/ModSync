// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace ModSync.Core.Services.Download
{
    /// <summary>
    /// Matches an incoming nxm hand-off to a loaded <see cref="ModComponent"/> by comparing
    /// the nxm game domain + mod id against Nexus Mods URLs in <see cref="ModComponent.ResourceRegistry"/>.
    /// </summary>
    public static class NxmComponentResolver
    {
        /// <summary>
        /// Returns the first component whose registry contains a Nexus URL for the same game and mod id.
        /// Returns null when no components are loaded or no registry key matches.
        /// </summary>
        public static ModComponent FindComponentForNxmUrl(NxmUrl nxmUrl, IReadOnlyList<ModComponent> components)
        {
            if (nxmUrl is null || components is null || components.Count == 0)
            {
                return null;
            }

            ModComponent match = null;
            foreach (ModComponent component in components)
            {
                if (component?.ResourceRegistry is null)
                {
                    continue;
                }

                foreach (string registryUrl in component.ResourceRegistry.Keys)
                {
                    if (!nxmUrl.MatchesNexusUrl(registryUrl))
                    {
                        continue;
                    }

                    if (match != null && !ReferenceEquals(match, component))
                    {
                        return null;
                    }

                    match = component;
                    break;
                }
            }

            return match;
        }
    }
}
