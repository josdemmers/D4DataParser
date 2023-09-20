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

namespace D4DataParser.Parsers
{
    public class SigilParser
    {
        private string _d4datePath = string.Empty;
        private List<string> _languages = new List<string>();
        private NightmareDungeonMeta _nightmareDungeonMeta = new NightmareDungeonMeta();
        private List<SigilInfo> _sigilInfoList = new List<SigilInfo>();

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

        public string D4datePath { get => _d4datePath; set => _d4datePath = value; }

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

            _sigilInfoList.Clear();
            foreach (var language in _languages)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");
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
                    description = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixDesc_Legendary", StringComparison.OrdinalIgnoreCase)).szText;
                    sigilInfo.Type = "Minor";
                }
                else if (fileName.Contains("_Major_"))
                {
                    name = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixName", StringComparison.OrdinalIgnoreCase)).szText;
                    description = affixLocalisation.arStrings.FirstOrDefault(s => s.szLabel.Equals("AffixDesc", StringComparison.OrdinalIgnoreCase)).szText;
                    sigilInfo.Type = "Major";
                }

                sigilInfo.IdSno = affixLocalisation.__snoID__;
                sigilInfo.Name = name;
                sigilInfo.Description = description;

                return sigilInfo;
            }

            foreach (var affix in affixes)
            {
                _sigilInfoList.Add(GetSigilInfoFromAffixLocalisation(affix));
            }

            // Beautify names and descriptions
            foreach(var sigilInfo in _sigilInfoList)
            {
                sigilInfo.Name = sigilInfo.Name.Replace("{c_bonus}", string.Empty);
                sigilInfo.Name = sigilInfo.Name.Replace("{/c}", string.Empty);
            }

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
