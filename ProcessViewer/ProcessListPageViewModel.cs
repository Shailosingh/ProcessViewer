using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProcessViewer
{
    partial class ProcessListPageViewModel : ObservableObject
    {
        [ObservableProperty]
        List<ProcessViewModel> processList;

        [ObservableProperty]
        bool hasMainWindowFilterEnabled;

        public ProcessListPageViewModel()
        {
            Process[] processArray = Process.GetProcesses();
            ProcessList = processArray.Select(p => new ProcessViewModel(p)).ToList();

            FilterChanged();
        }

        private void RefreshList()
        {
            Process[] processArray = Process.GetProcesses();
            ProcessList = processArray.Select(p => new ProcessViewModel(p)).ToList();
        }

        [RelayCommand]
        private void FilterChanged()
        {
            RefreshList();
                
            if (HasMainWindowFilterEnabled)
            {
                ProcessList = ProcessList.Where(p => p.MainWindowName != "~").ToList();
            }
        }
    }
}
