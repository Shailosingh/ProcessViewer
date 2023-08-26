using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProcessViewer
{
    partial class ProcessListPageViewModel : ObservableObject
    {
        System.Timers.Timer PageTimer;
        object RefreshLock = new object(); //This is used to ensure that the refresh command can only be used once

        [ObservableProperty]
        ObservableCollection<ProcessViewModel> processList;

        [ObservableProperty]
        bool hasMainWindowFilterEnabled = true;

        public ProcessListPageViewModel()
        {
            Process[] processArray = Process.GetProcesses();
            ProcessList = new ObservableCollection<ProcessViewModel>(processArray.Select(p => new ProcessViewModel(p)));
            RefreshList();
            PageTimer = new System.Timers.Timer(1000);
            PageTimer.Elapsed += PageTimerElapsed;
            PageTimer.Start();
        }

        private void PageTimerElapsed(Object? source, System.Timers.ElapsedEventArgs e)
        {
            lock(RefreshLock)
            {
                RefreshList();
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            PageTimer.Stop();
            lock (RefreshLock)
            {
                RefreshList();
            }
            PageTimer.Start();
        }

        private void RefreshList()
        {
            //Get new array of processes and create hashset of process IDs
            Process[] newProcessArray = Process.GetProcesses();
            HashSet<int> newProcessIDs = new HashSet<int>(newProcessArray.Select(p => p.Id));

            //Iterate through every process in the list and if it is not in the hashset, remove it from the list. Also remove it from the list if it is to be filtered
            for (int index = 0; index < ProcessList.Count; index++)
            {
                bool isFilteredOut = HasMainWindowFilterEnabled && !ProcessList[index].HasMainWindow();
                if (isFilteredOut || !newProcessIDs.Contains(ProcessList[index].ProcessID))
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ProcessList.RemoveAt(index);
                        index--;
                    });
                }
            }

            //Get a hashset of the process IDs in the remaining list
            HashSet<int> remainingProcessIDs = new HashSet<int>(ProcessList.Select(p => p.ProcessID));

            //Iterate through every process in the new array and if it is not in the hashset of remaining processes, add it to the list, as it is a new process
            //Also consider the filter. If it is filtered out, do not add it to the list
            foreach (Process process in newProcessArray)
            {
                bool isFilteredOut = HasMainWindowFilterEnabled && (process.MainWindowHandle == IntPtr.Zero);
                if (!isFilteredOut && !remainingProcessIDs.Contains(process.Id))
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ProcessList.Add(new ProcessViewModel(process));
                    });
                }
            }

            //Now we have every active process. We must refresh each of them to get the latest data
            foreach (ProcessViewModel process in ProcessList)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    process.Refresh();
                });
            }
        }
    }
}
