using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.Infrastructure.Enums;
using ImageService.Logging;
using ImageService.Modal;
using System;
using System.Configuration;


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
        public String[] handlersArray { get; set; }
        #endregion
        
        #region Properties
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          // The event that notifies about a new Command being recieved
        public event EventHandler<DirectoryCloseEventArgs> ServerClose;            // the event that notifies about server closing to relavant handlers
        #endregion
        /// <summary>
        /// constructor.
        /// read the list of folder that should have a handler and create one
        /// for them.
        /// </summary>
        /// <param name="modal">ImageServiceModal</param>
        /// <param name="logging">LoggingModal</param>
        public ImageServer(IImageServiceModal modal, ILoggingService logging)
        {
            this.m_controller = new ImageController(modal, logging);
            this.m_logging = logging;
            this.handlersArray = (ConfigurationManager.AppSettings["handler"]).Split(';');
            foreach (string path in handlersArray)
            {
                this.CreateHandler(path);
            }

        }
        /// <summary>
        /// CreateHandler function.
        /// create a handler that listen to event of command of newFile
        /// and an event of of closing server, start to handle the directory
        /// and update the logger.
        /// </summary>
        /// <param name="path"> path of wanted directory</param>
        public void CreateHandler(string path)
        {
            try
            {
                IDirectoryHandler handler = new DirectoyHandler(this.m_controller, this.m_logging);
                this.CommandRecieved += handler.OnCommandRecieved;
                this.ServerClose += handler.OnStopHandle;
                handler.StartHandleDirectory(path);
            } catch (Exception e)
            {
                this.m_logging.Log(e.ToString(), Logging.Modal.MessageTypeEnum.FAIL);
                return;
            }
            this.m_logging.Log("Handler" + path + "was created successfully", Logging.Modal.MessageTypeEnum.INFO);
        }
        /// <summary>
        /// closeServer function.
        /// raise and event of close server to all active handler, and notify
        /// the logger if success or failuer.
        /// </summary>
        public void CloseServer()
        {
            // invoke all relevant handlers to being closed
            try
            {
                this.m_logging.Log("start to close server", Logging.Modal.MessageTypeEnum.INFO);
                this.ServerClose?.Invoke(this, new DirectoryCloseEventArgs("path", "server is close"));
                this.m_logging.Log("the server was closed successfully", Logging.Modal.MessageTypeEnum.INFO);
            } catch (Exception e)
            {
                this.m_logging.Log(e.ToString(), Logging.Modal.MessageTypeEnum.FAIL);
            }
        } 
       
        // add function of send command
        public void SendCommand(CommandRecievedEventArgs e)
        {
            this.CommandRecieved?.Invoke(this, e);
        }

    }
}
