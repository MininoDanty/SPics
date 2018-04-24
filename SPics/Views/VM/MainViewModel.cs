using Microsoft.Win32;
using SPics.Models;
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
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;


namespace SPics.Views.VM
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private string current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string XmlSettings = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SPicsSettings.xml";
        private string XmlTags = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\PicsTags.xml";


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


        private SPicsDirectory MyImgDirectory;


        public ICommand RunCommand => runCommand;
        private DelegateCommand runCommand;


        public ICommand AddImagesCommand => addImagesCommand;
        private DelegateCommand addImagesCommand;




        internal MainViewModel()
        {
            runCommand = new DelegateCommand(ExecuteRunCommand, () => { return true; });
            addImagesCommand = new DelegateCommand(ExecuteRunCommand, () => { return true; });

            MyImgDirectory = new SPicsDirectory();
            picsList = new ObservableCollection<Pic>();
            Window_Loaded();
        }

        private void ExecuteRunCommand()
        {
            Window_Loaded();
        }





        private void Window_Loaded()
        {
            // read path from settings
            XElement xDocSet = LoadSettings();
            
            MyImgDirectory.path = xDocSet.Value;
            XMLSETTINGS = xDocSet.Value;


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
                        Tags = new List<string>(tagList)
                    });
            }

            foreach (var item in MyImgDirectory.Files)
            {
                PicsList.Add(item);
            }

        }




        private void btnAdd_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = XMLSETTINGS;
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
                foreach (var file in selectedFiles)
                {
                    var name = file.Split('\\').Last();
                    File.Copy(file, XMLSETTINGS + "\\" + name);
                    var tmpFile = new XElement("Pic", name, new XAttribute("Tags", "soyunTag"));

                    d.Document.Element("Pics").Add(tmpFile);

                }

                File.WriteAllText(XmlTags, d.Document.ToString(), Encoding.UTF8);


            }
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
