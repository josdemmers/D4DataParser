using D4DataParser.Entities;
using D4DataParser.Entities.D4Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace D4DataParser.Parsers
{
    public class ParagonParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<ParagonBoardInfo>> _paragonBoardInfo = new Dictionary<string, List<ParagonBoardInfo>>();
        private Dictionary<string, List<ParagonGlyphInfo>> _paragonGlyphInfo = new Dictionary<string, List<ParagonGlyphInfo>>();

        // Start of Constructors region

        #region Constructors

        public ParagonParser()
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

            // Reset
            _paragonBoardInfo.Clear();
            _paragonGlyphInfo.Clear();

            foreach (var language in _languages)
            {
                if (Directory.Exists($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");
                    ParseByLanguage(language);
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped {language}, not available.");
                }
            }
        }

        private void ParseByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // ParagonBoardInfo
            var paragonBoardInfoList = new List<ParagonBoardInfo>();
            _paragonBoardInfo[language] = paragonBoardInfoList;

            string directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            var fileEntries = Directory.EnumerateFiles(directory).Where(filePath => Path.GetFileNameWithoutExtension(filePath).StartsWith("ParagonBoard_Paragon_", StringComparison.OrdinalIgnoreCase));
            foreach (var fileEntry in fileEntries)
            {
                string fileNameLocalisation = fileEntry;
                var jsonAsText = File.ReadAllText(fileNameLocalisation);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    ParagonBoardInfo paragonBoardInfo = new ParagonBoardInfo();
                    paragonBoardInfo.IdName = localisation.__fileName__.Split("/")[3]
                        .Replace("ParagonBoard_", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace(".stl", string.Empty, StringComparison.OrdinalIgnoreCase);
                    var localisationName = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase));
                    paragonBoardInfo.Name = localisationName?.szText ?? string.Empty;
                    paragonBoardInfoList.Add(paragonBoardInfo);
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (ParagonBoardInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // ParagonGlyphInfo
            var paragonGlyphInfoList = new List<ParagonGlyphInfo>();
            _paragonGlyphInfo[language] = paragonGlyphInfoList;

            fileEntries = Directory.EnumerateFiles(directory).Where(filePath => Path.GetFileNameWithoutExtension(filePath).StartsWith("ParagonGlyph_", StringComparison.OrdinalIgnoreCase));
            foreach (var fileEntry in fileEntries)
            {
                string fileNameLocalisation = fileEntry;
                var jsonAsText = File.ReadAllText(fileNameLocalisation);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    ParagonGlyphInfo paragonGlyphInfo = new ParagonGlyphInfo();
                    paragonGlyphInfo.IdName = localisation.__fileName__.Split("/")[3]
                        .Replace("ParagonGlyph_", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace(".stl", string.Empty, StringComparison.OrdinalIgnoreCase);
                    var localisationName = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase));
                    paragonGlyphInfo.Name = localisationName?.szText ?? string.Empty;
                    paragonGlyphInfoList.Add(paragonGlyphInfo);
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (ParagonGlyphInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            SaveParagonBoards(language);
            SaveParagonGlyphs(language);

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void SaveParagonBoards(string language)
        {
            string fileName = $"Data/ParagonBoards.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _paragonBoardInfo[language], options);
        }

        private void SaveParagonGlyphs(string language)
        {
            string fileName = $"Data/ParagonGlyphs.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _paragonGlyphInfo[language], options);
        }

        #endregion
    }
}
