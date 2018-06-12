using System.Runtime.Serialization;

namespace ExpTcpCommon
{
    [DataContract]
    public class TransferFileJobResponse
    {
        [DataMember]
        public ulong Size { get; set; }

        [DataMember]
        public int Status { get; set; }
    }
}
