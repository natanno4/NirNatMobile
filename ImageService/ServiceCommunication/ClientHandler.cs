using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using ImageService.Commands;
using Newtonsoft.Json;
using ImageService.Infrastructure.Enums;
using ImageService.Controller;
using System.Threading;
using ImageService.Logging;
using ImageService.Logging.Modal;
using Infrastructure;


namespace ImageService.ServiceCommunication
{
    public class ClientHandler : IClientHandler
    {
        private List<TcpClient> clientList;
        private IImageController controller;
        public static Mutex wMutex;
        private ILoggingService logging;

        /// <summary>
        /// create client handler
        /// </summary>
        public ClientHandler(List<TcpClient> c, IImageController ic, ILoggingService logs)
        {
            this.clientList = c;
            this.controller = ic;
            wMutex = new Mutex();
            this.logging = logs;
           
        }
        /// <summary>
        /// handle client until the client disconnect.
        /// </summary>
        public void HandleClient(TcpClient client)
        {

            new Task(() =>
            {
                while (client.Connected)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        BinaryWriter writer = new BinaryWriter(stream);
                        BinaryReader reader = new BinaryReader(stream);
                        //get command from client
                        string recived = reader.ReadString();
                        MsgCommand msg = JsonConvert.DeserializeObject<MsgCommand>(recived);
                         bool res;
                        //execute the command and get the result.
                         string result = this.controller.ExecuteCommand((int)msg.commandID, msg.args, out res);
                         wMutex.WaitOne();
                        //write back the result to client
                         writer.Write(result);
                         wMutex.ReleaseMutex();
                        
                    } catch (Exception e)
                    {
                        //close client
                       Console.WriteLine(e.ToString());
                        if (clientList.Contains(client)) {
                            clientList.Remove(client);
                        }
                         
                       client.Close();
                    }
                      
                }
            }).Start();
        }

        /// <summary>
        /// send the messgae command msg to the given client.
        /// </summary>
        public void notifyClient(TcpClient client, MsgCommand msg)
        {
            NetworkStream stream = client.GetStream();
            BinaryWriter writer = new BinaryWriter(stream);
            string writeCommand = msg.ToJSON();
            wMutex.WaitOne();
            writer.Write(writeCommand);
            wMutex.ReleaseMutex();

        }
    }
}
