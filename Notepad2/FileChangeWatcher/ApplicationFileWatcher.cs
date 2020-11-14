﻿using Notepad2.FileExplorer;
using Notepad2.InformationStuff;
using Notepad2.Notepad;
using Notepad2.Preferences;
using Notepad2.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notepad2.FileChangeWatcher
{
    /// <summary>
    /// A class for watching multiple FileWatchers and checking if their associated
    /// document's contents have changed. <see cref="FileWatcher"/>s do not work on their own,
    /// they require this class.
    /// </summary>
    public static class ApplicationFileWatcher
    {
        private static List<FileWatcher> Watchers { get; set; }

        public static bool IsRunning { get; set; }

        static ApplicationFileWatcher()
        {
            Watchers = new List<FileWatcher>();
        }

        public static void AddDocumentToWatcher(FileWatcher d)
        {
            if (!Watchers.Contains(d))
                Watchers.Add(d);
        }

        public static void RemoveDocumentFromWatcher(FileWatcher d)
        {
            Watchers.Remove(d);
        }

        public static void RunDocumentWatcher()
        {
            IsRunning = true;
            Task.Run(async () =>
            {
                while (IsRunning)
                {
                    if (GlobalPreferences.ENABLE_FILE_WATCHER)
                    {
                        try
                        {
                            // for loop because async modifications stuff
                            for (int i = 0; i < Watchers.Count; i++)
                            {
                                FileWatcher watcher = Watchers[i];
                                if (watcher != null && watcher.IsEnabled && watcher.Document.FilePath.IsFile())
                                {
                                    FileInfo fInfo = new FileInfo(watcher.Document.FilePath);
                                    if (watcher.Document.FileSizeBytes != fInfo.Length)
                                    {
                                        // Changed
                                        watcher.FileContentsChanged?.Invoke();
                                    }
                                    if (PreferencesG.CHECK_FILENAME_CHANGES_IN_DOCUMENT_WATCHER && watcher.Document.FileName != fInfo.Name)
                                    {
                                        watcher.FileNameChanged?.Invoke(fInfo.Name);
                                    }
                                }
                            }
                            // Breaks the special drag drop. not using.
                            //else
                            //{
                            //    FilePathChanged?.Invoke();
                            //}
                        }
                        catch (Exception e)
                        {
                            Information.Show($"Exception in File Watcher: {e.Message}", "FileWatcher");
                        }
                    }

                    await Task.Delay(2000);
                }
            });
        }

        public static void StopDocumentWatcher()
        {
            IsRunning = false;
        }
    }
}
