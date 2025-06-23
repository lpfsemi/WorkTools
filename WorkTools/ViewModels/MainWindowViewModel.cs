using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using WorkTools.Views.Pages;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace WorkTools.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _applicationTitle = String.Empty; 

        [ObservableProperty]
        private string _applicationAuthor = "Hello";

        [ObservableProperty]
        private string _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ApplicationTitle = "IOT";

            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Home",
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home24,
                    PageType = typeof(DashboardPage)
                },
                new NavigationItem()
                {
                    Content = "Data",
                    PageTag = "data",
                    Icon = SymbolRegular.DataHistogram24,
                    PageType = typeof(DataPage)
                },
                new NavigationItem()
                {
                    Content = "PLC Diagnosis",
                    PageTag = "plc",
                    Icon = SymbolRegular.DataArea20,
                    PageType = typeof(PLCPage)
                },
                new NavigationItem()
                {
                    Content = "Control Instrument",
                    PageTag = "controlInstrument",
                    Icon = SymbolRegular.ConferenceRoom20,
                    PageType = typeof(Views.Pages.ControlInstrumentPage)
                },
                new NavigationItem()
                {
                    Content = "Recipe",
                    PageTag = "recipe",
                    Icon = SymbolRegular.DataFunnel20,
                    PageType = typeof(Views.Pages.RecipePage)
                }
            };

            NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Settings",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings24,
                    PageType = typeof(Views.Pages.SettingsPage)
                }
            };

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };

            _isInitialized = true;
        }
    }
}
