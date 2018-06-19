using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Infrastructure.Enums;
using Infrastructure;

namespace ImageService.Commands
{
    public class GetConfigCommand : ICommand
    {
        /// <summary>
        /// get the App Config information and returns it as a command format converted to string. 
        /// </summary>
        public string Execute(string[] args, out bool result)
        {
            string[] strConfig = new string[5];
            strConfig[0] = ConfigurationManager.AppSettings.Get("OutputDir");
            strConfig[1] = ConfigurationManager.AppSettings.Get("SourceName");
            strConfig[2] = ConfigurationManager.AppSettings.Get("LogName");
            strConfig[3] = ConfigurationManager.AppSettings.Get("ThumbnailSize");
            strConfig[4] = ConfigurationManager.AppSettings.Get("Handler");

            MsgCommand cmnd = new MsgCommand((int)CommandEnum.GetConfigCommand, strConfig);
            //conver command to string.
            string jsonFormat = JsonConvert.SerializeObject(cmnd);
            result = true;
            return jsonFormat;
        }
    }
}
