using System;
using System.IO;
using System.Threading.Tasks;
using ExpTcpCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpTcpTests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void TransferRequest_Test()
        {
            var fn = "Boo";
            using (var ms = new MemoryStream())
            {
                SendReceiveHelper.SendTransferRequest(ms, new TransferFileJobRequest
                {
                    FileName = fn,
                    Size = 33,
                });

                ms.Position = 0;

                var req = SendReceiveHelper.GetTransferRequest(ms);

                Assert.AreEqual(req.Size, 33UL);
                Assert.AreEqual(req.FileName, fn);
            }
        }

        [TestMethod]
        public void SendTransferFileResponse_Test()
        {
            using (var ms = new MemoryStream())
            {
                SendReceiveHelper.SendTransferFileJobResponse(ms, new TransferFileJobResponse()
                {
                    Size = 33,
                    Status = 33
                });

                ms.Position = 0;

                var req = SendReceiveHelper.GetTransferFileJobResponse(ms);

                Assert.AreEqual(req.Size, 33U);
                Assert.AreEqual(req.Status, 33);
            }
        }
    }
}
