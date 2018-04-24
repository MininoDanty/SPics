using Microsoft.Win32;
using SPics.Models;
using SPics.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;


namespace SPics.Views.VM
{
    //https://stackoverflow.com/questions/14444285/listview-with-treeviewitems-in-xaml
    internal class MainViewModel : INotifyPropertyChanged
    {
        private string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string XmlSettings = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SPicsSettings.xml";
        private string XmlTags = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\PicsTags.xml";

        #region bindings

        public string XMLSETTINGS
        {
            get
            {
                return XmlSettings;
            }
            set
            {
                if (value != XmlSettings)
                {
                    XmlSettings = value;
                    NotifyPropertyChanged(nameof(XMLSETTINGS));
                }
            }
        }

        private string searchText = "";
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (value != null)
                {
                    //var result = GetFilteredResults(PicsList.ToList(), value);                    
                    //PicsList = new ObservableCollection<Pic>(result);

                    searchText = value;
                    NotifyPropertyChanged(nameof(SearchText));
                }
            }
        }

        private ObservableCollection<Pic> originalPicsList;
        public ObservableCollection<Pic> OriginalPicsList
        {
            get
            {
                return originalPicsList;
            }
            set
            {
                if (value != originalPicsList)
                {
                    originalPicsList = value;
                    NotifyPropertyChanged(nameof(OriginalPicsList));
                }
            }
        }


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


        private SPicsDirectory myImgDirectory;
        public SPicsDirectory MyImgDirectory
        {
            get
            {
                return myImgDirectory;
            }
            set
            {
                if (value != myImgDirectory)
                {
                    myImgDirectory = value;
                    NotifyPropertyChanged(nameof(MyImgDirectory));
                }
            }
        }

        #endregion

        #region commands

        public ICommand SetSettingsCommand => setSettingsCommand;
        private DelegateCommand setSettingsCommand;

        public ICommand AddImagesCommand => addImagesCommand;
        private DelegateCommand addImagesCommand;

        public ICommand ModifyImagesCommand => modifyImagesCommand;
        private DelegateCommand modifyImagesCommand;

        #endregion


        internal MainViewModel()
        {
            setSettingsCommand = new DelegateCommand(ExecuteSetSettingsCommand, () => { return true; });
            addImagesCommand = new DelegateCommand(ExecuteAddImages, () => { return true; });
            modifyImagesCommand = new DelegateCommand(ExecuteModifyImages, () => { return true; });

            MyImgDirectory = new SPicsDirectory();
            picsList = new ObservableCollection<Pic>();
            originalPicsList = new ObservableCollection<Pic>();

            LoadSavedImages();
        }

        private void ExecuteSetSettingsCommand()
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.SelectedPath = MyImgDirectory.path;
            fd.ShowDialog();

            var xml = LoadSettings();
            xml.Value = fd.SelectedPath;
            xml.Save(XmlSettings);

            MyImgDirectory.path = fd.SelectedPath;
        }



        private void ExecuteAddImages()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.InitialDirectory = MyImgDirectory.path;
            ofd.Multiselect = true;

            if ((bool)ofd.ShowDialog())
            {
                XDocument d = XDocument.Parse(File.ReadAllText(XmlTags));
                var t = d.Document;

                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                var selectedFiles = ofd.FileNames;

                List<Pic> lst = new List<Pic>();

                foreach (var item in selectedFiles)
                {
                    lst.Add(new Pic
                        {
                            Path = item,
                            Image = null,
                            Name = item.Split('\\').Last(),
                            Tags = null,
                            TagsAsString = null,
                            TagsForUI = null                    
                        });
                }                

                AddPicViewModel AddPVM = new AddPicViewModel(lst);

                AddPic f = new AddPic();
                f.DataContext = AddPVM;
                f.ShowDialog();

                var myResult = AddPVM.Result_PicListAfterSave;

                if (myResult.Count > 0)
                {
                    foreach (var file in myResult)
                    {
                        File.Copy(file.Path, MyImgDirectory.path + "\\" + file.Name);
                        var tmpFile = new XElement("Pic", file.Name, new XAttribute("Tags", file.TagsAsString));

                        d.Document.Element("Pics").Add(tmpFile);

                    }

                    File.WriteAllText(XmlTags, d.Document.ToString(), Encoding.UTF8);
                    LoadImagesFromTMPfolder();
                }
            }



        }

        private void ExecuteModifyImages()
        {            
            AddPicViewModel AddPVM = new AddPicViewModel(OriginalPicsList.ToList());

            AddPic f = new AddPic();
            f.DataContext = AddPVM;
            f.ShowDialog();

            var myResult = AddPVM.Result_PicListAfterSave;

            if (myResult.Count > 0)
            {
            }

                //foreach (var file in myResult)
                //{
                //File.Copy(file.Path, MyImgDirectory.path + "\\" + file.Name);
                //var tmpFile = new XElement("Pic", file.Name, new XAttribute("Tags", file.TagsAsString));

                //d.Document.Element("Pics").Add(tmpFile);

                //}

                //File.WriteAllText(XmlTags, d.Document.ToString(), Encoding.UTF8);

                //LoadImagesFromTMPfolder();
            }



        private void LoadSavedImages()
        {
            try
            {
                LoadImagesFromTMPfolder();
            }
            catch
            {
                ExecuteSetSettingsCommand();
                LoadImagesFromTMPfolder();
            }
        }



        private void LoadImagesFromTMPfolder()
        {
            // read path from settings
            XElement xDocSet = LoadSettings();

            MyImgDirectory.path = xDocSet.Value;
            //XMLSETTINGS = xDocSet.Value;


            // read info
            XElement xDocTags = LoadTags();

            MyImgDirectory.Files = new List<Pic>();

            foreach (var item in xDocTags.Elements("Pic"))
            {
                var tagList = item.FirstAttribute.Value.Split(',').ToList().Select(x => x.Trim());

                MyImgDirectory.Files.Add(
                    new Pic
                    {
                        Name = item.Value,
                        Path = MyImgDirectory.path + "\\" + item.Value,
                        Image = Image.FromFile(MyImgDirectory.path + "\\" + item.Value),
                        Tags = new List<string>(tagList),
                        TagsAsString = null,
                        TagsForUI = null                       
                    });
            }

            foreach (var item in MyImgDirectory.Files)
            {
                OriginalPicsList.Add(item);
                PicsList.Add(item);
            }
        }




        public static List<Pic> GetFilteredResultsByName(List<Pic> originalList, string filter)
        {
            List<Pic> NewList = new List<Pic>();

            foreach (var p in originalList)
            {
                if (p.Name.ToUpper().Contains(filter.ToUpper()))
                    NewList.Add(p);                
            }

            return NewList.ToList();
        }


        public static List<Pic> GetFilteredResultsByTag(List<Pic> originalList, string filter)
        {
            List<Pic> NewList = new List<Pic>();

            foreach (var p in originalList)
            {
                if (p.TagsAsString.ToUpper().Contains(filter.ToUpper()))
                    NewList.Add(p);
            }

            return NewList.ToList();
        }




        private XElement LoadSettings()
        {
            return XElement.Parse(File.ReadAllText(XmlSettings));
        }


        private XElement LoadTags()
        {
            return XElement.Parse(File.ReadAllText(XmlTags));
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
