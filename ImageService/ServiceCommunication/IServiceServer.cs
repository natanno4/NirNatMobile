using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ServiceCommunication
{
    public interface IServiceServer
    {
        void Start();
        void Stop();
    }
}
