using SPics.Views.VM;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SPics
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static WindowController wController;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            wController = new WindowController();
            wController.AddMappings(WindowMapping.Mappings);

            Window main = new MainWindow();
            main.DataContext = new MainViewModel();

            main.Show();
        }
    }




    public static class WindowMapping
    {
        public static IEnumerable<WindowMap> Mappings => new WindowMap[]
        {
            new WindowMap<MainWindow, MainViewModel>(),
            //new WindowMap<Views.Settings, SettingsViewModel>(),
            //new WindowMap<Views.ProgressBar, ProgressBarViewModel>()
        };
    }

    public class WindowMap
    {
        //public static Dictionary<string, object> AllWindows { get; set; } = new Dictionary<string, object>();

        public WindowMap(Type windowType, Type modelType)
        {
            WindowType = windowType;
            ModelType = modelType;
        }

        public Type WindowType { get; }
        public Type ModelType { get; }
    }

    public class WindowMap<TWindow, TModel> : WindowMap
        where TWindow : FrameworkElement
    {
        public WindowMap() : base(typeof(TWindow), typeof(TModel))
        {
        }
    }


    public interface IWindowController
    {
        bool ShowWindow(object viewModel);

        void ShowWindowAsync(object viewModel);
    }

    public class WindowController : IWindowController
    {
        private Window MainWindow => Application.Current.MainWindow;

        public WindowController()
        {
        }

        public void AddMappings(IEnumerable<WindowMap> mappings)
        {
            foreach (var mapping in mappings)
                this.mappings.Add(mapping.ModelType, mapping.WindowType);
        }

        public bool ShowWindow(object viewModel)
        {
            Type windowType;
            if (!mappings.TryGetValue(viewModel.GetType(), out windowType))
                throw new ArgumentException("No existe ventana para el tipo: " + viewModel.GetType(), nameof(viewModel));

            Window owner = this.MainWindow;

            Window window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;

            window.Owner = owner;

            bool? result = window.ShowDialog();
            window.DataContext = null;
            return result == true;
        }

        public void ShowWindowAsync(object viewModel)
        {
            Type windowType;
            if (!mappings.TryGetValue(viewModel.GetType(), out windowType))
                throw new ArgumentException("No existe ventana para el tipo: " + viewModel.GetType(), nameof(viewModel));

            Window owner = this.MainWindow;

            Window window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;

            window.Owner = owner;

            bool? result = window.ShowDialog();
            window.DataContext = null;
        }

        private readonly IDictionary<Type, Type> mappings = new Dictionary<Type, Type>();
    }


}
