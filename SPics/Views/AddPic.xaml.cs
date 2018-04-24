using SPics.Models;
using SPics.Views.VM;
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

namespace SPics.Views
{
    /// <summary>
    /// Interaction logic for AddPic.xaml
    /// </summary>
    public partial class AddPic : Window
    {
        public AddPic()
        {
            InitializeComponent();
        }

        private void lvPics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = (AddPicViewModel)this.DataContext;
            vm.ExecuteSaveCommand();

            this.Close();
        }
    }
}
