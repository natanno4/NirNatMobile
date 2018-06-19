using ImageService.Infrastructure.Enums;
using ImageService.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace ImageService.Commands
{
    public class LogCommand : ICommand
    {
        private ILoggingService logService;
        /// <summary>
        /// create log command
        /// </summary>
        public LogCommand(ILoggingService lg)
        {
            logService = lg;
        }

        /// <summary>
        /// returns the list of the current logs of the ImageService as a command converted to string.
        /// </summary>
        public string Execute(string[] args, out bool result)
        {
            ObservableCollection<string> obs = logService.listOfLogs;
            //convert list to string
            string jsonFormat = JsonConvert.SerializeObject(obs);
            string[] arg = { jsonFormat };
            MsgCommand msg = new MsgCommand((int)CommandEnum.LogCommand, arg);
            //convert command to string
            string newCommand = JsonConvert.SerializeObject(msg);
            result = true;
            return newCommand;
        }
    }
}
