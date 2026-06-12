// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace ModSync.Core.Services.Download
{
    /// <summary>
    /// Result of resolving an nxm hand-off against loaded instruction components.
    /// </summary>
    public enum NxmComponentResolveStatus
    {
        NotFound,
        Ambiguous,
        Matched,
    }

    /// <summary>
    /// A resolved nxm hand-off: the matched component and its HTTPS registry URL key.
    /// </summary>
    public sealed class NxmComponentMatch
    {
        public ModComponent Component { get; set; }

        /// <summary>
        /// The <see cref="ModComponent.ResourceRegistry"/> HTTPS key used for cache updates.
        /// </summary>
        public string RegistryUrl { get; set; }
    }

    /// <summary>
    /// Matches an incoming nxm hand-off to a loaded <see cref="ModComponent"/> by comparing
    /// the nxm game domain + mod id against Nexus Mods URLs in <see cref="ModComponent.ResourceRegistry"/>.
    /// </summary>
    public static class NxmComponentResolver
    {
        /// <summary>
        /// Resolves an nxm URL to a component and the matching HTTPS registry key.
        /// Returns <see cref="NxmComponentResolveStatus.Ambiguous"/> when multiple components match.
        /// </summary>
        public static NxmComponentResolveStatus TryResolve(
            NxmUrl nxmUrl,
            IReadOnlyList<ModComponent> components,
            out NxmComponentMatch match)
        {
            match = null;
            if (nxmUrl is null || components is null || components.Count == 0)
            {
                return NxmComponentResolveStatus.NotFound;
            }

            ModComponent matchedComponent = null;
            string matchedRegistryUrl = null;

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

                    if (matchedComponent != null && !ReferenceEquals(matchedComponent, component))
                    {
                        match = null;
                        return NxmComponentResolveStatus.Ambiguous;
                    }

                    matchedComponent = component;
                    matchedRegistryUrl = registryUrl;
                    break;
                }
            }

            if (matchedComponent is null)
            {
                return NxmComponentResolveStatus.NotFound;
            }

            match = new NxmComponentMatch
            {
                Component = matchedComponent,
                RegistryUrl = matchedRegistryUrl,
            };
            return NxmComponentResolveStatus.Matched;
        }

        /// <summary>
        /// Returns the first component whose registry contains a Nexus URL for the same game and mod id.
        /// Returns null when no components are loaded, no registry key matches, or the match is ambiguous.
        /// </summary>
        public static ModComponent FindComponentForNxmUrl(NxmUrl nxmUrl, IReadOnlyList<ModComponent> components)
        {
            NxmComponentResolveStatus status = TryResolve(nxmUrl, components, out NxmComponentMatch match);
            return status == NxmComponentResolveStatus.Matched ? match?.Component : null;
        }
    }
}
