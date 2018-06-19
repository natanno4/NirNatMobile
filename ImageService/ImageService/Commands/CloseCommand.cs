using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    class CloseCommand : ICommand
    {
        public string Execute(string[] args, out bool result)
        {
            // call the closeHander command / raise event
            result = true;
            return "yes";
        }
    }
}
