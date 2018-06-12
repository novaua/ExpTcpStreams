using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExpTcpCommon
{
    [DataContract]
    public class TransferFileJobRequest
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public ulong Size { get; set; }

        [DataMember]
        public bool DownloadFromServer { get; set; }

        [DataMember]
        public bool IsDone { get; set; }

    }
}
