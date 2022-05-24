using Traven.Logic.Model;
using Traven.Logic.ViewModel.classes;
using Prism.Mvvm;
using Traven.Commands;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using Traven.UOW;
using System.Linq;
using Traven.View;

namespace Traven.Logic.ViewModel
{
    public class MenuVM : BindableBase
    {
        private UserModel user;
        public MainWindowVM mainWindowVM;
        private SignInModel signInModel;
        private List<MindMap> mindMaps;
        //private string avatar;
        //public string Name { get; set; }
        //public string Surname { get; set; }
        //public string Avatar
        //{
        //    get => avatar;
        //    set
        //    {
        //        SetProperty(ref avatar, value);
        //    }
        //}
        public List<MindMap> MindMaps
        {
            get { return mindMaps; }
            set { SetProperty(ref mindMaps, value); }
        }
        public SignInModel SignInModel
        {
            get => signInModel;
            set => SetProperty(ref signInModel, value);
        }
        public ICommand mindMapViewCommand;
        public ICommand MindMapCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand UserCommand { get; set; }
        public ICommand MindMapViewCommand
        {
            get => mindMapViewCommand ?? (mindMapViewCommand = new RelayCommand(parameter => mainWindowVM.SetViewModel(VMEnum.MindMapMain)));
        }
        public ICommand OpenMindMapCommand { get; set; }

        public UserModel User
        {
            get => user;
            set
            {
                user = value;
                SetProperty(ref user, value);
            }
        }
        private static bool loading = true;
        public MenuVM(MainWindowVM window)
        {
            mainWindowVM = window;
            MindMapCommand = new RelayCommand(Menu);
            ExitCommand = new RelayCommand(Exit);
            UserCommand = new RelayCommand(UserInfo);
            OpenMindMapCommand = new RelayCommand(OpenMindmap);
            if(loading)
            {
                loading = false;
            }
            else
            {
                using (UnitOfWork unit = new UnitOfWork())
                {
                    if(CurrentUser.Id == 2)
                    {
                        MindMaps = unit.MindMapRepository.GetAll().ToList();
                    }
                    else
                    {
                        User dbuser = unit.UserRepository.GetAll().FirstOrDefault(u => u.Id == CurrentUser.Id);
                        MindMaps = unit.MindMapRepository.GetWithInclude(m => m.Creator).Where(m => m.Creator.Id == dbuser.Id).ToList();
                    }
                }
            }
        }
        private void OpenMindmap(object obj)
        {
            if (!(obj is MindMap mindMap))
            {
                return;
            }
            CurrentUser.MapId = mindMap.Id;
            mainWindowVM.SetViewModel(VMEnum.MindMapMain);
        }
        private void Menu(object obj)
        {
            CurrentUser.MapId = -1;
            mainWindowVM.SetViewModel(VMEnum.MindMapMain);
        }
        private void Exit(object obj)
        {
            mainWindowVM.SetViewModel(VMEnum.SignIn);
        }  
        private void UserInfo(object obj)
        {
            UserInfo ui = new UserInfo();
            ui.Show();
        }
    }
}
