using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessViewer
{
    partial class ProcessInfoPageViewModel : ObservableObject
    {
        System.Timers.Timer PageTimer;
        object TimerLock = new object();
        bool IsDebugThreadRunning = false;
        //TODO: Make thread object to fill the debug box. Ensure the thread is closed on unload and restarted on load
        
        //Observable datafields
        [ObservableProperty]
        ProcessViewModel currentProcess;

        [ObservableProperty]
        string debugOutput;

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
            DebugOutput = "";

            PageTimer = new System.Timers.Timer(1000);
            PageTimer.Elapsed += PageTimer_Elapsed;
            PageTimer.Start();

            Thread debugLoopThread = new Thread(DebugThread);
            IsDebugThreadRunning = true;
            debugLoopThread.Start();
        }

        private void DebugThread()
        {
            Debug.WriteLine($"Debug thread started: {CurrentProcess.ProcessName}");
            while (IsDebugThreadRunning)
            {
                //Wait for a second for a new message to come. If it doesn't come, continue and check if the thread should close
                if (!DebugOutputController.WaitForDebugString(CurrentProcess.ProcessID))
                {
                    continue;
                }

                //The debug string is ready, so get it and add it to the debug output
                string debugString = DebugOutputController.GetDebugString(CurrentProcess.ProcessID);

                //TODO: Find a way to append the difference instead of copying the string everytime
                //TODO: Maybe find an alternative to TextBlock that is more efficient
                //TODO: Implement a button to clear the debug output of process and also a check box that can stagger the output to every second or two instead of 100 ms
                //https://github.com/dotnet/wpf/issues/5887
                //https://stackoverflow.com/questions/1192335/automatic-vertical-scroll-bar-in-wpf-textblock
                //https://stackoverflow.com/questions/18260702/textbox-appendtext-not-autoscrolling
                bool isSuccess = GC.TryStartNoGCRegion(250000000); //This is a crutch for games with extreme amounts of debug output
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    DebugOutput = debugString;
                });
                if (isSuccess)
                {
                    try //If this fails and throws exception, it is okay but, the program will begin freezing up again as the GC is allowed to run again. Currently unavoidable
                    {
                        GC.EndNoGCRegion(); 
                    }
                    catch { }
                }
                Thread.Sleep(100);
            }
            Debug.WriteLine($"Debug thread ended: {CurrentProcess.ProcessName}");
        }

        public void PageLoaded()
        {
            PageTimer.Start();
        }

        public void PageUnloaded()
        {
            PageTimer.Stop();
            IsDebugThreadRunning = false;
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
