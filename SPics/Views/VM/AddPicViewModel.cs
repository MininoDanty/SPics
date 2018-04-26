using SPics.Models;
using SPics.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SPics.Views.VM
{
    internal class AddPicViewModel : INotifyPropertyChanged
    {        
        
        internal List<Pic> Result_PicListAfterSave = new List<Pic>();

        private ObservableCollection<Pic> picsList;        
        public ObservableCollection<Pic> PicsList
        {
            get
            {
                return picsList;
            }
            set
            {
                if (value != picsList)
                {
                    picsList = value;
                    NotifyPropertyChanged(nameof(PicsList));
                }
            }
        }

        private Pic selectedPic;
        public Pic SelectedPic
        {
            get
            {
                return selectedPic;
            }
            set
            {
                if (value != selectedPic && value != null)
                {
                    // guardamos tags
                    if (selectedPic != null)
                    {
                        var res = SaveTags(PicsList.ToList(), selectedPic);
                    }

                    selectedPic = value;
                    NotifyPropertyChanged(nameof(SelectedPic));
                }
            }
        }

        public ICommand SaveCommand => saveCommand;
        private DelegateCommand saveCommand;


        internal AddPicViewModel(IEnumerable<Pic> Files)
        {
            saveCommand = new DelegateCommand(ExecuteSaveCommand, () => { return true; });
            picsList = new ObservableCollection<Pic>();

            foreach (var item in Files)
            {
                PicsList.Add(ExtMeth.NewPic(
                    item.Name, 
                    item.Path, 
                    item.Image, 
                    item.Tags, 
                    item.TagsAsString, 
                    item.TagsForUI
                    ));                    
            }
        }

        public List<Pic> SaveTags(List<Pic> lista, Pic p)
        {
            var myTempList = lista.ToList();

            var S = myTempList.Single(x => x.Name == p.Name);
            int index = myTempList.IndexOf(S);

            myTempList.Remove(S);
            myTempList.Insert(index, p);

            return myTempList;
        }

        public void ExecuteSaveCommand()
        {
            // Guardamos la selección actual para no perder el último elemento
            PicsList = new ObservableCollection<Pic>(SaveTags(PicsList.ToList(), SelectedPic));

            foreach (var item in PicsList)
            {
                var tags = item.TagsForUI.Split(',').Select(x => x.Trim()).ToList();
                item.Tags = tags;
                
                Result_PicListAfterSave.Add(item);
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
