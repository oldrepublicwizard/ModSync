// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Protocol
{
    /// <summary>
    /// Rejects instruction-fetch URLs that target localhost / private / link-local hosts
    /// (SSRF guard for <c>modsync://</c> downloads).
    /// </summary>
    public static class InstructionUrlSafety
    {
        /// <summary>Maximum instruction-file download size (5 MiB).</summary>
        public const long MaxInstructionBytes = 5L * 1024 * 1024;

        [NotNull]
        public static Uri RequireAbsoluteHttpUrl([NotNull] string instructionUrl)
        {
            if (string.IsNullOrWhiteSpace(instructionUrl))
            {
                throw new ArgumentException("Instruction URL is required.", nameof(instructionUrl));
            }

            if (!Uri.TryCreate(instructionUrl, UriKind.Absolute, out Uri uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException(
                    "Instruction URL must be an absolute http(s) URL.",
                    nameof(instructionUrl));
            }

            return uri;
        }

        /// <summary>
        /// Validates scheme and resolves the host; throws when the target is not a public
        /// internet address suitable for instruction downloads.
        /// </summary>
        public static async Task EnsureSafeFetchTargetAsync(
            [NotNull] Uri uri,
            CancellationToken cancellationToken = default)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidOperationException(
                    "Instruction URL must use http or https.");
            }

            string host = uri.DnsSafeHost;
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("Instruction URL host is missing.");
            }

            if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Instruction downloads cannot target localhost or .local hosts.");
            }

            if (IPAddress.TryParse(host, out IPAddress literal))
            {
                if (IsBlockedAddress(literal))
                {
                    throw new InvalidOperationException(
                        "Instruction downloads cannot target private or loopback IP addresses.");
                }

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            if (addresses is null || addresses.Length == 0)
            {
                throw new InvalidOperationException(
                    "Instruction URL host could not be resolved.");
            }

            if (addresses.Any(IsBlockedAddress))
            {
                throw new InvalidOperationException(
                    "Instruction downloads cannot target hosts that resolve to private or loopback addresses.");
            }
        }

        public static bool IsBlockedAddress([NotNull] IPAddress address)
        {
            if (address is null)
            {
                return true;
            }

            if (IPAddress.IsLoopback(address)
                || address.Equals(IPAddress.Any)
                || address.Equals(IPAddress.IPv6Any)
                || address.Equals(IPAddress.None))
            {
                return true;
            }

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bytes = address.GetAddressBytes();
                // 10.0.0.0/8
                if (bytes[0] == 10)
                {
                    return true;
                }

                // 172.16.0.0/12
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                {
                    return true;
                }

                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168)
                {
                    return true;
                }

                // 169.254.0.0/16 (link-local / cloud metadata)
                if (bytes[0] == 169 && bytes[1] == 254)
                {
                    return true;
                }

                // 100.64.0.0/10 (CGNAT)
                if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
                {
                    return true;
                }

                // 0.0.0.0/8
                if (bytes[0] == 0)
                {
                    return true;
                }

                return false;
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast)
                {
                    return true;
                }

                byte[] bytes = address.GetAddressBytes();
                // Unique local fc00::/7
                if ((bytes[0] & 0xfe) == 0xfc)
                {
                    return true;
                }

                // IPv4-mapped IPv6
                if (address.IsIPv4MappedToIPv6)
                {
                    return IsBlockedAddress(address.MapToIPv4());
                }
            }

            return false;
        }
    }
}
