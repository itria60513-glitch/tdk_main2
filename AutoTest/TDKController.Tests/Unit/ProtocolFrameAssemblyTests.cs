using System.Text;
using Communication.Protocol;
using NUnit.Framework;

namespace TDKController.Tests.Unit
{
    [TestFixture]
    public class ProtocolFrameAssemblyTests
    {
        [Test]
        public void OmronProtocol_Pop_WhenCrArrivesInFinalFragment_ReturnsFullFrame()
        {
            var protocol = new OmronProtocol();
            byte[] buffer = new byte[protocol.BufferSize];

            Assert.AreEqual(8, protocol.Push(Encoding.ASCII.GetBytes("00wvffUQ"), 8));
            Assert.AreEqual(0, protocol.Pop(ref buffer));

            Assert.AreEqual(8, protocol.Push(Encoding.ASCII.GetBytes("#EB12345"), 8));
            Assert.AreEqual(0, protocol.Pop(ref buffer));

            Assert.AreEqual(3, protocol.Push(Encoding.ASCII.GetBytes("68\r"), 3));
            int length = protocol.Pop(ref buffer);
            var verifyResult = protocol.VerifyInFrameStructure(buffer, length);

            Assert.AreEqual(19, length);
            Assert.IsTrue(verifyResult.Item1);
            Assert.AreEqual("00wvffUQ#EB1234568", Encoding.ASCII.GetString(verifyResult.Item2));
        }

        [Test]
        public void HermosProtocol_Pop_WhenCrArrivesInFinalFragment_ReturnsFullFrame()
        {
            var protocol = new HermosProtocol();
            byte[] buffer = new byte[protocol.BufferSize];

            Assert.AreEqual(4, protocol.Push(Encoding.ASCII.GetBytes("ABCD"), 4));
            Assert.AreEqual(0, protocol.Pop(ref buffer));

            Assert.AreEqual(4, protocol.Push(Encoding.ASCII.GetBytes("1234"), 4));
            Assert.AreEqual(0, protocol.Pop(ref buffer));

            Assert.AreEqual(1, protocol.Push(Encoding.ASCII.GetBytes("\r"), 1));
            int length = protocol.Pop(ref buffer);

            Assert.AreEqual(9, length);
            Assert.AreEqual("ABCD1234\r", Encoding.ASCII.GetString(buffer, 0, length));
        }

        [Test]
        public void OmronProtocol_Purge_ResetsSearchCursor()
        {
            var protocol = new OmronProtocol();
            byte[] buffer = new byte[protocol.BufferSize];

            Assert.AreEqual(8, protocol.Push(Encoding.ASCII.GetBytes("PARTIAL1"), 8));
            Assert.AreEqual(0, protocol.Pop(ref buffer));

            protocol.Purge();

            Assert.AreEqual(3, protocol.Push(Encoding.ASCII.GetBytes("OK\r"), 3));
            int length = protocol.Pop(ref buffer);
            var verifyResult = protocol.VerifyInFrameStructure(buffer, length);

            Assert.AreEqual(3, length);
            Assert.IsTrue(verifyResult.Item1);
            Assert.AreEqual("OK", Encoding.ASCII.GetString(verifyResult.Item2));
        }
    }
}
