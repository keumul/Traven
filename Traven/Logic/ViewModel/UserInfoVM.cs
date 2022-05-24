using Traven.Logic.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Traven.Commands;
using Traven.View;

namespace Traven.Logic.ViewModel
{
    public class UserInfoVM : BindableBase
    {
         private UserModel user;
        private string avatar;
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Avatar
        {
            get => avatar;
            set => SetProperty(ref avatar, value);
        }
        public UserModel User
        {
            get => user;
            set
            {
                SetProperty(ref user, value);
            }
        }
        public UserInfoVM()
        {
            Name = CurrentUser.Name;
            Surname = CurrentUser.Surname;
            Avatar = CurrentUser.Avatar;
        }
    }
}
