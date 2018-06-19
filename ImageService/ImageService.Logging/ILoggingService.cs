using Infrastructure;
using ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Logging
{
    public interface ILoggingService
    {
        event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        void Log(string message, MessageTypeEnum type);           // Logging the Message
        event EventHandler<MsgCommand> LogAdded;
        ObservableCollection<string> listOfLogs { get; set; }
    }
}
