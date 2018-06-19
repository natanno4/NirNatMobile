using ImageService.Commands;
using ImageService.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace ImageService.Commands
{
    class RemoveHandlerCommand : ICommand
    {

        /// <summary>
        /// returns the handler the the has been deleted as a command converted to string.
        /// </summary>
        public string Execute(string[] args, out bool result)
        {
            bool flag = false;
            string save = null;
            string temp = ConfigurationManager.AppSettings["Handler"];
            //get handlers
            string []handlers = temp.Split(';');
            List<string> copy = new List<string>(handlers);
            //delete the wanted one.
            foreach (string h in handlers)
            {
                if (h.Equals(args[0]))
                {
                    copy.Remove(h);
                    flag = true;
                    save = h;
                    break;
                }
            }
            if (flag)
            {
                //delete succedded
                string[] removeHandler = {save}; 
                MsgCommand msg = new MsgCommand((int)CommandEnum.RemoveHandlerCommand, removeHandler);
                //chage AppConfig.
                createNewHandler(copy); 
                result = true;
                return msg.ToJSON();
            } 
            else
            {
                //fail
                result = false;
                return null;
            }
            
        }
        /// <summary>
        /// change the handelers in the AppConfig.
        /// </summary>
        private void createNewHandler(List<string> handlers)
        {
            string handler = null;
            bool first = true;
            //create handlers string seperated by ;.
            foreach (string h in handlers)
            {
                if (first)
                {
                    handler = h;
                    first = false;
                    continue;
                }
                handler = handler + ";" + h;
            }
            //chnage AppConfig.
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("Handler");
            config.AppSettings.Settings.Add("Handler", handler);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}
