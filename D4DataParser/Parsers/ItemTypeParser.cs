using D4DataParser.Constants;
using D4DataParser.Entities;
using D4DataParser.Entities.D4Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace D4DataParser.Parsers
{
    public class ItemTypeParser
    {
        private string _d4dataPath = string.Empty;
        private List<string> _languages = new List<string>();
        private List<ItemTypeInfo> _itemTypeInfoList = new List<ItemTypeInfo>();

        // Start of Constructors region

        #region Constructors

        public ItemTypeParser()
        {
            // Init languages
            InitLocalisations();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public string D4dataPath { get => _d4dataPath; set => _d4dataPath = value; }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        private void InitLocalisations()
        {
            _languages.Clear();

            // TODO: - DEV - Enable languages for release
            _languages.Add("deDE");
            _languages.Add("enUS");
            _languages.Add("esES");
            _languages.Add("esMX");
            _languages.Add("frFR");
            _languages.Add("itIT");
            _languages.Add("jaJP");
            _languages.Add("koKR");
            _languages.Add("plPL");
            _languages.Add("ptBR");
            _languages.Add("ruRU");
            _languages.Add("trTR");
            _languages.Add("zhCN");
            _languages.Add("zhTW");
        }

        public void Parse()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            foreach (var language in _languages)
            {
                if (Directory.Exists($"{_d4dataPath}json\\{language}_Text\\"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");
                    ParseByLanguage(language);
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped {language}, not available.");
                }
            }

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time: {watch.ElapsedMilliseconds}");
        }

        private void ParseByLanguage(string language)
        {
            // Reset
            _itemTypeInfoList.Clear();

            // ItemQuality - ".\d4data\json\enUS_Text\meta\StringList\ItemQuality.stl.json"
            var jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemQuality.stl.json");
            var localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();

            // List quality
            List<ArString> qualities = new List<ArString>
            {
                new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Sacred", StringComparison.OrdinalIgnoreCase)) ?? new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Ancestral", StringComparison.OrdinalIgnoreCase)) ?? new()
            };

            // List rarerity
            List<ArString> rarities = new List<ArString>
            {
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Normal", StringComparison.OrdinalIgnoreCase)) ?? new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Magic", StringComparison.OrdinalIgnoreCase)) ?? new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Rare", StringComparison.OrdinalIgnoreCase)) ?? new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Legendary", StringComparison.OrdinalIgnoreCase)) ?? new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Unique", StringComparison.OrdinalIgnoreCase)) ?? new(),
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Mythic", StringComparison.OrdinalIgnoreCase)) ?? new()
            };

            // Local function to combine ItemType with ItemQuality
            void AddItemType(string type, string typeLoc)
            {
                string variant = string.Empty;
                if (typeLoc.Contains("["))
                {
                    variant = typeLoc.Substring(0, typeLoc.IndexOf("]") + 1);
                }

                foreach (var quality in qualities)
                {
                    foreach (var rarity in rarities)
                    {
                        // Extract variant from quality and rarity that matches with the current type.
                        string qualityVariant = string.IsNullOrWhiteSpace(quality.szText) ? string.Empty : string.IsNullOrWhiteSpace(variant) ? quality.szText :
                            quality.szText.Substring(quality.szText.IndexOf(variant) + variant.Length, (quality.szText.IndexOf("[", quality.szText.IndexOf(variant) + variant.Length) == -1 ? quality.szText.Length : quality.szText.IndexOf("[", quality.szText.IndexOf(variant) + variant.Length)) - (quality.szText.IndexOf(variant) + variant.Length));

                        string rarityVariant = string.IsNullOrWhiteSpace(variant) ? rarity.szText :
                            rarity.szText.Substring(rarity.szText.IndexOf(variant) + variant.Length, (rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length) == -1 ? rarity.szText.Length : rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length)) - (rarity.szText.IndexOf(variant) + variant.Length));

                        string name = $"{RemoveVariantIndicator(qualityVariant)} {RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(typeLoc)}".Trim();
                        if (language.Equals("trTR"))
                        {
                            if (string.IsNullOrWhiteSpace(qualityVariant))
                            {
                                name = $"{RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(typeLoc)}".Trim();
                            }
                            else
                            {
                                name = $"{RemoveVariantIndicator(qualityVariant)} {RemoveVariantIndicator(typeLoc)}".Trim();
                            }
                        }
                        else if(language.Equals("esES") || language.Equals("ptBR"))
                        {
                            if (string.IsNullOrWhiteSpace(qualityVariant))
                            {
                                name = $"{RemoveVariantIndicator(typeLoc)} {RemoveVariantIndicator(rarityVariant)}".Trim();
                            }
                            else
                            {
                                name = $"{RemoveVariantIndicator(typeLoc)} {RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(qualityVariant)}".Trim();
                            }
                        }
                        else if (language.Equals("esMX") || language.Equals("frFR") || language.Equals("itIT"))
                        {
                            if (string.IsNullOrWhiteSpace(qualityVariant))
                            {
                                name = $"{RemoveVariantIndicator(typeLoc)} {RemoveVariantIndicator(rarityVariant)}".Trim();
                            }
                            else
                            {
                                name = $"{RemoveVariantIndicator(typeLoc)} {RemoveVariantIndicator(qualityVariant)} {RemoveVariantIndicator(rarityVariant)}".Trim();
                            }
                        }
                        else if (language.Equals("zhCN"))
                        {
                            // Note: No spaces
                            name = $"{RemoveVariantIndicator(qualityVariant)}{RemoveVariantIndicator(rarityVariant)}{RemoveVariantIndicator(typeLoc)}".Trim();
                        }

                        // Skip duplicates
                        if (_itemTypeInfoList.Any(t => t.Name.Equals(name))) continue;

                        _itemTypeInfoList.Add(new ItemTypeInfo
                        {
                            Name = name,
                            Type = type
                        });
                    }
                }
            }

            // Local function to combine ItemType Runes with ItemRarity
            void AddItemTypeRunes(string type, string itemTypeLoc)
            {
                string variant = itemTypeLoc.Contains("[") ? itemTypeLoc.Substring(0, itemTypeLoc.IndexOf("]") + 1) : string.Empty;
                foreach (var rarity in rarities)
                {
                    if (!rarity.szLabel.Equals("Magic") &&
                        !rarity.szLabel.Equals("Rare") &&
                        !rarity.szLabel.Equals("Legendary")) continue;

                    // Extract variant from rarity that matches with the current type.
                    string rarityVariant = string.IsNullOrWhiteSpace(variant) ? rarity.szText :
                        rarity.szText.Substring(rarity.szText.IndexOf(variant) + variant.Length, (rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length) == -1 ? rarity.szText.Length : rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length)) - (rarity.szText.IndexOf(variant) + variant.Length));

                    string name = $"{RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(itemTypeLoc)}".Trim();
                    if (!string.IsNullOrWhiteSpace(rarityVariant) && (language.Equals("esES") || language.Equals("frFR")))
                    {
                        name = $"{RemoveVariantIndicator(itemTypeLoc)} {RemoveVariantIndicator(rarityVariant)}".Trim();
                    }

                    _itemTypeInfoList.Add(new ItemTypeInfo
                    {
                        Name = name,
                        Type = type
                    });
                }
            }

            // Local function to combine ItemType Occult Gem with ItemRarity
            void AddItemTypeOccultGem(string type, string itemTypeLoc)
            {
                string variant = itemTypeLoc.Contains("[") ? itemTypeLoc.Substring(0, itemTypeLoc.IndexOf("]") + 1) : string.Empty;
                foreach (var rarity in rarities)
                {
                    if (!rarity.szLabel.Equals("Magic") &&
                        !rarity.szLabel.Equals("Rare") &&
                        !rarity.szLabel.Equals("Legendary")) continue;

                    // Extract variant from rarity that matches with the current type.
                    string rarityVariant = string.IsNullOrWhiteSpace(variant) ? rarity.szText :
                        rarity.szText.Substring(rarity.szText.IndexOf(variant) + variant.Length, (rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length) == -1 ? rarity.szText.Length : rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length)) - (rarity.szText.IndexOf(variant) + variant.Length));

                    string name = $"{RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(itemTypeLoc)}".Trim();
                    if (!string.IsNullOrWhiteSpace(rarityVariant) && (language.Equals("frFR")))
                    {
                        name = $"{RemoveVariantIndicator(itemTypeLoc)} {RemoveVariantIndicator(rarityVariant)}".Trim();
                    }

                    _itemTypeInfoList.Add(new ItemTypeInfo
                    {
                        Name = name,
                        Type = type
                    });
                }
            }

            // Local function to add ItemType WitcherSigil (Whispering Wood)
            void AddItemTypeWitcherSigil(string type, string itemTypeLoc)
            {
                string name = $"{RemoveVariantIndicator(itemTypeLoc)}".Trim();

                _itemTypeInfoList.Add(new ItemTypeInfo
                {
                    Name = name,
                    Type = type
                });
            }

            // Local function to add ItemType DungeonEscalation (Escalation Sigil)
            void AddItemTypeDungeonEscalation(string type, string itemTypeLoc)
            {
                string name = $"{RemoveVariantIndicator(itemTypeLoc)}".Trim();

                _itemTypeInfoList.Add(new ItemTypeInfo
                {
                    Name = name,
                    Type = type
                });
            }

            // Local function to combine ItemType Horadric Jewel with ItemRarity
            void AddItemTypeHoradricJewel(string type, string itemTypeLoc)
            {
                string variant = itemTypeLoc.Contains("[") ? itemTypeLoc.Substring(0, itemTypeLoc.IndexOf("]") + 1) : string.Empty;
                foreach (var rarity in rarities)
                {
                    if (!rarity.szLabel.Equals("Unique")) continue;

                    // Extract variant from rarity that matches with the current type.
                    string rarityVariant = string.IsNullOrWhiteSpace(variant) ? rarity.szText :
                        rarity.szText.Substring(rarity.szText.IndexOf(variant) + variant.Length, (rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length) == -1 ? rarity.szText.Length : rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length)) - (rarity.szText.IndexOf(variant) + variant.Length));

                    string name = $"{RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(itemTypeLoc)}".Trim();
                    if (!string.IsNullOrWhiteSpace(rarityVariant) && (language.Equals("esES") || language.Equals("frFR")))
                    {
                        name = $"{RemoveVariantIndicator(itemTypeLoc)} {RemoveVariantIndicator(rarityVariant)}".Trim();
                    }

                    _itemTypeInfoList.Add(new ItemTypeInfo
                    {
                        Name = name,
                        Type = type
                    });
                }
            }

            string RemoveVariantIndicator(string typeLoc)
            {
                // Remove variant indicator from text.
                return typeLoc.Contains("]") ? typeLoc.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[1] : typeLoc;
            }

            // List type
            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Amulet.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            string itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Amulet, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Axe.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Axe2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Boots.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Boots, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Bow.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Ranged, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_ChestArmor.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Chest, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Crossbow.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemType(ItemTypeConstants.Ranged, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Crossbow2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Ranged, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Dagger.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_DaggerOffHand.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Focus.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}\\meta\\StringList\\ItemType_FocusBookOffHand.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Glaive.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Gloves.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Gloves, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Helm.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Helm, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Legs.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Pants, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Mace.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Mace2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Mace2HDruid.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_OffHandTotem.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Polearm.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Quarterstaff.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Ring.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Ring, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Scythe.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Scythe2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Shield.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Staff.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_StaffDruid.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_StaffSorcerer.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Sword.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Sword2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("BarbarianArsenalOverrideName", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_Wand.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            // List type - Sigil
            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_DungeonKey.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            string variant = itemTypeLoc.Contains("[") ? itemTypeLoc.Substring(0, itemTypeLoc.IndexOf("]") + 1) : string.Empty;
            itemTypeLoc = itemTypeLoc.Contains("]") ? itemTypeLoc.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[1] : itemTypeLoc;

            _itemTypeInfoList.Add(new ItemTypeInfo
            {
                Name = itemTypeLoc,
                Type = ItemTypeConstants.Sigil
            });

            //foreach (var quality in qualities)
            //{
            //    if (string.IsNullOrWhiteSpace(quality.szText)) continue;

            //    // Extract variant from quality that matches with the current type.
            //    string qualityVariant = string.IsNullOrWhiteSpace(variant) ? quality.szText :
            //        quality.szText.Substring(quality.szText.IndexOf(variant) + variant.Length, (quality.szText.IndexOf("[", quality.szText.IndexOf(variant) + variant.Length) == -1 ? quality.szText.Length : quality.szText.IndexOf("[", quality.szText.IndexOf(variant) + variant.Length)) - (quality.szText.IndexOf(variant) + variant.Length));

            //    string name = $"{RemoveVariantIndicator(qualityVariant)} {RemoveVariantIndicator(itemTypeLoc)}".Trim();
            //    if (!string.IsNullOrWhiteSpace(qualityVariant) && 
            //        (language.Equals("frFR") || language.Equals("ptBR")))
            //    {
            //        name = $"{RemoveVariantIndicator(itemTypeLoc)} {RemoveVariantIndicator(qualityVariant)}".Trim();
            //    }

            //    _itemTypeInfoList.Add(new ItemTypeInfo
            //    {
            //        Name = name,
            //        Type = ItemTypeConstants.Sigil
            //    });
            //}

            // List type - Temper Manual
            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_TemperManual.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //variant = itemTypeLoc.Contains("[") ? itemTypeLoc.Substring(0, itemTypeLoc.IndexOf("]") + 1) : string.Empty;

            //foreach (var rarity in rarities)
            //{
            //    // Extract variant from rarity that matches with the current type.
            //    string rarityVariant = string.IsNullOrWhiteSpace(variant) ? rarity.szText :
            //        rarity.szText.Substring(rarity.szText.IndexOf(variant) + variant.Length, (rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length) == -1 ? rarity.szText.Length : rarity.szText.IndexOf("[", rarity.szText.IndexOf(variant) + variant.Length)) - (rarity.szText.IndexOf(variant) + variant.Length));

            //    string name = $"{RemoveVariantIndicator(rarityVariant)} {RemoveVariantIndicator(itemTypeLoc)}".Trim();
            //    if (!string.IsNullOrWhiteSpace(rarityVariant) && (language.Equals("frFR")))
            //    {
            //        name = $"{RemoveVariantIndicator(itemTypeLoc)} {RemoveVariantIndicator(rarityVariant)}".Trim();
            //    }

            //    _itemTypeInfoList.Add(new ItemTypeInfo
            //    {
            //        Name = name,
            //        Type = ItemTypeConstants.Temper
            //    });
            //}

            // List type - Runes
            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_ConditionRune.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemTypeRunes(ItemTypeConstants.Rune, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_EffectRune.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemTypeRunes(ItemTypeConstants.Rune, itemTypeLoc);

            // List type - Occult Gem (Season 7)
            //jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_SeasonalSocketable.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //AddItemTypeOccultGem(ItemTypeConstants.OccultGem, itemTypeLoc);

            // List type - Whispering Wood (Season 7 Sigil)
            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\Item_S07_WitcherSigil.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemTypeWitcherSigil(ItemTypeConstants.WitcherSigil, itemTypeLoc);

            // List type - Escalating Sigil (Season 9 Sigil)
            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_DungeonKey_DungeonEscalation.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemTypeDungeonEscalation(ItemTypeConstants.DungeonEscalation, itemTypeLoc);

            // List type - Horadric Jewel (Season 9)
            jsonAsText = File.ReadAllText($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\ItemType_SeasonalSocketable.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            AddItemTypeHoradricJewel(ItemTypeConstants.HoradricJewel, itemTypeLoc);

            // Save
            SaveItemTypes(language);
        }

        private void SaveItemTypes(string language)
        {
            string fileName = $"Data/ItemTypes.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _itemTypeInfoList, options);
        }

        #endregion
    }
}
