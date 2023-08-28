using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
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
        ICollectionView processListCollectionView;

        [ObservableProperty]
        bool hasMainWindowFilterEnabled = true;

        public ProcessListPageViewModel()
        {
            Process[] processArray = Process.GetProcesses();
            ProcessList = new ObservableCollection<ProcessViewModel>(processArray.Select(p => new ProcessViewModel(p)));
            ProcessListCollectionView = CollectionViewSource.GetDefaultView(ProcessList);
            RefreshList();
            PageTimer = new System.Timers.Timer(1000);
            PageTimer.Elapsed += PageTimerElapsed;
            PageTimer.Start();
        }

        private void PageTimerElapsed(Object? source, System.Timers.ElapsedEventArgs e)
        {
            RefreshList();
        }

        [RelayCommand]
        private void Refresh()
        {
            PageTimer.Stop();
            RefreshList();
            PageTimer.Start();
        }

        private void RefreshList()
        {
            //Ensure the refresh is only happening one at a time, so no race conditions happen with the data being altered
            lock (RefreshLock)
            {
                //Get new array of processes and create hashset of process IDs
                Process[] newProcessArray = Process.GetProcesses();

                //Get a hashset of the process IDs in the list
                HashSet<int> processIDs = new HashSet<int>(ProcessList.Select(p => p.ProcessID));

                //Iterate through every process in the new array and if it is not in the hashset of current processes, add it to the list, as it is a new process
                //Also consider the filter. If it is filtered out, do not add it to the list
                foreach (Process process in newProcessArray)
                {
                    bool isFilteredOut = HasMainWindowFilterEnabled && (process.MainWindowHandle == IntPtr.Zero);
                    if (!isFilteredOut && !processIDs.Contains(process.Id))
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

                //Iterate through every process in the current list and remove it if it is not running or if it is filtered out
                for (int index = 0; index < ProcessList.Count; index++)
                {
                    bool isFilteredOut = HasMainWindowFilterEnabled && !ProcessList[index].HasMainWindow();
                    if (isFilteredOut || !ProcessList[index].IsRunning)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ProcessList.RemoveAt(index);
                            index--;
                        });
                    }
                }
            }
        }

        [RelayCommand]
        private void SelectedProcess(ProcessViewModel selectedProcess)
        {
            NavigationController.NavigateToPage(new ProcessInfoPage(selectedProcess.ProcessID));
            
        }

        public void PageLoaded()
        {
            PageTimer.Start();
        }

        public void PageUnloaded()
        {
            PageTimer.Stop();
        }
    }
}
