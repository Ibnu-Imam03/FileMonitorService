using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace FileMonitorService
{
    public partial class FileMonitorService : ServiceBase
    {

        private string _SourceFolder;
        private string _DestinationFolder;
        private string _LogFolder;

        private FileSystemWatcher _Watcher;


        public FileMonitorService()
        {
            InitializeComponent();

            ServiceName = "FileMonitorService";
            CanStop = true;
            AutoLog = true;

        }

        private void InitializeDirectories()
        {
            Directory.CreateDirectory(_SourceFolder);
            Directory.CreateDirectory(_DestinationFolder);
            Directory.CreateDirectory(_LogFolder);
        }
        private void LoadConfiguration()
        {
            _SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            _DestinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];
            _LogFolder = ConfigurationManager.AppSettings["LogFolder"];

            if (string.IsNullOrWhiteSpace(_SourceFolder)) throw new ConfigurationErrorsException("SourceFolder is missing or empty.");

            if (string.IsNullOrWhiteSpace(_DestinationFolder)) throw new ConfigurationErrorsException("DestinationFolder is missing or empty.");

            if (string.IsNullOrWhiteSpace(_LogFolder)) throw new ConfigurationErrorsException("LogFolder is missing or empty.");

            // Convert paths to absolute paths.
            _SourceFolder = Path.GetFullPath(_SourceFolder);
            _DestinationFolder = Path.GetFullPath(_DestinationFolder);
            _LogFolder = Path.GetFullPath(_LogFolder);

            // Source and destination must be different.
            if (string.Equals(_SourceFolder, _LogFolder, StringComparison.OrdinalIgnoreCase) || string.Equals(_DestinationFolder, _LogFolder, StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationErrorsException("Log folder must be different from source and destination folders.");
            }
        }


        private void InitializeFileWatcher()
        {
            _Watcher = new FileSystemWatcher();

            _Watcher.Path = _SourceFolder;
            _Watcher.Filter = "*";
            _Watcher.IncludeSubdirectories = false;

            _Watcher.NotifyFilter = NotifyFilters.FileName;

            _Watcher.Created += OnFileCreated;

            _Watcher.EnableRaisingEvents = true;

            _Watcher.Renamed += OnFileRenamed;
        }
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                Logger.Log($"File renamed or downloaded: {e.FullPath}");
                ProcessFile(e.FullPath);
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                Logger.Log($"New file detected: {e.FullPath}");

                ProcessFile(e.FullPath);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LoadConfiguration();
                InitializeDirectories();

                Logger.Initialize(_LogFolder);
                Logger.Log("File Monitor Service started.");

                InitializeFileWatcher();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(_LogFolder))
                {
                    Logger.Log($"Service startup failed: {ex.Message}", "ERROR");
                }

                throw;
            }

        }
        private void ProcessFile(string sourceFilePath)
        {
            try
            {
                string extension = Path.GetExtension(sourceFilePath);
                string newFileName = Guid.NewGuid().ToString() + extension;

                string destinationFilePath =Path.Combine(_DestinationFolder, newFileName);

                // Retry if another process is still using the file.
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        File.Move(sourceFilePath, destinationFilePath);

                        Logger.Log($"File processed successfully: {destinationFilePath}");
                        return;
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                Logger.Log($"File processing failed: File is still in use: {sourceFilePath}", "ERROR");
            }
            catch (Exception ex)
            {
                Logger.Log($"File processing failed: {ex.Message}", "ERROR");
            }
        }


        protected override void OnStop()
        {
            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = false;

                _Watcher.Dispose();

                _Watcher = null;
            }
            Logger.Log("File Monitor Service stopped.");

        }
    }
}
