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
    /// Interaction logic for ProcessListPage.xaml
    /// </summary>
    public partial class ProcessListPage : Page
    {
        public ProcessListPage()
        {
            InitializeComponent();
            
            Debug.WriteLine("ProcessListPage constructed");
        }

        ~ProcessListPage()
        {
            Debug.WriteLine("ProcessListPage destructed");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ProcessListPageViewModel? viewModel = DataContext as ProcessListPageViewModel;
            viewModel?.PageUnloaded();
            
            Debug.WriteLine("ProcessListPage Unloaded");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessListPageViewModel? viewModel = DataContext as ProcessListPageViewModel;
            viewModel?.PageLoaded();
            
            Debug.WriteLine("ProcessListPage loaded");
        }
    }
}
