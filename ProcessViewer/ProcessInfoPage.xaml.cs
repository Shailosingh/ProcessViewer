using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessViewer
{
    /// <summary>
    /// Interaction logic for ProcessInfoPage.xaml
    /// </summary>
    public partial class ProcessInfoPage : Page
    {
        public ProcessInfoPage(int processID)
        {
            InitializeComponent();
            ((ProcessInfoPageViewModel)DataContext).ProcessID = processID;

            Debug.WriteLine("ProcessInfoPage constructed");
        }

        ~ProcessInfoPage()
        {
            Debug.WriteLine("ProcessInfoPage destructed");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ProcessInfoPageViewModel? viewModel = DataContext as ProcessInfoPageViewModel;
            viewModel?.PageUnloaded();

            Debug.WriteLine("ProcessInfoPage Unloaded");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessInfoPageViewModel? viewModel = DataContext as ProcessInfoPageViewModel;
            viewModel?.PageLoaded();

            Debug.WriteLine("ProcessInfoPage loaded");
        }
    }
}
