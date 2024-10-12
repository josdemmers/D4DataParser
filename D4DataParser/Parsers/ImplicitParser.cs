using D4DataParser.Entities.D4Data;
using D4DataParser.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace D4DataParser.Parsers
{
    public class ImplicitParser
    {
        private string _d4dataPath = string.Empty;

        // Start of Constructors region

        #region Constructors

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

        public void Parse()
        {
            // Parse .\d4data\json\base\meta\Item\
            string directory = $"{_d4dataPath}json\\base\\meta\\Item\\";
            if (Directory.Exists(directory))
            {
                // 1HAxe
                List<string> implicits = GetImplicitAffixes($"{directory}1HAxe_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HAxe");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HDagger
                implicits = GetImplicitAffixes($"{directory}1HDagger_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HDagger");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HFocus
                implicits = GetImplicitAffixes($"{directory}1HFocus_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HFocus");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HMace
                implicits = GetImplicitAffixes($"{directory}1HMace_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HMace");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HScythe
                implicits = GetImplicitAffixes($"{directory}1HScythe_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HScythe");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HShield
                implicits = GetImplicitAffixes($"{directory}1HShield_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HShield");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HSword
                implicits = GetImplicitAffixes($"{directory}1HSword_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HSword");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HTotem
                implicits = GetImplicitAffixes($"{directory}1HTotem_Legendary_Druid_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HTotem");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 1HWand
                implicits = GetImplicitAffixes($"{directory}1HWand_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 1HWand");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HAxe
                implicits = GetImplicitAffixes($"{directory}2HAxe_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HAxe");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HBow
                implicits = GetImplicitAffixes($"{directory}2HBow_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HBow");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HCrossbow
                implicits = GetImplicitAffixes($"{directory}2HCrossbow_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HCrossbow");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HGlaive
                implicits = GetImplicitAffixes($"{directory}X1_2HGlaive_Legendary_Spiritborn_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HGlaive");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HMace
                implicits = GetImplicitAffixes($"{directory}2HMace_Legendary_Generic_002.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HMace");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HPolearm
                implicits = GetImplicitAffixes($"{directory}2HPolearm_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HPolearm");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HQuarterstaff
                implicits = GetImplicitAffixes($"{directory}X1_2HQuarterstaff_Legendary_Spiritborn_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HQuarterstaff");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HScythe
                implicits = GetImplicitAffixes($"{directory}2HScythe_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HScythe");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HStaff
                implicits = GetImplicitAffixes($"{directory}2HStaff_Legendary_Druid_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HStaff");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // 2HSword
                implicits = GetImplicitAffixes($"{directory}2HSword_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: 2HSword");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Amulet
                implicits = GetImplicitAffixes($"{directory}Amulet_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Amulet");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Boots
                implicits = GetImplicitAffixes($"{directory}Boots_Legendary_Generic_050.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Boots");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Chest
                implicits = GetImplicitAffixes($"{directory}Chest_Legendary_Generic_050.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Chest");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Gloves
                implicits = GetImplicitAffixes($"{directory}Gloves_Legendary_Generic_050.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Gloves");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Helm
                implicits = GetImplicitAffixes($"{directory}Helm_Legendary_Generic_050.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Helm");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Pants
                implicits = GetImplicitAffixes($"{directory}Pants_Legendary_Generic_050.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Pants");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }

                // Ring
                implicits = GetImplicitAffixes($"{directory}Ring_Legendary_Generic_001.itm.json");
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Ring");
                foreach (var affix in implicits)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affix}");
                }
            }
        }

        private List<string> GetImplicitAffixes(string fileLocation)
        {
            List<string> implicits = new List<string>();

            using (FileStream? stream = File.OpenRead(fileLocation))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    //options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new UIntConverter());

                    var itemMetaJson = JsonSerializer.Deserialize<ItemMeta>(stream, options) ?? new ItemMeta();
                    var arInherentAffixes = itemMetaJson.arInherentAffixes;
                    if (arInherentAffixes != null)
                    {
                        implicits.AddRange(arInherentAffixes.Select(a => a.name));
                    }
                }
            }

            return implicits;
        }

        #endregion
    }
}
