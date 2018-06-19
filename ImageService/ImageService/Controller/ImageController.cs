using ImageService.Commands;
using ImageService.Infrastructure;
using ImageService.Infrastructure.Enums;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Logging;


namespace ImageService.Controller
{
    public class ImageController : IImageController
    {
        private IImageServiceModal m_modal;                      // The Modal Object
        private Dictionary<int, ICommand> commands;
        /// <summary>
        /// constructor.
        /// init a dictionary with key of command of newFile.
        /// </summary>
        /// <param name="modal">modal</param>
        public ImageController(IImageServiceModal modal, ILoggingService logging)
        {
            m_modal = modal;                    // Storing the Modal Of The System
            commands = new Dictionary<int, ICommand>()
            {

                {(int) CommandEnum.NewFileCommand, new NewFileCommand(modal) },
                {(int) CommandEnum.GetConfigCommand, new GetConfigCommand() },
                {(int) CommandEnum.LogCommand, new LogCommand(logging)},
                {(int) CommandEnum.RemoveHandlerCommand, new RemoveHandlerCommand()} 
				// For Now will contain NEW_FILE_COMMAND
            };
        }
        /// <summary>
        /// ExecuteCommand function.
        /// check if the command is valid, and if it is -> execute it by
        /// interpert the command with the dictionary.
        /// </summary>
        /// <param name="commandID">id of command</param>
        /// <param name="args"> args of command</param>
        /// <param name="resultSuccesful">boolean to say if success or fail</param>
        /// <returns></returns>
        public string ExecuteCommand(int commandID, string[] args, out bool resultSuccesful)
        {
            ICommand comm;
            if (commands.TryGetValue(commandID, out comm))
            {
                // command is valid, than execute the command
                return comm.Execute(args, out resultSuccesful);
            }
            resultSuccesful = false;
            return "command not valid";
           
        }
    }
}
