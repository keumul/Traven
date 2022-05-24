using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Traven.Logic.Model;

namespace Traven.View
{
    /// <summary>
    /// Логика взаимодействия для SignUp.xaml
    /// </summary>
    public partial class SignUp : UserControl
    {

        public SignUp()
        {
            InitializeComponent();
            User u = new User();
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).SignUpModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void repeatPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).SignUpModel.RepeatPassword = ((PasswordBox)sender).Password;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
        }
        private void MailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
