using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SPics.Views.VM
{
    public class TreeViewModel
    {
        public ObservableCollection<TVMlevel> Children { get; set; }

    }
}
