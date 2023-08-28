using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessViewer
{
    partial class ProcessInfoPageViewModel : ObservableObject
    {
        System.Timers.Timer PageTimer;
        object TimerLock = new object();
        
        //Observable datafields
        [ObservableProperty]
        ProcessViewModel currentProcess;

        //Setter for the process ID that the view will use to pass the process to the viewmodel
        public int ProcessID
        {
            get => CurrentProcess.ProcessID;
            set
            {
                CurrentProcess = new ProcessViewModel(Process.GetProcessById(value));
            }
        }

        public ProcessInfoPageViewModel()
        {
            CurrentProcess = new ProcessViewModel(Process.GetCurrentProcess());

            PageTimer = new System.Timers.Timer(1000);
            PageTimer.Elapsed += PageTimer_Elapsed;
            PageTimer.Start();
        }

        public void PageLoaded()
        {
            PageTimer.Start();
        }

        public void PageUnloaded()
        {
            PageTimer.Stop();
        }

        private void PageTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            lock (TimerLock)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    CurrentProcess.Refresh();

                    if (!CurrentProcess.IsRunning)
                    {
                        PageTimer.Stop();
                        NavigationController.NavigateBackAndClearCurrentPageFromHistory();
                    }
                });
            }
        }
    }
}
