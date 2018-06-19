using ImageService.Modal;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Infrastructure.Enums;
using ImageService.Logging;
using ImageService.Logging.Modal;
using ImageService.Server;
using System.Text.RegularExpressions;

namespace ImageService.Controller.Handlers
{
    public class DirectoyHandler : IDirectoryHandler
    {
        #region Members
        private IImageController m_controller;              // The Image Processing Controller
        private ILoggingService m_logging;
        private FileSystemWatcher m_dirWatcher;             // The Watcher of the Dir
        private string m_path                             // The Path of directory
        #endregion
        public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;   // The Event That Notifies that the Directory is being closed
        public event EventHandler<CommandRecievedEventArgs> createFile;
        /// <summary>
        /// constructor.
        /// </summary>
        /// <param name="m_controller"> controller</param>
        /// <param name="m_logging">logger</param>
        public DirectoyHandler(IImageController m_controller, ILoggingService m_logging) {
            this.m_controller = m_controller;
            this.m_logging = m_logging;
            this.m_dirWatcher = new FileSystemWatcher();
        }
        /// <summary>
        /// startHandleDirectory Function.
        /// receive a path to a folder, and start to watch the folder
        /// if a file was add there. also add message to logger.
        /// </summary>
        /// <param name="dirPath"> name of full path of folder</param>
        public void StartHandleDirectory(string dirPath) {
            this.m_logging.Log("start to handle directory " + dirPath, MessageTypeEnum.INFO);
            this.m_path = dirPath;
            this.m_dirWatcher.Path = dirPath;
            this.m_dirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.FileName |
                    NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.LastWrite |
                    NotifyFilters.FileName;
            this.m_dirWatcher.Filter = "*.*";
            this.m_dirWatcher.Created += new FileSystemEventHandler(CreateNewFile);
            
            this.m_dirWatcher.EnableRaisingEvents = true;

        }
        /// <summary>
        /// OnCommandReceived Function.
        /// is called when there is a command of addFile. first check that the
        /// path is valid, and than execute the command. write to logger if it 
        /// finished with success or fail.
        /// </summary>
        /// <param name="sender">the sender of event, in this case handler</param>
        /// <param name="e">args of command</param>
        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            Boolean result = true;
            string message;
            if (e.RequestDirPath.Contains(this.m_path)) {
               message = this.m_controller.ExecuteCommand(e.CommandID, e.Args, out result);
               if (result)
                {
                    this.m_logging.Log(message, MessageTypeEnum.INFO);
                } else
                {
                    this.m_logging.Log(message, MessageTypeEnum.FAIL);
                }
            }
           
        }
        /// <summary>
        /// CreateNewFile Function.
        /// the function that is raised when there is a command of adding
        /// a new image. first check if the extension is one of the four
        /// that the service support. than create the args of path and name
        /// of command, and call OnCommandReceived.
        /// </summary>
        /// <param name="sender">sender, in this case server</param>
        /// <param name="e">args</param>
        public void CreateNewFile(object sender, FileSystemEventArgs e)
        {
            FileInfo file = new FileInfo(e.FullPath);
            if (!(file.Extension.Equals(".jpg") || file.Extension.Equals(".png") || file.Extension.Equals(".gif") ||
                file.Extension.Equals(".bmp")))
            {
                return;
            }
            string[] args = { e.FullPath, e.Name, };
            CommandRecievedEventArgs ce = new CommandRecievedEventArgs((int)CommandEnum.NewFileCommand,
                args, e.FullPath);
            this.OnCommandRecieved(this, ce);
        }
        /// <summary>
        /// OnStopHandle Function.
        /// is called when the handler should stop watching on this folder, 
        /// and decrease all the function that should be activated when event
        /// is raised.
        /// </summary>
        /// <param name="sender">sender, server</param>
        /// <param name="e">args of close dir</param>
        public void OnStopHandle(object sender, DirectoryCloseEventArgs e)
        {
            
            ((ImageServer)sender).CommandRecieved -= this.OnCommandRecieved;
            ((ImageServer)sender).ServerClose -= this.OnStopHandle;
            this.m_dirWatcher.EnableRaisingEvents = false;
            this.m_dirWatcher.Created -= new FileSystemEventHandler(CreateNewFile);
            this.m_dirWatcher.Dispose();
            this.m_logging.Log("handler " + this.m_path + " was closed successfully", MessageTypeEnum.INFO);
        }
        

    }
}
