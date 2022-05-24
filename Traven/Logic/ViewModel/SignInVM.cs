using System;
using Traven.Commands;
using Traven.Logic.Model;
using Traven.Logic.ViewModel.classes;
using Traven.View;
using Traven.UOW;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Mvvm;

namespace Traven.Logic.ViewModel
{
    public class SignInVM : BindableBase
    {

        private MainWindowVM mainWindowVM;
        private SignInModel signInModel;

        public SignInModel SignInModel
        {
            get => signInModel;
            set => SetProperty(ref signInModel, value);
        }

        public ICommand setSignUpViewCommand;
        public ICommand SetSignUpViewCommand
        {
            get => setSignUpViewCommand ?? (setSignUpViewCommand = new RelayCommand(parameter => mainWindowVM.SetViewModel(VMEnum.SignUp)));
        }

        public ICommand SignInCommand { get; set; }

        public SignInVM(MainWindowVM window)
        {
            mainWindowVM = window;
            SignInModel = new SignInModel();
            SignInCommand = new RelayCommand(SignIn);

        }
        public SignInVM()
        {
            SignInModel = new SignInModel();
            SignInCommand = new RelayCommand(SignIn);

        }
        private void SignIn(object obj)
        {
            if (!(obj is PasswordBox passwordBox))
            {
                return;
            }

            SignInModel.Password = passwordBox.Password;
            using (UnitOfWork unit = new UnitOfWork())
            {
                if (String.IsNullOrEmpty(SignInModel.Nickname))
                {
                    MessageBox.Show("Nickname is required");
                    return;
                }
                if (!Regex.IsMatch(SignInModel.Nickname, @"(@)(.+)$"))
                {
                    MessageBox.Show("Incorrect nickname");
                    return;
                }

                var user = unit.UserRepository
                                .GetWithInclude(u => u.UserAuth)
                                .Where(u => u.UserAuth.Nickname == SignInModel.Nickname && u.UserAuth.Password == SignInModel.Password)
                                .FirstOrDefault();
                if (user != null)
                {
                    
                    if (unit.UserRepository
                                .GetWithInclude(u => u.UserAuth, u => u.Media)
                                .Where(u => u.UserAuth.Nickname == SignInModel.Nickname && u.UserAuth.Password == SignInModel.Password)
                                .FirstOrDefault() != null)
                    {
                        mainWindowVM.User = user;
                        CurrentUser.Id = mainWindowVM.User.Id;
                        if (mainWindowVM.User != null)
                        {
                            mainWindowVM.SetViewModel(VMEnum.Menu);
                            CurrentUser.Name = user.Name;
                            CurrentUser.Surname = user.Surname;
                            Media media = unit.MediaRepository.GetAll().Where(m => m.Id == user.Id).FirstOrDefault();
                            CurrentUser.Avatar = media.Path;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wrong nickname or password");
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect nickname or password");
                }
            }
        }
    }
}
