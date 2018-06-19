
using ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Commands;
using ImageService.Infrastructure.Enums;
using Infrastructure;
using Newtonsoft.Json;

namespace ImageService.Logging
{
    public class LoggingService : ILoggingService
    {
        private ObservableCollection<string> listOfLogs;

        ObservableCollection<string> ILoggingService.listOfLogs
        {
            get
            {
                return this.listOfLogs;
            }

            set
            {
                this.listOfLogs = value;

            }
        }

        public LoggingService()
        {
            this.listOfLogs = new ObservableCollection<string>();
        } 
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        public event EventHandler<MsgCommand> LogAdded;

        public void Log(string message, MessageTypeEnum type)
        {
            MessageRecievedEventArgs args = new MessageRecievedEventArgs();
            this.listOfLogs.Add(message + ";" + type.ToString());
            args.Message = message;
            args.Status = type;
            string[] arr = { args.ToJSON() };
            MessageRecieved?.Invoke(this, args);
            LogAdded?.Invoke(this, new MsgCommand((int)CommandEnum.AddLogCommand, arr));
        }
    }
}
