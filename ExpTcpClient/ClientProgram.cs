using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using ExpTcpCommon;

namespace ExpTcpClient
{
    class ClientProgram
    {
        const int PORT = 5006;
        const string ADDRESS = "127.0.0.1";

        static void UploadFile(Stream stream, FileInfo fileInfo, bool closeServer)
        {
            Console.WriteLine("About to transfer file '{0}' of size {1} ", fileInfo.FullName, fileInfo.Length);
           
            var request = new TransferFileJobRequest
            {
                FileName = fileInfo.Name,
                Size = (ulong)fileInfo.Length,
                IsDone = closeServer,
                DownloadFromServer = false,
            };

            SendReceiveHelper.SendTransferRequest(stream, request);
            var response = SendReceiveHelper.GetTransferFileJobResponse(stream);

            if (response.Status != 0)
            {
                throw new InvalidOperationException($"Error from server: {response.Status}");
            }
        
            Console.WriteLine("Transferring file");

            using (var file = File.OpenRead(fileInfo.FullName))
            {
                file.CopyTo(stream);
            }

            Console.WriteLine("File '{0}' sent!", fileInfo.Name);
        }

        static long DownloadFile(Stream stream, string fileName, bool closeServer, Action<long> onStarted)
        {
            Console.WriteLine("About to download file '{0}'", fileName);
            var request = new TransferFileJobRequest
            {
                FileName = fileName,
                IsDone = closeServer,
                DownloadFromServer = true,
            };

            SendReceiveHelper.SendTransferRequest(stream, request);
            var response = SendReceiveHelper.GetTransferFileJobResponse(stream);

            if (response.Status != 0)
            {
                throw new InvalidOperationException($"Error from server: {response.Status}");
            }

            var length = (long)response.Size;

            onStarted?.Invoke(length);

            var fi = new FileInfo(fileName);
            Console.WriteLine($"Downloading file {fi.Name} of size {length}");

            using (var file = File.OpenWrite(fi.Name))
            {
                stream.CopyTo(file);
            }

            Console.WriteLine("File '{0}' received", fileName);

            return length;
        }

        static void Main(string[] args)
        {
            bool closeServer = args.Contains("done");
            bool download = args.Contains("download");

            if (!args.Any() || !(File.Exists(args[0]) || download) )
            {
                Console.WriteLine("Existing file name expected!");
                return;
            }

            string fileName = args[0];
            TcpClient client = null;

            var watch = new AutoStopwatch();
            try
            {
                string server = ConfigurationManager.AppSettings["Tcp.Server"]?? ADDRESS;
                var port = int.Parse(ConfigurationManager.AppSettings["Tcp.Port"]?? PORT.ToString());
                
                client = new TcpClient(server, port);
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("Connected to {0}:{1}", server, port);
                    if (download)
                    {
                        DownloadFile(stream, fileName, closeServer, len => watch.Restart(len));
                    }
                    else
                    {
                        var fi = new FileInfo(fileName);
                        watch = new AutoStopwatch(fi.Length);
                        UploadFile(stream, fi, closeServer);
                    }
                }

                watch.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                client?.Close();
            }
        }
    }
}
