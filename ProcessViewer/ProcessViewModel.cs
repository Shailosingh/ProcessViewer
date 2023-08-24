using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ProcessViewer
{
    partial class ProcessViewModel : ObservableObject
    {
        //Private internal datafields
        Process CurrentProcess;

        //Observable datafields
        [ObservableProperty]
        string processName;

        [ObservableProperty]
        int processID;

        [ObservableProperty]
        string mainWindowName;

        [ObservableProperty]
        string modulePath;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedWorkingSet))]
        long workingSetBytes;

        [ObservableProperty]
        int numberOfThreads;

        //Formatted datafields
        public string FormattedWorkingSet => $"{WorkingSetBytes / 1024:N0} K";

        //Constructors
        public ProcessViewModel(Process process)
        {
            CurrentProcess = process;

            ProcessName = CurrentProcess.ProcessName;
            ProcessID = CurrentProcess.Id;
            MainWindowName = CurrentProcess.MainWindowTitle == "" ? "~" : CurrentProcess.MainWindowTitle; //Puts ~ if there is no Main Window title
            try
            {
                ModulePath = CurrentProcess.MainModule?.FileName ?? "~"; //Puts ~ if NULL
            }
            catch(Exception e)
            {
                ModulePath = e.Message;
            }
            WorkingSetBytes = CurrentProcess.WorkingSet64;
            NumberOfThreads = CurrentProcess.Threads.Count;
        }

        public void Refresh()
        {
            CurrentProcess.Refresh();

            ProcessName = CurrentProcess.ProcessName;
            ProcessID = CurrentProcess.Id;
            MainWindowName = CurrentProcess.MainWindowTitle;
            ModulePath = CurrentProcess.MainModule?.FileName ?? "~";
            WorkingSetBytes = CurrentProcess.WorkingSet64;
            NumberOfThreads = CurrentProcess.Threads.Count;
        }
    }
}
