using D4Companion.Constants;
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
        private string _d4datePath = string.Empty;
        private List<string> _languages = new List<string>();
        //private NightmareDungeonMeta _nightmareDungeonMeta = new NightmareDungeonMeta();
        //private SeasonMeta _seasonMeta = new SeasonMeta();
        private List<ItemTypeInfo> _itemTypeInfoList = new List<ItemTypeInfo>();
        //private List<ArString> _zoneMetaList = new List<ArString>();

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

        public string D4dataPath { get => _d4datePath; set => _d4datePath = value; }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        private void InitLocalisations()
        {
            _languages.Clear();
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

        public void ParseItemTypes()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            foreach (var language in _languages)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");

                //_sigilInfoList.Clear();
                ParseItemTypesByLanguage(language);
            }

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time: {watch.ElapsedMilliseconds}");
        }

        private void ParseItemTypesByLanguage(string language)
        {
            _itemTypeInfoList.Clear();

            // ItemQuality - ".\d4data\json\enUS_Text\meta\StringList\ItemQuality.stl.json"
            var jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemQuality.stl.json");
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
                localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Unique", StringComparison.OrdinalIgnoreCase)) ?? new()
            };

            void AddItemType(string type, string typeLoc)
            {
                foreach (var quality in qualities)
                {
                    foreach(var rarity in rarities)
                    {
                        _itemTypeInfoList.Add(new ItemTypeInfo
                        {
                            Name = $"{quality.szText} {rarity.szText} {typeLoc}".Trim(),
                            Type = type
                        });
                    }
                }
            }

            string CleanItemType(string typeLoc)
            {
                return typeLoc.Replace("[fp]", string.Empty).
                    Replace("[fs]", string.Empty).
                    Replace("[mp]", string.Empty).
                    Replace("[ms]", string.Empty).
                    Replace("[ns]", string.Empty).
                    Replace("[p]", string.Empty);
            }

            // List type
            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Amulet.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            string itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Amulet, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Axe.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Axe2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Boots.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Boots, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Bow.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Ranged, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_ChestArmor.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Chest, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Crossbow.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //itemTypeLoc = CleanItemType(itemTypeLoc);
            //AddItemType(ItemTypeConstants.Ranged, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Crossbow2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Ranged, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Dagger.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_DaggerOffHand.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //itemTypeLoc = CleanItemType(itemTypeLoc);
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Focus.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}\\meta\\StringList\\ItemType_FocusBookOffHand.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //itemTypeLoc = CleanItemType(itemTypeLoc);
            //AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Gloves.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Gloves, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Helm.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Helm, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Legs.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Pants, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Mace.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Mace2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Mace2HDruid.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //itemTypeLoc = CleanItemType(itemTypeLoc);
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_OffHandTotem.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Polearm.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Ring.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Ring, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Scythe.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Scythe2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Shield.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Offhand, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Staff.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_StaffDruid.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //itemTypeLoc = CleanItemType(itemTypeLoc);
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            //jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_StaffSorcerer.stl.json");
            //localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            //itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            //itemTypeLoc = CleanItemType(itemTypeLoc);
            //AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Sword.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Sword2H.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Wand.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);
            AddItemType(ItemTypeConstants.Weapon, itemTypeLoc);

            // List type - Sigil
            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_DungeonKey.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);

            foreach (var quality in qualities)
            {
                if (string.IsNullOrWhiteSpace(quality.szText)) continue;

                _itemTypeInfoList.Add(new ItemTypeInfo
                {
                    Name = $"{quality.szText} {itemTypeLoc}".Trim(),
                    Type = ItemTypeConstants.Sigil
                });
            }

            // List type - Extracted Aspect
            jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\ItemType_Essence.stl.json");
            localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
            itemTypeLoc = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
            itemTypeLoc = CleanItemType(itemTypeLoc);

            foreach (var quality in qualities)
            {
                foreach (var rarity in rarities) 
                {
                    if (!rarity.szLabel.Equals("Legendary")) continue;

                    _itemTypeInfoList.Add(new ItemTypeInfo
                    {
                        Name = $"{quality.szText} {rarity.szText} {itemTypeLoc}".Trim(),
                        Type = ItemTypeConstants.Aspect
                    });
                }
            }

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
