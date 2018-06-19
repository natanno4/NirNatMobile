using ImageService.Infrastructure;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class NewFileCommand : ICommand
    {
        private IImageServiceModal m_modal;

        /// <summary>
        /// create new file Command
        /// </summary>
        public NewFileCommand(IImageServiceModal modal)
        {
            m_modal = modal;            // Storing the Modal
        }
        /// <summary>
        /// Execute function
        /// </summary>
        /// <param name="args">args of newFile command</param>
        /// <param name="result">bool if success or not</param>
        /// <returns></returns>
        public string Execute(string[] args, out bool result)
        {
            return m_modal.AddFile(args[0], out result);
			// The String Will Return the New Path if result = true, and will return the error message
        }
    }
}
