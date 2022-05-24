using System;
using System.Windows;
using System.Windows.Input;

using Prism.Mvvm;
using Traven.Commands;
using Traven.Logic.Model;

namespace Traven.Logic.ViewModel
{
    public class MainWindowVM : BindableBase
    {
        private Uri selectedLanguage = new Uri("Languages\\Eng.xaml", UriKind.Relative);
        private Uri selectedThemes = new Uri("Themes\\Standart.xaml", UriKind.Relative);
        public User User { get; set; }

        private BindableBase _currentViewModel;
        public BindableBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set { SetProperty(ref _currentViewModel, value); }
        }
        private WindowState _windowState;
        public WindowState WindowState
        {
            get { return _windowState; }
            set { SetProperty(ref _windowState, value); }
        }

        private WindowStyle _windowStyle;
        public WindowStyle WindowStyle
        {
            get { return _windowStyle; }
            set { SetProperty(ref _windowStyle, value); }
        }
        private SignInVM signInVM { get; set; }
        private SignUpVM signUpVM { get; set; }
        public MenuVM menuVM { get; set; }
        private MindMapMainVM mapVM { get; set; }
        private RelayCommand<VMEnum> setViewCommand;
        public ICommand SetViewCommand
        {
            get => setViewCommand ?? (setViewCommand = new RelayCommand<VMEnum>(SetViewModel));
        }
        private RelayCommand toggleFullscreenCommand;
        public ICommand ToggleFullscreenCommand
        {
            get => toggleFullscreenCommand ?? (toggleFullscreenCommand = new RelayCommand(ToggleFullscreen));
        }
        private RelayCommand exitCommand;
        public ICommand ExitCommand
        {
            get => exitCommand ?? (exitCommand = new RelayCommand(o => App.Current.Shutdown()));
        }
        public ICommand LanguagesCommand { get; set; }
        public ICommand ThemesCommand { get; set; }

        private void ToggleFullscreen(object obj)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public void SetViewModel(VMEnum vm)
        {

            switch (vm)
            {
                case VMEnum.SignIn:
                    CurrentViewModel = signInVM;
                    break;

                case VMEnum.SignUp:
                    CurrentViewModel = signUpVM;
                    break;

                case VMEnum.Menu:
                    CurrentViewModel = new MenuVM(this);
                    break;
                case VMEnum.MindMapMain:
                    CurrentViewModel = mapVM;
                    break;
            }
        }

        public MainWindowVM()
        {
            signInVM = new SignInVM(this);
            signUpVM = new SignUpVM(this);
            menuVM = new MenuVM(this);
            mapVM = new MindMapMainVM(this);
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal;
            int userId = Properties.Settings.Default.UserId;
            SetViewModel(User != null ? VMEnum.Menu : VMEnum.SignIn);
            LanguagesCommand = new RelayCommand(Languages);
            ThemesCommand = new RelayCommand(Themes);
        }
        private void Languages(object obj)
        {
            if (selectedLanguage.OriginalString == "Languages\\Eng.xaml")
            {
                selectedLanguage = new Uri("Languages\\Rus.xaml", UriKind.Relative);
                LoadResourceDictionariesL();
            }
            else
            {
                selectedLanguage = new Uri("Languages\\Eng.xaml", UriKind.Relative);
                LoadResourceDictionariesL();
            }
        }

        private void Themes(object obj)
        {
            if (selectedThemes.OriginalString == "Themes\\Standart.xaml")
            {
                selectedThemes = new Uri("Themes\\Dark.xaml", UriKind.Relative);
                LoadResourceDictionariesTh();
            }
            else
            {
                selectedThemes = new Uri("Themes\\Standart.xaml", UriKind.Relative);
                LoadResourceDictionariesTh();
            }
        }

        private void LoadResourceDictionariesL()
        {
            ResourceDictionary langthem = new ResourceDictionary();
            langthem.Source = selectedLanguage;
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(langthem);
        }
        private void LoadResourceDictionariesTh()
        {
            ResourceDictionary langthem = new ResourceDictionary();
            langthem.Source = selectedThemes;
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(langthem);
        }
    }
}
