using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ExpTcpCommon
{
    public static class SendReceiveHelper
    {
        private const int LengthMessageSize = 8;
        public static void SendTransferRequest(Stream stream, TransferFileJobRequest request)
        {
            using (var ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(TransferFileJobRequest));
                ser.WriteObject(ms, request);
                var msgArray = ms.ToArray();

                var lenBytes = BitConverter.GetBytes((long)msgArray.Length);
                stream.Write(lenBytes, 0, lenBytes.Length);
                stream.Write(msgArray, 0, msgArray.Length);
            }
        }

        public static TransferFileJobResponse GetTransferFileJobResponse(Stream stream)
        {
            int stat = 0;
            TransferFileJobResponse response;

            var respLenghtBytes = new byte[LengthMessageSize];
            stat = stream.Read(respLenghtBytes, 0, respLenghtBytes.Length);
            if (stat <= 0)
            {
                throw new InvalidOperationException($"Read error: {stat}");
            }

            var responseLength = BitConverter.ToInt64(respLenghtBytes, 0);

            var responseBytes = new byte[responseLength];
            int readThisTime = 0;
            for (var rsf = 0; rsf < responseLength; rsf += readThisTime)
            {
                int st = stream.Read(responseBytes, rsf, (int)(responseLength - rsf));
                readThisTime += (st > 0) ? st : 0;
            }

            using (var ms = new MemoryStream(responseBytes))
            {
                var responseSerializer = new DataContractJsonSerializer(typeof(TransferFileJobResponse));
                response = responseSerializer.ReadObject(ms) as TransferFileJobResponse;
            }

            return response;
        }

        public static void SendTransferFileJobResponse(Stream stream, TransferFileJobResponse response)
        {
            using (var ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(TransferFileJobResponse));
                ser.WriteObject(ms, response);
                var msgArray = ms.ToArray();

                var lenBytes = BitConverter.GetBytes((long)msgArray.Length);
                stream.Write(lenBytes, 0, lenBytes.Length);
                stream.Write(msgArray, 0, msgArray.Length);
            }
        }

        public static TransferFileJobRequest GetTransferRequest(Stream stream)
        {
            TransferFileJobRequest result = null;
            var lenBytes = new byte[LengthMessageSize];
            int st = stream.Read(lenBytes, 0, lenBytes.Length);
            if (st < 0)
            {
                throw new InvalidOperationException("Read fail");
            }

            var responseLength = BitConverter.ToInt64(lenBytes, 0);

            var responseBytes = new byte[responseLength];
            int readThisTime = 0;
            for (var rsf = 0; rsf < responseLength; rsf += readThisTime)
            {
                st = stream.Read(responseBytes, rsf, (int)(responseLength - rsf));
                if (st > 0)
                {
                    readThisTime += st;
                }
            }
            using (var ms = new MemoryStream(responseBytes))
            {
                var responseSerializer = new DataContractJsonSerializer(typeof(TransferFileJobRequest));
                result = responseSerializer.ReadObject(ms) as TransferFileJobRequest;
            }

            return result;
        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output, long maxBytes)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while (maxBytes > 0 && (len = input.Read(buffer, 0, (int)Math.Min(buffer.Length, maxBytes))) > 0)
            {
                output.Write(buffer, 0, len);
                maxBytes -= len;
            }
        }
    }
}
