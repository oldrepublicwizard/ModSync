// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Headless.XUnit;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class ThemeServiceTests
    {
        [Fact(DisplayName = "GetStylePathForTheme maps each ThemeType to style resource")]
        public void GetStylePathForTheme_ReturnsExpectedPaths()
        {
            Assert.Equal("/Styles/LightStyle.axaml", ThemeService.GetStylePathForTheme(ThemeType.Light));
            Assert.Equal("/Styles/KotorStyle.axaml", ThemeService.GetStylePathForTheme(ThemeType.KOTOR));
            Assert.Equal("/Styles/Kotor2Style.axaml", ThemeService.GetStylePathForTheme(ThemeType.KOTOR2));
        }

        [AvaloniaFact(DisplayName = "ApplyTheme Light updates current theme metadata")]
        public void ApplyTheme_Light_UpdatesCurrentTheme()
        {
            ThemeService.ApplyTheme(ThemeType.Light);

            string currentTheme = ThemeService.GetCurrentTheme();
            Assert.Contains("LightStyle", currentTheme ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
            Assert.Equal(ThemeType.Light, ThemeService.GetCurrentThemeType());
        }

        [AvaloniaFact(DisplayName = "ApplyTheme KOTOR updates current theme metadata")]
        public void ApplyTheme_Kotor_UpdatesCurrentTheme()
        {
            ThemeService.ApplyTheme(ThemeType.KOTOR);

            string currentTheme = ThemeService.GetCurrentTheme();
            Assert.Contains("KotorStyle", currentTheme ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
            Assert.Equal(ThemeType.KOTOR, ThemeService.GetCurrentThemeType());
        }

        [AvaloniaFact(DisplayName = "ApplyTheme KOTOR2 updates current theme metadata")]
        public void ApplyTheme_Kotor2_UpdatesCurrentTheme()
        {
            ThemeService.ApplyTheme(ThemeType.KOTOR2);

            string currentTheme = ThemeService.GetCurrentTheme();
            Assert.Contains("Kotor2Style", currentTheme ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
            Assert.Equal(ThemeType.KOTOR2, ThemeService.GetCurrentThemeType());
        }

        [AvaloniaFact(DisplayName = "ApplyTheme ignores null or empty style path")]
        public void ApplyTheme_NullOrEmpty_DoesNotThrow()
        {
            ThemeService.ApplyTheme(ThemeType.Light);
            ThemeType before = ThemeService.GetCurrentThemeType();

            ThemeService.ApplyTheme(null);
            ThemeService.ApplyTheme(string.Empty);

            Assert.Equal(before, ThemeService.GetCurrentThemeType());
        }
    }
}
