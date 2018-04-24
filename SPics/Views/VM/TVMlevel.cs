using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPics.Views.VM
{
    public class TVMlevel
    {
        public string Parent { get; set; }
        public string Name { get; set; }

        public ObservableCollection<TVMlevel> Children { get; set; }
    }
}
