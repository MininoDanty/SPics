using System.Collections.ObjectModel;

namespace SPics.Views.VM
{
    public class TVMlevel
    {
        public string Parent { get; set; }
        public string Name { get; set; }
        public string CompletePath { get; set; }
        public ObservableCollection<TVMlevel> Children { get; set; }
    }
}
