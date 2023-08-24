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

        public ProcessListPageViewModel()
        {
            Process[] processArray = Process.GetProcesses();

            processList = processArray.Select(p => new ProcessViewModel(p)).ToList();
        }
    }
}
