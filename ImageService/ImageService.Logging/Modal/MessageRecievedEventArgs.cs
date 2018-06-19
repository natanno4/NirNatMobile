using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Logging.Modal
{
    public class MessageRecievedEventArgs : EventArgs
    {
        public MessageTypeEnum Status { get; set; }
        public string Message { get; set; }

        public MessageRecievedEventArgs(MessageTypeEnum s, string m)
        {
            this.Status = s;
            this.Message = m;
        }

        public MessageRecievedEventArgs() { }

        public static MessageRecievedEventArgs FromJSON(string str)
        {
            JObject jObject = JObject.Parse(str);
            int type = (int)jObject["Type"];
            string arr = (string)jObject["Message"];
            MessageRecievedEventArgs msg = new MessageRecievedEventArgs();
            msg.Message = arr;
            msg.Status = (MessageTypeEnum)type;
            return msg;
        }
        public string ToJSON()
        {
            JObject jObject = new JObject();
            jObject["Type"] = (int)this.Status;
            jObject["Message"] = (string)this.Message;
            return jObject.ToString();
        }
    }
}
