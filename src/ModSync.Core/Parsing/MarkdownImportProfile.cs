// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ModSync.Core.Parsing
{
    public sealed class MarkdownImportProfile : INotifyPropertyChanged
    {
        private string _rawRegexPattern = string.Empty;
        public string RawRegexPattern
        {
            get => _rawRegexPattern;

            set
            {
                if (string.Equals(_rawRegexPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _rawRegexPattern = value;
                OnPropertyChanged();
            }
        }

        private RegexOptions _rawRegexOptions = RegexOptions.Multiline;
        public RegexOptions RawRegexOptions
        {
            get => _rawRegexOptions;
            set
            {
                if (_rawRegexOptions == value)
                {
                    return;
                }

                _rawRegexOptions = value;
                OnPropertyChanged();
            }
        }

        private RegexMode _mode = RegexMode.Raw;
        public RegexMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value)
                {
                    return;
                }

                _mode = value;
                OnPropertyChanged();
            }
        }

        private bool _globalFlag = true;
        public bool GlobalFlag
        {
            get => _globalFlag;
            set
            {
                if (_globalFlag == value)
                {
                    return;
                }

                _globalFlag = value;
                OnPropertyChanged();
            }
        }

        private bool _multilineFlag = true;
        public bool MultilineFlag
        {
            get => _multilineFlag;
            set
            {
                if (_multilineFlag == value)
                {
                    return;
                }

                _multilineFlag = value;
                OnPropertyChanged();
            }
        }

        private bool _ignoreCaseFlag;
        public bool IgnoreCaseFlag
        {
            get => _ignoreCaseFlag;
            set
            {
                if (_ignoreCaseFlag == value)
                {
                    return;
                }

                _ignoreCaseFlag = value;
                OnPropertyChanged();
            }
        }

        private bool _singlelineFlag = true;
        public bool SinglelineFlag
        {
            get => _singlelineFlag;
            set
            {
                if (_singlelineFlag == value)
                {
                    return;
                }

                _singlelineFlag = value;
                OnPropertyChanged();
            }
        }

        private string _headingPattern = string.Empty;
        public string HeadingPattern
        {
            get => _headingPattern;

            set
            {
                if (string.Equals(_headingPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _headingPattern = value;
                OnPropertyChanged();
            }
        }

        private string _componentSectionPattern = string.Empty;
        public string ComponentSectionPattern
        {
            get => _componentSectionPattern;

            set
            {
                if (string.Equals(_componentSectionPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _componentSectionPattern = value;
                OnPropertyChanged();
            }
        }

        private RegexOptions _componentSectionOptions = RegexOptions.Multiline | RegexOptions.Singleline;
        public RegexOptions ComponentSectionOptions
        {
            get => _componentSectionOptions;
            set
            {
                if (_componentSectionOptions == value)
                {
                    return;
                }

                _componentSectionOptions = value;
                OnPropertyChanged();
            }
        }

        private string _namePattern = string.Empty;
        public string NamePattern
        {
            get => _namePattern;

            set
            {
                if (string.Equals(_namePattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _namePattern = value;
                OnPropertyChanged();
            }
        }

        private string _authorPattern = string.Empty;
        public string AuthorPattern
        {
            get => _authorPattern;

            set
            {
                if (string.Equals(_authorPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _authorPattern = value;
                OnPropertyChanged();
            }
        }

        private string _descriptionPattern = string.Empty;
        public string DescriptionPattern
        {
            get => _descriptionPattern;

            set
            {
                if (string.Equals(_descriptionPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _descriptionPattern = value;
                OnPropertyChanged();
            }
        }

        private string _modLinkPattern = string.Empty;
        public string ModLinkPattern
        {
            get => _modLinkPattern;

            set
            {
                if (string.Equals(_modLinkPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _modLinkPattern = value;
                OnPropertyChanged();
            }
        }

        private string _categoryTierPattern = string.Empty;
        public string CategoryTierPattern
        {
            get => _categoryTierPattern;

            set
            {
                if (string.Equals(_categoryTierPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _categoryTierPattern = value;
                OnPropertyChanged();
            }
        }

        private string _installationMethodPattern = string.Empty;
        public string InstallationMethodPattern
        {
            get => _installationMethodPattern;

            set
            {
                if (string.Equals(_installationMethodPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _installationMethodPattern = value;
                OnPropertyChanged();
            }
        }

        private string _downloadInstructionsPattern = string.Empty;
        public string DownloadInstructionsPattern
        {
            get => _downloadInstructionsPattern;

            set
            {
                if (string.Equals(_downloadInstructionsPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _downloadInstructionsPattern = value;
                OnPropertyChanged();
            }
        }

        private string _installationInstructionsPattern = string.Empty;
        public string InstallationInstructionsPattern
        {
            get => _installationInstructionsPattern;

            set
            {
                if (string.Equals(_installationInstructionsPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _installationInstructionsPattern = value;
                OnPropertyChanged();
            }
        }

        private string _usageWarningPattern = string.Empty;
        public string UsageWarningPattern
        {
            get => _usageWarningPattern;

            set
            {
                if (string.Equals(_usageWarningPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _usageWarningPattern = value;
                OnPropertyChanged();
            }
        }

        private string _screenshotsPattern = string.Empty;
        public string ScreenshotsPattern
        {
            get => _screenshotsPattern;

            set
            {
                if (string.Equals(_screenshotsPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _screenshotsPattern = value;
                OnPropertyChanged();
            }
        }

        private string _knownBugsPattern = string.Empty;
        public string KnownBugsPattern
        {
            get => _knownBugsPattern;

            set
            {
                if (string.Equals(_knownBugsPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _knownBugsPattern = value;
                OnPropertyChanged();
            }
        }

        private string _installationWarningPattern = string.Empty;
        public string InstallationWarningPattern
        {
            get => _installationWarningPattern;

            set
            {
                if (string.Equals(_installationWarningPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _installationWarningPattern = value;
                OnPropertyChanged();
            }
        }

        private string _compatibilityWarningPattern = string.Empty;
        public string CompatibilityWarningPattern
        {
            get => _compatibilityWarningPattern;

            set
            {
                if (string.Equals(_compatibilityWarningPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _compatibilityWarningPattern = value;
                OnPropertyChanged();
            }
        }

        private string _steamNotesPattern = string.Empty;
        public string SteamNotesPattern
        {
            get => _steamNotesPattern;

            set
            {
                if (string.Equals(_steamNotesPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _steamNotesPattern = value;
                OnPropertyChanged();
            }
        }

        private string _nonEnglishPattern = string.Empty;
        public string NonEnglishPattern
        {
            get => _nonEnglishPattern;

            set
            {
                if (string.Equals(_nonEnglishPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _nonEnglishPattern = value;
                OnPropertyChanged();
            }
        }

        private string _dependenciesPattern = string.Empty;
        public string DependenciesPattern
        {
            get => _dependenciesPattern;

            set
            {
                if (string.Equals(_dependenciesPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _dependenciesPattern = value;
                OnPropertyChanged();
            }
        }

        private string _dependenciesSeparatorPattern = string.Empty;
        public string DependenciesSeparatorPattern
        {
            get => _dependenciesSeparatorPattern;

            set
            {
                if (string.Equals(_dependenciesSeparatorPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _dependenciesSeparatorPattern = value;
                OnPropertyChanged();
            }
        }

        private string _restrictionsPattern = string.Empty;
        public string RestrictionsPattern
        {
            get => _restrictionsPattern;

            set
            {
                if (string.Equals(_restrictionsPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _restrictionsPattern = value;
                OnPropertyChanged();
            }
        }

        private string _optionPattern = string.Empty;
        public string OptionPattern
        {
            get => _optionPattern;

            set
            {
                if (string.Equals(_optionPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _optionPattern = value;
                OnPropertyChanged();
            }
        }

        private string _instructionPattern = string.Empty;
        public string InstructionPattern
        {
            get => _instructionPattern;

            set
            {
                if (string.Equals(_instructionPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _instructionPattern = value;
                OnPropertyChanged();
            }
        }

        private string _instructionsBlockPattern = string.Empty;
        public string InstructionsBlockPattern
        {
            get => _instructionsBlockPattern;

            set
            {
                if (string.Equals(_instructionsBlockPattern, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _instructionsBlockPattern = value;
                OnPropertyChanged();
            }
        }

        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>(System.StringComparer.Ordinal);

        public RegexOptions GetRegexOptions()
        {
            RegexOptions options = RegexOptions.Compiled;

            if (MultilineFlag)
            {
                options |= RegexOptions.Multiline;
            }

            if (SinglelineFlag)
            {
                options |= RegexOptions.Singleline;
            }

            if (IgnoreCaseFlag)
            {
                options |= RegexOptions.IgnoreCase;
            }

            return options;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MarkdownImportProfile Clone()
        {
            var clone = (MarkdownImportProfile)MemberwiseClone();
            clone.Metadata.Clear();
            foreach (KeyValuePair<string, object> pair in Metadata)
            {
                clone.Metadata[pair.Key] = pair.Value;
            }
            clone.PropertyChanged = null;
            return clone;
        }

        public static MarkdownImportProfile CreateDefault()
        {
            // Shared boundary: next bold **Field:**, plain Field:, horizontal rule, or markdown heading.
            // Bold alternatives are listed first so **Name:** wins when both styles appear.
            const string fieldBoundary =
                @"(?:\*\*[^*\r\n]{1,100}:\*\*|(?:Name|Author|Description|Masters|Category\s*&\s*Tier|Non-English Functionality|Installation Method|Installation Instructions|Install Instructions|Download Instructions|Usage Warning|Screenshots|Known Bugs|Installation Warning|Compatibility Warning|Steam Notes)\s*:|#{2,3}\s|_{3,}|-{3,})";

            const string defaultRawPattern =
                @"(?ms)^###\s*(?<heading>.+?)\s*\r?\n" +
                @"(?:[\s\S]*?(?:\*\*Name:\*\*|Name:)\s*(?:\[(?<name>(?<name_link>[^\]]+))\]\([^)]+\)|(?<name_plain>.*?))(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?:\*\*Author:\*\*|Author:)\s*(?<author>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?:\*\*Description:\*\*|Description:)\s*(?<description>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?:\*\*Masters:\*\*|Masters:)\s*(?<masters>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?:\*\*Category\s*&\s*Tier:\*\*|Category\s*&\s*Tier:)\s*(?<category_tier>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?:\*\*Non-English Functionality:\*\*|Non-English Functionality:)\s*(?<non_english>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?:\*\*Installation Method:\*\*|Installation Method:)\s*(?<installation_method>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z))?" +
                @"(?:[\s\S]*?(?::::note\s*\r?\n\s*Installation Instructions\s*\r?\n:\s*(?<installation_instructions>(?:(?!\r?\n\s*:::).)*?)\r?\n\s*:::|(?:\*\*(?:Install(?:ation)?|Installation) Instructions:\*\*|(?:Install(?:ation)?|Installation) Instructions:?)\s*(?<installation_instructions>.*?)(?=\r?\n\s*" + fieldBoundary + @"|\Z)))?" +
                @"[\s\S]*?(?=\r?\n\s*(?:-{3,}|_{3,})|\Z)";

            // Also stop at the next ### so site-scraped guides without ___ separators still split per mod.
            const string defaultOuterPattern = @"(?m)^###\s*.+?$[\s\S]*?(?=^###\s|^___\s*$|^##\s|\Z)";

            const string defaultInstructionsBlockPattern = @"<!--<<ModSync>>\s*(?<instructions>[\s\S]*?)-->";

            const string multilineFieldBody = @"(?:(?!\r?\n\s*" + fieldBoundary + @").)*";

            return new MarkdownImportProfile
            {
                Mode = RegexMode.Individual,
                ComponentSectionPattern = defaultOuterPattern,
                ComponentSectionOptions = RegexOptions.Multiline,
                RawRegexPattern = defaultRawPattern,
                RawRegexOptions = RegexOptions.Multiline | RegexOptions.Singleline,
                HeadingPattern = @"^###\s+(?<heading>.+?)(?:\s*\[.*?\])?\s*$",
                NamePattern = @"(?:\*\*Name:\*\*|Name:)\s*(?:\[(?<name>(?<name_link>[^\]]+))\]\([^)]+\)|(?<name_plain>[^\r\n]+))[^\r\n]*",
                AuthorPattern = @"(?:\*\*Author:\*\*|Author:)\s*(?<author>[^\r\n]+)",
                DescriptionPattern = @"(?:\*\*Description:\*\*|Description:)\s*(?<description>" + multilineFieldBody + @")",
                ModLinkPattern = @"\[(?<label>[^]]+)\]\((?<link>[^)]+)\)",
                CategoryTierPattern = @"(?:\*\*Category\s*&\s*Tier:\*\*|Category\s*&\s*Tier:)\s*(?<category>[^/\r\n]+)/\s*(?<tier>[^\r\n]+)",
                InstallationMethodPattern = @"(?:\*\*Installation Method:\*\*|Installation Method:)\s*(?<method>[^\r\n]+)",
                DownloadInstructionsPattern = @"(?:\*\*Download Instructions:\*\*|Download Instructions:?)\s*(?<download>" + multilineFieldBody + @")",
                InstallationInstructionsPattern =
                    @"(?::::note\s*\r?\n\s*Installation Instructions\s*\r?\n:\s*(?<directions>(?:(?!\r?\n\s*:::).)*?)\r?\n\s*:::" +
                    @"|(?:\*\*(?:Install(?:ation)?|Installation) Instructions:\*\*\s*(?<directions>" + multilineFieldBody + @"))" +
                    @"|(?m)^(?:Install(?:ation)?|Installation) Instructions:?\s*(?:\r?\n)+(?<directions>" + multilineFieldBody + @"))",
                UsageWarningPattern = @"(?:\*\*Usage Warning:\*\*|Usage Warning:?)\s*(?<warning>" + multilineFieldBody + @")",
                ScreenshotsPattern = @"(?:\*\*Screenshots:\*\*|Screenshots:?)\s*(?<screenshots>" + multilineFieldBody + @")",
                KnownBugsPattern =
                    @"(?::::warning\s*\r?\n\s*Known Bugs\s*\r?\n:\s*(?<bugs>(?:(?!\r?\n\s*:::).)*?)\r?\n\s*:::" +
                    @"|(?:\*\*Known Bugs:\*\*|Known Bugs:?)\s*(?<bugs>" + multilineFieldBody + @"))",
                InstallationWarningPattern = @"(?:\*\*Installation Warning:\*\*|Installation Warning:?)\s*(?<installwarning>" + multilineFieldBody + @")",
                CompatibilityWarningPattern = @"(?:\*\*Compatibility Warning:\*\*|Compatibility Warning:?)\s*(?<compatwarning>" + multilineFieldBody + @")",
                SteamNotesPattern = @"(?:\*\*Steam Notes:\*\*|Steam Notes:?)\s*(?<steamnotes>" + multilineFieldBody + @")",
                NonEnglishPattern = @"(?:\*\*Non-English Functionality:\*\*|Non-English Functionality:)\s*(?<value>[^\r\n]+)",
                DependenciesPattern = @"(?:\*\*Masters:\*\*|Masters:)\s*(?<masters>[^\r\n]+)",
                DependenciesSeparatorPattern = @"[,;+&]",
                RestrictionsPattern = string.Empty,
                OptionPattern = string.Empty,
                InstructionPattern = string.Empty,
                InstructionsBlockPattern = defaultInstructionsBlockPattern,

                GlobalFlag = true,
                MultilineFlag = true,
                SinglelineFlag = true,
                IgnoreCaseFlag = false,
            };
        }
    }
    public enum RegexMode
    {
        Individual,
        Raw,
    }
}
