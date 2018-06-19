using ImageService.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageService.ServiceCommunication
{
    public class ImageTransferHandler
    {

        private List<TcpClient> clientList;
        public static Mutex wMutex;
        private ILoggingService logging;
        string handler;

        public ImageTransferHandler(List<TcpClient> c, ILoggingService logs, string h)
        {
            logging = logs;
            clientList = c;
            handler = h;
        }

        public void HandleClient(TcpClient client)
        {
            new Task(() =>
            {
                while (client.Connected)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        //get pic name
                        Byte[] name = new byte[6790];
                        int nameLength = stream.Read(name, 0, name.Length);
                        string picName = Encoding.ASCII.GetString(name, 0, nameLength);

                        //send confirmation that the server got the image name.
                        byte[] confirm = { 1 };
                        stream.Write(confirm, 0, 1);
                        List<byte> ImageByte = new List<byte>();
                        byte[] tempBytes = new byte[6790];
                        do
                        {
                            int length = stream.Read(tempBytes, 0, tempBytes.Length);
                            foreach(byte b in tempBytes)
                            {
                                ImageByte.Add(b);
                            }
                            Thread.Sleep(300);

                        } while (stream.DataAvailable);
                        File.WriteAllBytes(this.handler + @"\" + name, ImageByte.ToArray());
                    }
                    catch (Exception e)
                    {
                        //close client
                        Console.WriteLine(e.ToString());
                        logging.Log("error in client", Logging.Modal.MessageTypeEnum.FAIL);
                        if (clientList.Contains(client))
                        {
                            clientList.Remove(client);
                        }

                        client.Close();
                    }

                }
            }).Start();
        }
    }
    }
}
