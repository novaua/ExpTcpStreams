using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ExpTcpCommon;

namespace ExpTcpServer
{
    class ServerProgram
    {
        const int PORT = 5006;
        static TcpListener listener;

        protected static void cancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nThe read operation has been interrupted.");
            listener.Stop();
            listener = null;
        }

        static void ReceiveFile(Stream stream, string fileName, long length)
        {
            Console.WriteLine("Writing file {0}", fileName);
            using (var file = File.OpenWrite(fileName))
            {
                SendReceiveHelper.CopyStream(stream, file, length);
            }
        }

        static void SendFile(Stream stream, FileInfo fileInfo)
        {
            using (var file = File.OpenRead(fileInfo.FullName))
            {
                file.CopyTo(stream);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += cancelHandler;

                listener = new TcpListener(IPAddress.Parse("0.0.0.0"), PORT);
                listener.Start();
               
                Console.WriteLine($"Waiting for connection at :{PORT}");
                Console.Write("Press any key, or 'C' to quit");
                while (true)
                {
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        Console.WriteLine("connected");
                        var request = SendReceiveHelper.GetTransferRequest(stream);

                        Console.WriteLine("Got file name {0} for {1}", request.FileName, request.DownloadFromServer?"download":"upload");
                        var isReceive = !request.DownloadFromServer;
                        if (isReceive)
                        {
                            SendReceiveHelper.SendTransferFileJobResponse(stream, new TransferFileJobResponse()
                            {
                                Size = request.Size,
                                Status = 0,
                            });

                            using (new AutoStopwatch((long)request.Size))
                            {
                                ReceiveFile(stream, request.FileName, (long)request.Size);
                            }   
                        }
                        else
                        {
                            var fi = new FileInfo(request.FileName);
                            if (fi.Exists)
                            {
                                SendReceiveHelper.SendTransferFileJobResponse(stream, new TransferFileJobResponse()
                                {
                                    Size = (ulong) fi.Length,
                                    Status = 0,
                                });

                                using (new AutoStopwatch(fi.Length))
                                {
                                    SendFile(stream, fi);
                                }
                            }
                            else
                            {
                                SendReceiveHelper.SendTransferFileJobResponse(stream, new TransferFileJobResponse()
                                {
                                    Size = 0,
                                    Status = 1,
                                });
                            }
                        }
                      
                        Console.WriteLine("Done {0}", request.IsDone ? "stopping server." : "");
                        if (request.IsDone)
                            break;
                    }
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("Interrupted {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught {0}", ex.Message);
            }
            finally
            {
                listener?.Stop();
            }
        }
    }
}
