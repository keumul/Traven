using Traven.Commands;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.Model;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Mvvm;
using Traven.UOW;
using System.Windows;
using System.Text.RegularExpressions;
using System;
using System.Linq;

namespace Traven.Logic.ViewModel
{
    public class SignUpVM : BindableBase
    {
        private MainWindowVM mainWindowVM;
        private SignUpModel signUpModel;
        public SignUpModel SignUpModel
        {
            get => signUpModel;
            set => SetProperty(ref signUpModel, value);
        }
        public ICommand SignUpCommand { get; set; }
        public ICommand ChooseImageCommand { get; set; }

        public ICommand setSignInViewCommand;
        public ICommand SetSignInViewCommand
        {
            get => setSignInViewCommand ?? (setSignInViewCommand = new RelayCommand(parameter => mainWindowVM.SetViewModel(VMEnum.SignIn)));
        }
        public SignUpVM(MainWindowVM mainWindowVM)
        {
            this.mainWindowVM = mainWindowVM;
            SignUpModel = new SignUpModel();
            SignUpCommand = new RelayCommand(SignUp);
            ChooseImageCommand = new RelayCommand(ChooseImage);
        }
        public SignUpVM()
        {
            SignUpModel = new SignUpModel();
            SignUpCommand = new RelayCommand(SignUp);
            ChooseImageCommand = new RelayCommand(ChooseImage);
        }
        private void ChooseImage(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                SignUpModel.Avatar = openFileDialog.FileName;
            }
        }
        private void SignUp(object obj)
        {
            using (UnitOfWork unit = new UnitOfWork())
            {
                if (!(obj is PasswordBox password))
                {
                    return;
                }
                if (String.IsNullOrEmpty(SignUpModel.Mail))
                {
                    MessageBox.Show("Nickname is required");
                    return;
                }
                if (!Regex.IsMatch(SignUpModel.Mail, @"(@)(.+)$"))
                {
                    MessageBox.Show("Incorrect nickname");
                    return;
                }
                if (String.IsNullOrEmpty(SignUpModel.Name))
                {
                    MessageBox.Show("Name is required");
                    return;
                }
                if (!Regex.IsMatch(SignUpModel.Name, @"^[A-ZА-Я][A-Za-zА-Яа-я]+$"))
                {
                    MessageBox.Show("Incorrect name");
                    return;
                }
                if (String.IsNullOrEmpty(SignUpModel.Surname))
                {
                    MessageBox.Show("Surname is required");
                    return;
                }
                if (!Regex.IsMatch(SignUpModel.Surname, @"^[A-ZА-Я][A-Za-zА-Яа-я]+$"))
                {
                    MessageBox.Show("Incorrect surname");
                    return;
                }

                if (password.Password != SignUpModel.RepeatPassword)
                {
                    MessageBox.Show("Passwords do not match");
                    return;
                }
                SignUpModel.Password = password.Password;
                SignUpModel.RepeatPassword = password.Password;
                if (unit.UserRepository
                                .GetWithInclude(u => u.UserAuth)
                                .Where(u => u.UserAuth.Nickname == SignUpModel.Mail && u.UserAuth.Password == SignUpModel.Password)
                                .FirstOrDefault() != null)
                {
                    MessageBox.Show("User with this nickname already exists");
                    return;
                }
                    UserAuth userAuth = new UserAuth()
                {
                    Nickname = SignUpModel.Mail,
                    Password = SignUpModel.Password,
                };
                unit.UserAuthRepository.Create(userAuth);

                Media media = new Media()
                {
                    Path = SignUpModel.Avatar
                };
                unit.MediaRepository.Create(media);

                User user = new User()
                {
                    Name = SignUpModel.Name,
                    Surname = SignUpModel.Surname,
                    Media = media,
                    UserAuth = userAuth
                };
                unit.UserRepository.Create(user);

                unit.Save();
            }
        }
    }
}
