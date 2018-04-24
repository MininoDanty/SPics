using SPics.Views.VM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SPics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void tbSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var vm = (MainViewModel)this.DataContext;

            var tb = (TextBox)sender;
            var r = MainViewModel.GetFilteredResultsByName(vm.OriginalPicsList.ToList(), tb.Text);
            vm.PicsList = new ObservableCollection<Models.Pic>(r);
        }

        private void tbSearchTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = (MainViewModel)this.DataContext;

            var tb = (TextBox)sender;
            var r = MainViewModel.GetFilteredResultsByTag(vm.OriginalPicsList.ToList(), tb.Text);
            vm.PicsList = new ObservableCollection<Models.Pic>(r);
        }

        private void tvDirs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = (MainViewModel)this.DataContext;
            vm.SelectedTvItem = (TVMlevel)e.NewValue;
        }



    }
}
