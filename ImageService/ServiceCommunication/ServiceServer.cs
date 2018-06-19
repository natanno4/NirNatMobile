using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using ImageService.Controller;
using ImageService.Logging;
using ImageService.Logging.Modal;
using Infrastructure;
using Communication;

namespace ImageService.ServiceCommunication
{
    public class ServiceServer : IServiceServer
    {
        private int port;
        private string IP;
        private TcpListener listener;
        private IClientHandler handler;
        private List<TcpClient> clients;
        private ILoggingService logging;

        /// <summary>
        /// create a service server.
        /// </summary>
        public ServiceServer(IImageController controller, ILoggingService logService)
        {
            //get connection information
            CommunicationInfo com = new CommunicationInfo(); 
            this.logging = logService;
            this.port = com.port;
            this.IP = com.IPNumber;
            clients = new List<TcpClient>();
            this.handler = new ClientHandler(clients, controller, logService);
        }

        /// <summary>
        /// start the server.listens to clients and handle them.
        /// </summary>
        public void Start()
        {
            try
            {
                //event for loggs adding
                this.logging.LogAdded += this.onNotifyClients;
                IPEndPoint pnt = new IPEndPoint(IPAddress.Parse(this.IP), this.port);
                listener = new TcpListener(pnt);
                listener.Start();
                // print of connections
                Task tsk = new Task(() =>
                {
                    while (true)
                    {
                        try
                        {
                            TcpClient client = listener.AcceptTcpClient();
                            // success in recieve
                            clients.Add(client);
                            handler.HandleClient(client);
                        }
                        catch (Exception e)
                        {  
                            //end server.
                            Console.WriteLine(e.ToString());
                            this.Stop();
                            break;
                        }

                    } 
                });
                tsk.Start();
            } catch(Exception t)
            {
                Console.WriteLine(t.ToString());
            }
        }

        /// <summary>
        /// stop listeninig
        /// </summary>
        public void Stop()
        {
            listener.Stop();
        }

        /// <summary>
        /// send the message to all the clients in the list.
        /// </summary>
        public void onNotifyClients(object sender, MsgCommand msg)
        {
            new Task(() =>
            {
                foreach (TcpClient C in clients)
                {
                    try
                    {
                        this.handler.notifyClient(C, msg);
                    } catch (Exception e)
                    {
                        //close client
                        Console.WriteLine(e.ToString());
                        C.Close();
                        if (this.clients.Contains(C))
                        {
                            this.clients.Remove(C);
                        }
                        
                    }
                }
            }).Start();

        }

    }
}
