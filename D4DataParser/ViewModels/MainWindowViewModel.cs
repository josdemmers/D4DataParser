using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D4DataParser.Constants;
using D4DataParser.Entities;
using D4DataParser.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace D4DataParser.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<AffixInfo> _affixInfoList = new ObservableCollection<AffixInfo>();

        private AffixParser _affixParser = new AffixParser();
        private AspectParser _aspectParser = new AspectParser();
        private ItemTypeParser _itemTypeParser = new ItemTypeParser();
        private SigilParser _sigilParser = new SigilParser();
        private RuneParser _runeParser = new RuneParser();
        private UniqueParser _uniqueParser = new UniqueParser();
        private ImplicitParser _implicitParser = new ImplicitParser();
        private string _d4DataPath = DiabloPathConstants.Retail;
        private string _selectedDiabloVersion = string.Empty;

        // Filters
        private string _affixTypeTextFilter = string.Empty;
        private string _categoryTextFilter = string.Empty;
        private string _flagsTextFilter = string.Empty;
        private string _magicTypeTextFilter = string.Empty;
        private string _nameTextFilter = string.Empty;
        private string _snoTextFilter = string.Empty;
        private string _temperedTextFilter = string.Empty;
        private string _classTextFilter = string.Empty;
        private string _itemsTextFilter = string.Empty;
        private string _descriptionTextFilter = string.Empty;
        private string _descriptionCleanTextFilter = string.Empty;

        // Start of Constructors region

        #region Constructors

        public MainWindowViewModel()
        {
            // Init View commands
            ParseAffixDataCommand = new RelayCommand(ParseAffixDataExecute);
            ParseAspectDataCommand = new RelayCommand(ParseAspectDataExecute);
            ParseItemTypeDataCommand = new RelayCommand(ParseItemTypeDataExecute);
            ParseSigilDataCommand = new RelayCommand(ParseSigilDataExecute);
            ParseRuneDataCommand = new RelayCommand(ParseRuneDataExecute);
            ParseUniqueDataCommand = new RelayCommand(ParseUniqueDataExecute);
            ParseImplicitDataCommand = new RelayCommand(ParseImplicitDataExecute);
            ParseAllDataCommand = new RelayCommand(ParseAllDataExecute);

            // Init filter views
            CreateAffixInfoFilteredView();

            InitDiabloVersions();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public ObservableCollection<AffixInfo> AffixInfoList { get => _affixInfoList; set => _affixInfoList = value; }
        public ObservableCollection<string> DiabloVersions { get; } = new ObservableCollection<string>();
        public ListCollectionView? AffixInfoListFiltered { get; private set; }

        public ICommand ParseAffixDataCommand { get; }
        public ICommand ParseAspectDataCommand { get; }
        public ICommand ParseItemTypeDataCommand { get; }
        public ICommand ParseSigilDataCommand { get; }
        public ICommand ParseRuneDataCommand { get; }
        public ICommand ParseUniqueDataCommand { get; }
        public ICommand ParseImplicitDataCommand { get; }
        public ICommand ParseAllDataCommand { get; }

        public string AffixTypeTextFilter
        {
            get => _affixTypeTextFilter;
            set
            {
                SetProperty(ref _affixTypeTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string CategoryTextFilter
        {
            get => _categoryTextFilter;
            set
            {
                SetProperty(ref _categoryTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }
        
        public string FlagsTextFilter
        {
            get => _flagsTextFilter;
            set
            {
                SetProperty(ref _flagsTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string MagicTypeTextFilter
        {
            get => _magicTypeTextFilter;
            set
            {
                SetProperty(ref _magicTypeTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string NameTextFilter
        {
            get => _nameTextFilter;
            set
            {
                SetProperty(ref _nameTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string SNOTextFilter
        {
            get => _snoTextFilter;
            set
            {
                SetProperty(ref _snoTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string TemperedTextFilter
        {
            get => _temperedTextFilter;
            set
            {
                SetProperty(ref _temperedTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string ClassTextFilter
        {
            get => _classTextFilter;
            set
            {
                SetProperty(ref _classTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string ItemsTextFilter
        {
            get => _itemsTextFilter;
            set
            {
                SetProperty(ref _itemsTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string DescriptionTextFilter
        {
            get => _descriptionTextFilter;
            set
            {
                SetProperty(ref _descriptionTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string DescriptionCleanTextFilter
        {
            get => _descriptionCleanTextFilter;
            set
            {
                SetProperty(ref _descriptionCleanTextFilter, value);
                AffixInfoListFiltered?.Refresh();
            }
        }

        public string SelectedDiabloVersion 
        {
            get => _selectedDiabloVersion;
            set
            {
                SetProperty(ref _selectedDiabloVersion, value);
                switch (_selectedDiabloVersion)
                {
                    case DiabloVersionConstants.Ptr:
                        _d4DataPath = DiabloPathConstants.Ptr;
                        break;
                    case DiabloVersionConstants.Retail:
                        _d4DataPath = DiabloPathConstants.Retail;
                        break;
                    default:
                        _d4DataPath = DiabloPathConstants.Retail;
                        break;
                }
            }
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private void ParseAffixDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _affixParser.D4dataPath = _d4DataPath;
                _affixParser.Parse();

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    AffixInfoList.Clear();
                    foreach (var item in _affixParser.GetAffixInfoByLanguage("enUS"))
                    {
                        AffixInfoList.Add(item);
                    }
                });
            });
        }

        private void ParseAspectDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _aspectParser.D4dataPath = _d4DataPath;
                _aspectParser.Parse();
            });
        }

        private void ParseItemTypeDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _itemTypeParser.D4dataPath = _d4DataPath;
                _itemTypeParser.Parse();
            });
        }

        private void ParseSigilDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _sigilParser.D4dataPath = _d4DataPath;
                _sigilParser.Parse();
            });
        }

        private void ParseRuneDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _runeParser.D4dataPath = _d4DataPath;
                _runeParser.Parse();
            });
        }

        private void ParseUniqueDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _uniqueParser.D4dataPath = _d4DataPath;
                _uniqueParser.Parse();
            });
        }

        private void ParseImplicitDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                _implicitParser.D4dataPath = _d4DataPath;
                _implicitParser.Parse();
            });
        }

        private void ParseAllDataExecute()
        {
            Task.Factory.StartNew(() =>
            {
                // Affixes
                _affixParser.D4dataPath = _d4DataPath;
                _affixParser.Parse();

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    AffixInfoList.Clear();
                    foreach (var item in _affixParser.GetAffixInfoByLanguage("enUS"))
                    {
                        AffixInfoList.Add(item);
                    }
                });

                // Aspects
                _aspectParser.D4dataPath = _d4DataPath;
                _aspectParser.Parse();

                // Item types
                _itemTypeParser.D4dataPath = _d4DataPath;
                _itemTypeParser.Parse();

                // Sigils
                _sigilParser.D4dataPath = _d4DataPath;
                _sigilParser.Parse();

                // Runes
                _runeParser.D4dataPath = _d4DataPath;
                _runeParser.Parse();

                // Uniques
                _uniqueParser.D4dataPath = _d4DataPath;
                _uniqueParser.Parse();

                // Implicits
                _implicitParser.D4dataPath = _d4DataPath;
                _implicitParser.Parse();
            });
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void InitDiabloVersions()
        {
            DiabloVersions.Clear();
            DiabloVersions.Add(DiabloVersionConstants.Retail);
            DiabloVersions.Add(DiabloVersionConstants.Ptr);

            SelectedDiabloVersion = DiabloVersions[1];
        }

        private void CreateAffixInfoFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                AffixInfoListFiltered = new ListCollectionView(AffixInfoList)
                {
                    Filter = FilterAffixInfoList
                };
            });
        }

        private bool FilterAffixInfoList(object affixObj)
        {
            var allowed = true;
            if (affixObj == null) return false;

            AffixInfo affixInfo = (AffixInfo)affixObj;

            // AffixType
            var keywords = AffixTypeTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (keywords.Count > 0 && !keywords.Contains(affixInfo.AffixType.ToString()))
            {
                return false;
            }

            // Category
            keywords = CategoryTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (keywords.Count > 0 && !keywords.Contains(affixInfo.Category.ToString()))
            {
                return false;
            }

            // Flags
            keywords = FlagsTextFilter.Split(";",StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (keywords.Count > 0 && !keywords.Contains(affixInfo.Flags.ToString()))
            {
                return false;
            }

            // MagicType
            // Name
            keywords = NameTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                if (!affixInfo.IdName.ToLower().Contains(keyword.Trim().ToLower()))
                {
                    return false;
                }
            }
            // SNO
            // Tempered
            // Class
            keywords = ClassTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                string allowedForPlayerClassAsString = string.Join(",", affixInfo.AllowedForPlayerClass);

                if (!allowedForPlayerClassAsString.Equals(keyword.Trim()))
                {
                    return false;
                }
            }

            // Items
            keywords = ItemsTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                string allowedItemLabels = string.Join(",", affixInfo.AllowedItemLabels);

                if (!allowedItemLabels.Contains(keyword.Trim()))
                {
                    return false;
                }
            }

            // Description
            keywords = DescriptionTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                if (!affixInfo.Description.ToLower().Contains(keyword.Trim().ToLower()))
                {
                    return false;
                }
            }

            // DescriptionClean
            keywords = DescriptionCleanTextFilter.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                if (!affixInfo.DescriptionClean.ToLower().Contains(keyword.Trim().ToLower()))
                {
                    return false;
                }
            }

            return allowed;
        }

        #endregion
    }
}
