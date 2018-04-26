using SPics.Models;
using SPics.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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


        private TreeViewModel tvContent;
        public TreeViewModel TvContent
        {
            get
            {
                return tvContent;
            }
            set
            {
                if (value != tvContent)
                {
                    tvContent = value;
                    NotifyPropertyChanged(nameof(TvContent));
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


        private TVMlevel selectedTvItem;
        public TVMlevel SelectedTvItem
        {
            get
            {
                return selectedTvItem;
            }
            set
            {
                if (value != selectedTvItem)
                {
                    selectedTvItem = value;
                    NotifyPropertyChanged(nameof(SelectedTvItem));
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

        public ICommand CreateFolderCommand => createFolderCommand;
        private DelegateCommand createFolderCommand;

        public ICommand AboutCommand => aboutCommand;
        private DelegateCommand aboutCommand;

        #endregion


        internal MainViewModel()
        {
            setSettingsCommand = new DelegateCommand(ExecuteSetSettingsCommand, () => { return true; });
            addImagesCommand = new DelegateCommand(ExecuteAddImages, () => { return true; });
            modifyImagesCommand = new DelegateCommand(ExecuteModifyImages, () => { return true; });
            createFolderCommand = new DelegateCommand(ExecuteCreateFolder, () => { return true; });
            aboutCommand = new DelegateCommand(ExecuteShowAbout, () => { return true; });

            MyImgDirectory = new SPicsDirectory();
            picsList = new ObservableCollection<Pic>();
            originalPicsList = new ObservableCollection<Pic>();

            LoadSavedImages();
        }


        private void ExecuteShowAbout()
        {
            About aForm = new About();
            aForm.ShowDialog();
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
                List<Pic> lst = new List<Pic>();

                // cambiar por loadtags() ???
                XDocument d = XDocument.Parse(File.ReadAllText(XmlTags));
                var t = d.Document;

                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                var selectedFiles = ofd.FileNames;                

                foreach (var item in selectedFiles)
                {
                    lst.Add(ExtMeth.NewPic(
                        item.Split('\\').Last(),
                        item
                        ));
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

        private void ExecuteCreateFolder()
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();

            if (selectedTvItem != null)
                fd.SelectedPath = SelectedTvItem.CompletePath;
            else
                fd.SelectedPath = MyImgDirectory.path;

            fd.ShowDialog();

            CreateTreeviewItems();
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


        private  TVMlevel GetDirectoryNodes(string path, string parent)
        {            
            var node = new TVMlevel
            {
                Name = path.Split('\\').LastOrDefault(),
                CompletePath = path,
                Parent = parent,
                Children = new ObservableCollection<TVMlevel>(),
            };

            var subDirs = Directory.GetDirectories(path).Select(d => GetDirectoryNodes(d, string.Join("\\", d.Split('\\').Reverse().Skip(1).Reverse()))).ToArray();

            foreach (var d in subDirs)
            {
                node.Children.Add(d);
            }

            return node;
        }
      
        private void CreateTreeviewItems()
        {
            var tmpDirs = Directory.GetDirectories(MyImgDirectory.path, "*", SearchOption.AllDirectories).ToList();
            
            // para los directorios de un mismo nivel
            //Dictionary<int, List<string>> dirsLevel = new Dictionary<int, List<string>>();
            //var tmpLvls = tmpDirs.Select(d => d.Split('\\').Count()).OrderBy(o => o).ToList();            

            //for (int i = 0; i < tmpDirs.Count(); i++)
            //{
            //    var currentLvl = tmpLvls[i];
            //    var dirsList = tmpDirs.Where(d => d.Split('\\').Count() == currentLvl).ToList();

            //    dirsLevel.Add(currentLvl, dirsList);

            //    // para saltarnos lo iguales le sumamos la cantidad de añadidos y le restamos uno para la próxima vuelta
            //    i += dirsList.Count - 1;
            //}            
            
            TVMlevel tmpLvl = new TVMlevel();
            tmpLvl = GetDirectoryNodes(MyImgDirectory.path, "root");

            //int index = dirsLevel.First().Key;
            //for (int i = 0; i < dirsLevel.Count; i++)
            //{
            //    var lvl = dirsLevel.Where(x => x.Key == index + i).FirstOrDefault().Key;
            //    var dirs = dirsLevel.Where(x => x.Key == index + i).FirstOrDefault().Value;

            //    var predecessors = dirsLevel.Where(x => x.Key == index + i - 1).FirstOrDefault().Value;               
            //}

            TreeViewModel tvm = new TreeViewModel
            {
                Children = new ObservableCollection<TVMlevel>()
                {
                    tmpLvl,
                }
            };            

            TvContent = tvm;
        }


        private void LoadImagesFromTMPfolder()
        {
            XElement xDocSet = LoadSettings();
            XElement xDocTags = LoadTags();

            MyImgDirectory.path = xDocSet.Value;
            MyImgDirectory.Files = new List<Pic>();
            
            // create treeview structure (left control)
            CreateTreeviewItems();

            // fill pics info, paths, tags, etc
            LoadPicsInfo(xDocTags, "Pic");

            // fill binding lists
            FillLists(MyImgDirectory.Files);           
        }

        private void LoadPicsInfo(XElement xDocTags, string tagToRead)
        {
            foreach (var item in xDocTags.Elements(tagToRead))
            {
                var tagList = item.FirstAttribute.Value.Split(',').ToList().Select(x => x.Trim());

                MyImgDirectory.Files.Add(ExtMeth.NewPic(
                    item.Value, 
                    MyImgDirectory.path + "\\" + item.Value, 
                    System.Drawing.Image.FromFile(MyImgDirectory.path + "\\" + item.Value),
                    new List<string>(tagList)
                    )); 
            }
        }        

        private void FillLists(List<Pic> sourceList)
        {
            foreach (var item in sourceList)
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
            return XElement.Parse(File.ReadAllText(XmlSettings.ExistsThisFile()));
        }

        private XElement LoadTags()
        {
            return XElement.Parse(File.ReadAllText(XmlTags.ExistsThisFile()));
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
