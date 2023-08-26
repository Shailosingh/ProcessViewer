using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.InteropServices;

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
        [NotifyPropertyChangedFor(nameof(FormattedPrivateWorkingSet))]
        long privateWorkingSetBytes;

        [ObservableProperty]
        int numberOfThreads;

        //Formatted datafields
        public string FormattedPrivateWorkingSet => $"{PrivateWorkingSetBytes / 1024:N0} K";

        //Constructor
        public ProcessViewModel(Process process)
        {
            //IN CONSTRUCTOR, IF MODULE PATH IS CAUGHT, THEN THIS PROCESS'S ACCESS IS DENIED. RECORD THIS AND DO SOMETHING ABOUT IT REGARDING IsRunning AND DataGrid!
            CurrentProcess = process;

            ProcessName = CurrentProcess.ProcessName;
            ProcessID = CurrentProcess.Id;
            PrivateWorkingSetBytes = CurrentProcess.PrivateMemorySize64;
            NumberOfThreads = CurrentProcess.Threads.Count;
            MainWindowName = CurrentProcess.MainWindowTitle == "" ? "~" : CurrentProcess.MainWindowTitle; //Puts ~ if there is no Main Window title

            try
            {
                ModulePath = CurrentProcess.MainModule?.FileName ?? "~"; //Puts ~ if NULL
            }
            catch (Exception e)
            {
                ModulePath = e.Message;
            }
        }

        public void DefaultValues()
        {
            ProcessName = "~";
            ProcessID = 0;
            MainWindowName = "~";
            ModulePath = "~";
            PrivateWorkingSetBytes = 0;
            NumberOfThreads = 0;
        }

        public void Refresh()
        {
            CurrentProcess.Refresh();

            try
            {
                PrivateWorkingSetBytes = CurrentProcess.PrivateMemorySize64;
                NumberOfThreads = CurrentProcess.Threads.Count;
                MainWindowName = CurrentProcess.MainWindowTitle == "" ? "~" : CurrentProcess.MainWindowTitle; //Puts ~ if there is no Main Window title
            }
            catch(Exception e)
            {
                DefaultValues();
            }

            //Module path, PID and Process Name never changes during the execution of a process, so we don't need to refresh it
        }

        public bool HasMainWindow()
        {
            return CurrentProcess.MainWindowHandle != IntPtr.Zero;
        }

        //Override equals--------------------------------------------------------------------------
        public override bool Equals(object? obj) => this.Equals(obj as ProcessViewModel);

        public bool Equals(ProcessViewModel? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.ProcessID == other.ProcessID;
        }

        public override int GetHashCode() => this.ProcessID.GetHashCode();
    }
}
