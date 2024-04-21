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
    public class SigilParser
    {
        private string _d4datePath = string.Empty;
        private List<string> _languages = new List<string>();
        private NightmareDungeonMeta _nightmareDungeonMeta = new NightmareDungeonMeta();
        private SeasonMeta _seasonMeta = new SeasonMeta();
        private List<SigilInfo> _sigilInfoList = new List<SigilInfo>();
        private List<ArString> _zoneMetaList = new List<ArString>();

        // Start of Constructors region

        #region Constructors

        public SigilParser()
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

        public void ParseSigils()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            foreach (var language in _languages)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");

                _sigilInfoList.Clear();
                ParseSigilsByLanguage(language);
            }

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time: {watch.ElapsedMilliseconds}");
        }

        private void ParseSigilsByLanguage(string language)
        {
            // Nightmare dungeons - ".\d4data\json\base\meta\Global\nightmare_dungeons.glo.json"
            var jsonAsText = File.ReadAllText($"{_d4datePath}json\\base\\meta\\Global\\nightmare_dungeons.glo.json");
            _nightmareDungeonMeta = System.Text.Json.JsonSerializer.Deserialize<NightmareDungeonMeta>(jsonAsText) ?? new NightmareDungeonMeta();

            List<string> dungeonIds = new List<string>();
            foreach (var arDungeonList in _nightmareDungeonMeta.ptContent[0].arDungeonLists)
            {
                foreach (var dungeon in arDungeonList.arDungeons)
                {
                    dungeonIds.Add(dungeon.name);
                }
            }

            // Nightmare dungeons (Season) - ".\d4data\json\base\meta\Season\Season 3.sea.json"
            jsonAsText = File.ReadAllText($"{_d4datePath}json\\base\\meta\\Season\\Season 3.sea.json");
            _seasonMeta = System.Text.Json.JsonSerializer.Deserialize<SeasonMeta>(jsonAsText) ?? new SeasonMeta();

            foreach (var arDungeonList in _seasonMeta.arDungeonLists)
            {
                foreach (var dungeon in arDungeonList.arDungeons)
                {
                    dungeonIds.Add(dungeon.name);
                }
            }

            SigilInfo GetSigilInfoFromId(string dungeonId)
            {
                SigilInfo sigilInfo = new SigilInfo();

                var jsonAsText = File.ReadAllText($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\World_{dungeonId}.stl.json");
                var localisation = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();

                string name = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase)).szText;
                string description = localisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("Desc", StringComparison.OrdinalIgnoreCase)).szText;

                sigilInfo.IdSno = localisation.__snoID__;
                sigilInfo.Name = name;
                sigilInfo.Description = description;
                sigilInfo.Type = "Dungeon";

                return sigilInfo;
            }

            // Add all dungeon locations.
            // Missing DungeonZoneInfo property at this step. Will be added further in the update process.
            var dungeonIdsUnique = dungeonIds.Distinct().ToList();
            foreach (var dungeonId in dungeonIdsUnique)
            {
                _sigilInfoList.Add(GetSigilInfoFromId(dungeonId));
            }

            // Affixes (Positive, Minor, Major)
            // ".\d4data\json\enUS_Text\meta\StringList\DungeonAffix_Positive_AttackMoveSpeedOnKill.stl.json"
            // ".\d4data\json\enUS_Text\meta\StringList\DungeonAffix_Minor_Monster_AntiCC.stl.json"
            // ".\d4data\json\enUS_Text\meta\StringList\DungeonAffix_Major_Avenger.stl.json"
            var affixFiles = Directory.EnumerateFiles($"{_d4datePath}json\\{language}_Text\\meta\\StringList\\", "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.Contains("DungeonAffix_Positive_", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("DungeonAffix_Minor_", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("DungeonAffix_Major_", StringComparison.OrdinalIgnoreCase));

            var affixes = new List<Localisation>();
            foreach (var affixFile in affixFiles) 
            {
                jsonAsText = File.ReadAllText(affixFile);
                var affix = System.Text.Json.JsonSerializer.Deserialize<Localisation>(jsonAsText) ?? new Localisation();
                affixes.Add(affix);
            }

            SigilInfo GetSigilInfoFromAffixLocalisation(Localisation affixLocalisation)
            {
                SigilInfo sigilInfo = new SigilInfo();

                string fileName = affixLocalisation.__fileName__;
                string name = string.Empty;
                string description = string.Empty;
                if (fileName.Contains("_Positive_"))
                {
                    name = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixName", StringComparison.OrdinalIgnoreCase)).szText;
                    description = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixDesc", StringComparison.OrdinalIgnoreCase))?.szText;
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        description = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixDesc_Legendary", StringComparison.OrdinalIgnoreCase))?.szText;
                    }
                    sigilInfo.Type = "Positive";
                }
                else if (fileName.Contains("_Minor_"))
                {
                    name = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixName", StringComparison.OrdinalIgnoreCase)).szText;
                    description = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixDesc_Legendary", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
                    sigilInfo.Type = "Minor";
                }
                else if (fileName.Contains("_Major_"))
                {
                    name = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixName", StringComparison.OrdinalIgnoreCase)).szText;
                    description = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.StartsWith("AffixDesc", StringComparison.OrdinalIgnoreCase)).szText;
                    sigilInfo.Type = "Major";
                }

                sigilInfo.IdSno = affixLocalisation.__snoID__;
                sigilInfo.Name = name;
                sigilInfo.Description = description;

                return sigilInfo;
            }

            // Add all Postive, Minor, and Major affixes.
            // Missing IdName property at this step. Will be added further in the update process.
            foreach (var affix in affixes)
            {
                _sigilInfoList.Add(GetSigilInfoFromAffixLocalisation(affix));
            }

            // Add missing IdName
            int coreTOCIndex = 42;
            jsonAsText = File.ReadAllText($"{_d4datePath}json\\base\\CoreTOC.dat.json");
            var coreTOCDictionary = JsonSerializer.Deserialize<Dictionary<long, Dictionary<long, string>>>(jsonAsText);
            var sigilDictionary = coreTOCDictionary[coreTOCIndex];
            foreach (var sigilInfo in _sigilInfoList)
            {
                sigilInfo.IdName = sigilDictionary[sigilInfo.IdSno];
            }

            // Zones - ".\d4data\json\enUS_Text\meta\StringList\Zones.stl.json"
            string directory = $"{_d4datePath}json\\{language}_Text\\meta\\StringList\\";
            string fileNameLoc = $"{directory}Zones.stl.json";
            string prefix = string.Empty;
            jsonAsText = File.ReadAllText(fileNameLoc);
            _zoneMetaList = JsonSerializer.Deserialize<Localisation>(jsonAsText)?.arStrings ?? new List<ArString>();

            string GetSigilDungeonZoneInfo(string idName)
            {
                switch (idName)
                {
                    case var _ when idName.Contains("_S03_", StringComparison.OrdinalIgnoreCase):
                        return _zoneMetaList.FirstOrDefault(z => z.szLabel.Equals("ZONE_S03_HUB", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;

                    case var _ when idName.Contains("_Frac_",StringComparison.OrdinalIgnoreCase):
                        return _zoneMetaList.FirstOrDefault(z => z.szLabel.Equals("ZONE_FRACTURED_PEAKS", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
                    case var _ when idName.Contains("_Hawe_", StringComparison.OrdinalIgnoreCase):
                        return _zoneMetaList.FirstOrDefault(z => z.szLabel.Equals("ZONE_HAWAZAR_SWAMPS", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
                    case var _ when idName.Contains("_Kehj_", StringComparison.OrdinalIgnoreCase):
                        return _zoneMetaList.FirstOrDefault(z => z.szLabel.Equals("ZONE_KEHJISTAN", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
                    case var _ when idName.Contains("_Scos_", StringComparison.OrdinalIgnoreCase):
                        return _zoneMetaList.FirstOrDefault(z => z.szLabel.Equals("ZONE_SCOSGLEN", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
                    case var _ when idName.Contains("_Step_", StringComparison.OrdinalIgnoreCase):
                        return _zoneMetaList.FirstOrDefault(z => z.szLabel.Equals("ZONE_DRY_STEPPES", StringComparison.OrdinalIgnoreCase))?.szText ?? string.Empty;
                    default:
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Zone not found for {idName}");
                        break;
                }

                return string.Empty;
            }

            string GetSigilDungeonZoneInfoPrefix()
            {
                string directory = $"{_d4datePath}json\\{language}_Text\\meta\\StringList\\";
                string fileNameLoc = $"{directory}UIToolTips.stl.json";
                string prefix = string.Empty;
                var jsonAsText = File.ReadAllText(fileNameLoc);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var sigilLocalisationInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("ItemDungeonZoneInfo", StringComparison.CurrentCultureIgnoreCase));
                    if (sigilLocalisationInfo != null)
                    {
                        prefix = sigilLocalisationInfo.szText;
                    }
                }

                prefix = prefix.Replace("{c_white}", string.Empty);
                prefix = prefix.Replace("{/c_white}", string.Empty);
                prefix = prefix.Replace("{s1}", string.Empty);
                prefix = prefix.Replace("{s2}", string.Empty);
                prefix = prefix.Trim();

                return prefix;
            }

            // Add missing DungeonZoneInfo for sigils of type Dungeon
            string dungeonZoneInfoPrefix = GetSigilDungeonZoneInfoPrefix();
            foreach (var sigilInfo in _sigilInfoList.Where(s => s.Type.Equals("Dungeon")))
            {
                sigilInfo.DungeonZoneInfo = $"{dungeonZoneInfoPrefix} {GetSigilDungeonZoneInfo(sigilInfo.IdName)}";
            }

            SigilInfo GetCustomSigilInfoFromUITooltips(string idName)
            {
                SigilInfo sigilInfo = new SigilInfo();

                string directory = $"{_d4datePath}json\\{language}_Text\\meta\\StringList\\";
                string fileNameLoc = $"{directory}UIToolTips.stl.json";
                var jsonAsText = File.ReadAllText(fileNameLoc);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var sigilLocalisationInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(idName, StringComparison.CurrentCultureIgnoreCase));
                    if (sigilLocalisationInfo != null)
                    {
                        sigilInfo.IdSno = sigilLocalisationInfo.hLabel;
                        sigilInfo.IdName = sigilLocalisationInfo.szLabel;
                        sigilInfo.Name = sigilLocalisationInfo.szText;
                        sigilInfo.Description = string.Empty;
                        sigilInfo.Type = "Misc";
                    }
                }

                return sigilInfo;
            }

            // Add custom sigil affixes
            // - MonsterLevel, e.g. "Monster Level:"
            // - ItemDungeonAffixResses, e.g. "Revives Allowed:"
            _sigilInfoList.Add(GetCustomSigilInfoFromUITooltips("MonsterLevel"));
            _sigilInfoList.Add(GetCustomSigilInfoFromUITooltips("ItemDungeonAffixResses"));

            // Beautify names and descriptions
            foreach (var sigilInfo in _sigilInfoList)
            {
                sigilInfo.Name = sigilInfo.Name.Replace("{c_bonus}", string.Empty);
                sigilInfo.Name = sigilInfo.Name.Replace("{c_resource}", string.Empty);
                sigilInfo.Name = sigilInfo.Name.Replace("{/c}", string.Empty);
                sigilInfo.Name = sigilInfo.Name.Replace("{s1}", string.Empty);
            }

            // Sort
            _sigilInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            // Delete unwanted sigils
            _sigilInfoList.RemoveAll(s => s.IdName.Equals("DungeonAffix_Positive_S03_LTE_LunarNewYearShrine", StringComparison.OrdinalIgnoreCase));

            // Save
            SaveSigils(language);
        }

        private void SaveSigils(string language)
        {
            string fileName = $"Data/Sigils.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _sigilInfoList, options);
        }

        #endregion
    }
}
