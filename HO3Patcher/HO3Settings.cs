using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.WPF.Reflection.Attributes;
using Mutagen.Bethesda.Fallout4;
using System.Text.RegularExpressions;
using Mutagen.Bethesda;

namespace HO3Patcher
{
    public class HO3Settings
    {
        [Tooltip("Will only patch Armors from these mods. Leave empty to patch all mods.")]
        [MaintainOrder]
        public HashSet<ModKey> ModsToPatch = new();
        [Tooltip("These rules will be applied to each Armor")]
        [MaintainOrder]
        public List<HHSRules> Rules = new List<HHSRules>();
    }

    public class HHSRules
    {
        [Tooltip("The priority of the current rule. Higher priorities get applied after lower priorities")]
        [MaintainOrder]
        public int Priority = 0;
        [Tooltip("The rules that will determine which Armors will be patched")]
        [MaintainOrder]
        public ArmorMatcher MatchingRules = new ArmorMatcher();
        [Tooltip("High Heels offset (same as in the .txt)")]
        [MaintainOrder]
        public float HHSHeight = 0;
        [Tooltip("How much the heels are allowed to clip into the ground before there is any need to adjust scaling etc.")]
        [MaintainOrder]
        public float GroundClipAllowance = 0;
    }

    public class ArmorMatcher
    {
        [Tooltip("Selecting this option will make all non-empty matches apply; i.e. if one of them is unsatisfied, the Armor will not be patched. Otherwise, only one of the below has to match.")]
        [MaintainOrder]
        public bool AND = false;
        [Tooltip("Manually select Armors to be patched. Will only patch Armors that are in the chosen Mods.")]
        public HashSet<IFormLink<IArmorGetter>> ManualSelection = new();
        [Tooltip("Regex by which to match an Armor's Keywords. Only has to match one keyword.")]
        public string KeywordRegex = string.Empty;
        [Tooltip("Regex by which to match an Armor's EditorID.")]
        public string EditorIdRegex = string.Empty;
        [Tooltip("Regex by which to match an Armor's Display Name.")]
        public string DisplayNameRegex = string.Empty;
        [Tooltip("Regex by which to match an Armor's .nif model file.")]
        public string NifRegex = string.Empty;
    }

    /// <summary>
    /// Defines the various operations associated with the settings
    /// </summary>
    public class ArmorMatcherOperations : ArmorMatcher
    {
        public Regex Keyword { get; }
        public Regex EditorId { get; }
        public Regex DisplayName { get; }
        public Regex Nif { get; }
        public ILinkCache LinkCache { get; }

        public ArmorMatcherOperations(ArmorMatcher parent, ILinkCache linkCache)
        {
            KeywordRegex = parent.KeywordRegex;
            EditorIdRegex = parent.EditorIdRegex;
            DisplayNameRegex = parent.DisplayNameRegex;
            ManualSelection = parent.ManualSelection;
            NifRegex = parent.NifRegex;
            AND = parent.AND;

            Keyword = new Regex(KeywordRegex);
            EditorId = new Regex(EditorIdRegex);
            DisplayName = new Regex(DisplayNameRegex);
            Nif = new Regex(NifRegex);
            LinkCache = linkCache;
        }

        /// <inheritdoc cref="MatchArmor(IArmorGetter)" path="//param"/>
        /// <summary>
        /// Checks if any of <paramref name="armor"/>'s keywords match the regex in <see cref="ArmorMatcher.KeywordRegex"/>
        /// </summary>
        /// <returns><c>true</c> if <see cref="ArmorMatcher.KeywordRegex"/> is not empty and there is a match, <c>null</c> if <see cref="ArmorMatcher.AND"/> is off and either there is no match or the input is empty, <c>false</c> otherwise</returns>
        /// <exception cref="KeyNotFoundException">If one of <paramref name="armor"/>'s Keywords does not point to a valid record</exception>
        public bool? MatchKeywords(IArmorGetter armor)
        {
            if (Keyword.ToString() == string.Empty) return null;
            var keywords = armor.Keywords;
            if (keywords != null)
            {
                foreach (var keywordLink in keywords)
                {
                    if (keywordLink.TryResolve(LinkCache, out var keyword))
                    {
                        if (keyword.EditorID != null && Keyword.IsMatch(keyword.EditorID))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Keyword {keywordLink.FormKey} of " +
                            $"Armor {armor.EditorID} could not be resolved");
                    }
                }
            }
            return AND ? false : null;
        }

        /// <summary>
        /// Checks if <paramref name="armorName"/> matches the chosen <paramref name="property"/>
        /// </summary>
        /// <param name="property">A function pointing to an <see cref="ArmorMatcherOperations"/> property</param>
        /// <param name="armorName">The name of the armor to check</param>
        /// <returns><c>true</c> if neither the <paramref name="property"/> nor <paramref name="armorName"/> are empty and there is a match, <c>null</c> if not and <see cref="ArmorMatcher.AND"/> is off, and <c>false</c> otherwise.</returns>
        public bool? MatchName(Func<ArmorMatcherOperations, Regex> property, string? armorName)
        {
            if (property(this).ToString() != string.Empty
                )
            {
                return armorName != null && property(this).IsMatch(armorName);
            }
            else
            {
                return AND ? false : null;
            }
        }

        /// <inheritdoc cref="MatchArmor" path="//param"/>
        /// <summary>
        /// Checks if <paramref name="armor"/> is among <see cref="ArmorMatcher.ManualSelection"/>
        /// </summary>
        /// <returns><see cref="true"/> if found, <see cref="null"/> if not found and <see cref="ArmorMatcher.AND"/> is off, <see cref="false"/> otherwise</returns>
        public bool? MatchManual(IArmorGetter armor)
        {
            if (ManualSelection.Contains(armor))
            {
                return true;
            }
            else if (ManualSelection.Any() && AND)
                return false;
            return null;
        }

        public bool? MatchNif(IArmorGetter armor)
        {
            foreach (IArmorAddonModelGetter armorAddon in armor.Armatures)
            {
                var addon = armorAddon.AddonModel.TryResolve(LinkCache);
                if (addon?.WorldModel == null || !addon.WorldModel.Any())
                {
                    continue;
                }
                if (addon
                    .WorldModel
                    .Where(model => model != null && Nif.IsMatch(Path.GetFileNameWithoutExtension(model.File) == null ? "" : Path.GetFileNameWithoutExtension(model.File)!)).Any())
                {
                    return true;
                }
            }
            return AND ? false : null;
        }

        /// <summary>
        /// Checks if <paramref name="armor"/> matches any of the rules defined in <see cref="ArmorMatcher"/>
        /// </summary>
        /// <param name="armor">The armor to check</param>
        /// <returns><c>true</c> if there is a match, <c>false</c> otherwise</returns>
        public bool MatchArmor(IArmorGetter armor)
        {
            return
                MatchNif(armor)
                ?? MatchManual(armor)
                ?? MatchName((ops => ops.DisplayName), armor.Name?.String)
                ?? MatchName((ops => ops.EditorId), armor.EditorID)
                ?? MatchKeywords(armor)
                ?? false;
        }
    }
}
