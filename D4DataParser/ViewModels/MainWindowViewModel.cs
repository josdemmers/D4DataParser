using D4DataParser.Entities;
using D4DataParser.Entities.D4Data;
using D4DataParser.Helpers;
using D4DataParser.Parsers;
using ImTools;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace D4DataParser.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private AffixParser _affixParser = new AffixParser();
        private AspectParser _aspectParser = new AspectParser();
        private SigilParser _sigilParser = new SigilParser();
        private ItemTypeParser _itemTypeParser = new ItemTypeParser();

        //private string _d4dataPath = @"D:\Games\DiabloIV\d4data\"; // Blizzhackers repo
        private string _d4dataPath = @"D:\Games\DiabloIV\d4data-dt\"; // DiabloTools repo
        private bool _keepDuplicates = false;

        // Start of Constructors region

        #region Constructors

        public MainWindowViewModel()
        {
            // Init View commands
            ParseAllCommand = new DelegateCommand(ParseAllExecute);
            ParseAffixDataCommand = new DelegateCommand(ParseAffixDataExecute);
            ParseAspectDataCommand = new DelegateCommand(ParseAspectDataExecute);
            ParseSigilDataCommand = new DelegateCommand(ParseSigilDataExecute);
            ParseItemTypesDataCommand = new DelegateCommand(ParseItemTypesDataExecute);
            TestCommand = new DelegateCommand(TestExecute);
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand ParseAllCommand { get; }
        public DelegateCommand ParseAffixDataCommand { get; }        
        public DelegateCommand ParseAspectDataCommand { get; }
        public DelegateCommand ParseSigilDataCommand { get; }
        public DelegateCommand ParseItemTypesDataCommand { get; }
        public DelegateCommand TestCommand { get; }

        public string D4dataPath
        {
            get => _d4dataPath;
            set
            {
                _d4dataPath = value;
                RaisePropertyChanged(nameof(D4dataPath));
            }
        }

        public bool KeepDuplicates
        {
            get => _keepDuplicates;
            set
            {
                _keepDuplicates = value;
                RaisePropertyChanged(nameof(KeepDuplicates));
            }
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private void ParseAllExecute()
        {
            Task.Factory.StartNew(() =>
            {
                KeepDuplicates = true;
                _affixParser.D4dataPath = D4dataPath;
                _affixParser.KeepDuplicates = KeepDuplicates;
                _affixParser.ParseAffixes();

                KeepDuplicates = false;
                _affixParser.D4dataPath = D4dataPath;
                _affixParser.KeepDuplicates = KeepDuplicates;
                _affixParser.ParseAffixes();

                _aspectParser.D4dataPath = D4dataPath;
                _aspectParser.ParseAffixes();

                _sigilParser.D4dataPath = D4dataPath;
                _sigilParser.ParseSigils();

                _itemTypeParser.D4dataPath = D4dataPath;
                _itemTypeParser.ParseItemTypes();
            });
        }

        private void ParseAffixDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _affixParser.D4dataPath = D4dataPath;
                _affixParser.KeepDuplicates = KeepDuplicates;
                _affixParser.ParseAffixes();

                // Copy affixes.glo.json
                //File.Copy($"{D4dataPath}json\\base\\meta\\Global\\affixes.glo.json", "Data\\affixes.glo.json", true);
            });
        }

        private void ParseAspectDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _aspectParser.D4dataPath = D4dataPath;
                _aspectParser.ParseAffixes();
            });
        }

        private void ParseSigilDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _sigilParser.D4dataPath = D4dataPath;
                _sigilParser.ParseSigils();
            });
        }

        private void ParseItemTypesDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _itemTypeParser.D4dataPath = D4dataPath;
                _itemTypeParser.ParseItemTypes();
            });
        }

        private void TestExecute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            // 2 seconds
            //var result = CSharpScript.EvaluateAsync("(1.3+(0.2*1)) / 100").Result;
            //Debug.WriteLine(result);

            double result = Convert.ToDouble(new DataTable().Compute("(1.3+(0.2*1)) / 100", null));
            Debug.WriteLine(result);

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        #endregion

        // Start of Methods region

        #region Methods

        #endregion

    }
}
