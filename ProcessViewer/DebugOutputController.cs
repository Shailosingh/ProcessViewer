using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessViewer
{
    static class DebugOutputController
    {
        //Constants
        private const string PROCESS_NAME = "Folderify";
        private const int BUFFER_SIZE = 4096;
        private static readonly byte[] EMPTY_BUFFER = new byte[BUFFER_SIZE];

        //Private internal datafields
        private static EventWaitHandle? DBWIN_BUFFER_READY;
        private static EventWaitHandle? DBWIN_DATA_READY;
        private static MemoryMappedFile? DBWIN_BUFFER;
        private static Dictionary<int, string>? PIDToDebugString;
        private static Dictionary<int, EventWaitHandle>? PIDToDebugEvent;
        private static object PIDToDebugStringLock = new object(); //NOTE: Two seperate locks are used here to ensure that the Debug String loop is never blocked by someone waiting on an event
        private static object GarbageCollectorLock = new object();
        private static bool IsRunning = false;

        public static void Start()
        {
            if(IsRunning)
            {
                return;
            }

            //Launch the debug loop as a thread
            Thread debugLoopThread = new Thread(DebugLoopThread);
            IsRunning = true;
            debugLoopThread.Start();

            //Launch a garbage collector thread so that every 10 seconds, the PIDToDebugString dictionary is cleared of old processes
        }

        public static void End()
        {
            if(!IsRunning)
            {
                return;
            }

            //Stop the debug loop
            IsRunning = false;
            if (DBWIN_DATA_READY != null)
            {
                DBWIN_DATA_READY.Set();
            }
        }

        public static string GetDebugString(int processID)
        {
            lock(GarbageCollectorLock)
            {
                lock (PIDToDebugStringLock)
                {
                    if (PIDToDebugString == null)
                    {
                        return string.Empty;
                    }

                    if (PIDToDebugString.ContainsKey(processID))
                    {
                        return PIDToDebugString[processID];
                    }

                    return string.Empty;
                }
            }   
        }

        public static bool WaitForDebugString(int processID, int timeout = 1000)
        {
            lock (GarbageCollectorLock)
            {
                if (PIDToDebugEvent == null)
                {
                    return false;
                }

                if (PIDToDebugEvent.ContainsKey(processID))
                {
                    return PIDToDebugEvent[processID].WaitOne(timeout);
                }

                return false;
            }  
        }

        private static void DebugLoopThread()
        {
            //Create events that manage debug buffer
            DBWIN_BUFFER_READY = new EventWaitHandle(true, EventResetMode.AutoReset, "DBWIN_BUFFER_READY");
            DBWIN_DATA_READY = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_DATA_READY");

            //Create shared memory segment
            DBWIN_BUFFER = MemoryMappedFile.CreateOrOpen("DBWIN_BUFFER", BUFFER_SIZE, MemoryMappedFileAccess.ReadWrite);

            //Create dictionary that maps PID to debug string
            PIDToDebugString = new Dictionary<int, string>();

            //Create dictionary that maps PID to events that signal new debug string has arrived
            PIDToDebugEvent = new Dictionary<int, EventWaitHandle>();

            while (IsRunning)
            {
                //Let applications know that the buffer is ready for debug input
                DBWIN_BUFFER_READY.Set();

                //Wait for the application to write to the buffer (this event will be set also when I decide to end the program and destroy this controller)
                DBWIN_DATA_READY.WaitOne();

                if (!IsRunning)
                {
                    break;
                }

                //Open the shared memory segment
                using (MemoryMappedViewStream stream = DBWIN_BUFFER.CreateViewStream())
                {
                    //Create a reader for the stream
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //Read the process ID 
                        int processID = reader.ReadInt32();

                        //Read the debug output into byte buffer
                        byte[] debugStringBuffer = reader.ReadBytes(BUFFER_SIZE - sizeof(UInt32));
                        debugStringBuffer[debugStringBuffer.Length - 1] = 0;

                        //Convert to ASCII
                        string debugOutput = Encoding.ASCII.GetString(debugStringBuffer).TrimEnd('\0');

                        lock (PIDToDebugStringLock)
                        {
                            //Add to dictionary and signal event
                            if (PIDToDebugString.ContainsKey(processID))
                            {
                                PIDToDebugString[processID] += debugOutput;
                                PIDToDebugEvent[processID].Set();
                            }
                            else
                            {
                                PIDToDebugString.Add(processID, debugOutput);
                                PIDToDebugEvent.Add(processID, new EventWaitHandle(true, EventResetMode.AutoReset));
                            }
                        }
                    }
                }

                //Clear entire memory segment
                using (MemoryMappedViewStream stream = DBWIN_BUFFER.CreateViewStream())
                {
                    stream.Write(EMPTY_BUFFER, 0, BUFFER_SIZE);
                }
            }

            DBWIN_BUFFER_READY.Close();
            DBWIN_DATA_READY.Close();
            DBWIN_BUFFER.Dispose();
        }

        private static void GarbageCollectionThread()
        {
            if (PIDToDebugString == null)
            {
                throw new Exception("PIDToDebugString is null");
            }
            if(PIDToDebugEvent == null)
            {
                throw new Exception("PIDToDebugEvent is null");
            }
            
            while(IsRunning)
            {
                //Get an array of all the PID keys of the dictionary
                int[] keys;
                lock (PIDToDebugStringLock)
                {
                    keys = PIDToDebugString.Keys.ToArray();
                }

                //Iterate through all the keys and remove any that are not running
                foreach (int key in keys)
                {
                    try
                    {
                        Process process = Process.GetProcessById(key);
                    }
                    catch(Exception)
                    {
                        //If an exception is thrown, then the process is not running, so remove it from the dictionaries
                        lock (GarbageCollectorLock)
                        {
                            lock(PIDToDebugString)
                            {
                                Debug.WriteLine($"Removing PID '{key}' from debug controller");
                                PIDToDebugString.Remove(key);
                                PIDToDebugEvent[key].Close();
                            }
                            
                        }
                    }
                }

                //Wait 10 seconds before checking again
                Thread.Sleep(10000);
            }
        }
    }
}
