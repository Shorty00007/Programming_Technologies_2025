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
using BookStore.Presentation.ViewModels;

namespace BookStore.Presentation.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();

            PasswordInput.PasswordChanged += (s, e) =>
            {
                if (DataContext is RegisterViewModel vm)
                {
                    vm.Form.Password = PasswordInput.Password;
                }
            };
        }
    }
}