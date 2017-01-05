using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static Commons.Data.Comm.Command;
using static Commons.Data.Comm.Packet;

namespace Commons.Data.Comm.Commands
{
    [TestFixture]
    [TestOf(typeof(Msg))]
    public class MsgTests
    {
        #region TESTS
        [Test]
        public void Msg_MessageIsCoalesced_Test(
            [Values(null, "")] string message,
            [Values("")] string expectedMessage)
        {
            //Act
            var msg = new Msg(message);

            //Assert
            Assert.AreEqual(expectedMessage, msg.Message);
        }

        [Test]
        public void Msg_ConstructorFromString_Test(
            [Values(null, "", "dummy string")] string message)
        {
            //Act
            var msg = new Msg(message);
            var actualIsValid = msg.IsValid();

            //Assert
            Assert.IsTrue(actualIsValid);
        }

        [Test]
        public void Msg_ConstructorFromStream_Test()
        {
            //Arrange
            var expectedPacCmd = PacCmd.Message;
            var expectedMessage = "dummy message";
            var expectedDataSize = Encoding.ASCII.GetByteCount(expectedMessage);
            var expectedSize = expectedDataSize + HEADER_LENGTH;
            var expectedDataHash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(expectedMessage));

            var buffer = new byte[expectedSize];
            SetDelim(buffer);
            SetPacCmd(buffer, expectedPacCmd);
            SetDataSize(buffer, expectedDataSize);
            SetDataHash(buffer, expectedDataHash);
            Msg.SetMessage(buffer, expectedMessage);

            //Act
            Msg msg;
            using (var ms = new MemoryStream(buffer))
                msg = new Msg(ms);

            var actualIsValid = msg.IsValid();

            //Assert
            Assert.IsTrue(actualIsValid);
            Assert.AreEqual(expectedPacCmd, msg.PacCmd);
            Assert.AreEqual(expectedDataSize, msg.DataSize);
            Assert.AreEqual(expectedSize, msg.Size);
            Assert.AreEqual(expectedMessage, msg.Message);
            Assert.AreEqual(expectedDataHash, msg.DataHash);
        }

        [Test]
        public void Msg_CyclicalConsistency_Test()
        {
            //Arrange
            var expectedMsg = new Msg("dummy text");

            //Act
            var buffer = expectedMsg.GetBytes();
            Msg actualMsg;
            using (var ms = new MemoryStream(buffer))
                actualMsg = new Msg(ms);

            //Assert
            Assert.AreEqual(expectedMsg, actualMsg);
        }

        [Test, Sequential]
        public void Msg_Equality_Test(
            [Values(null, "", null, "", "dummy message", "dummy message 1")] string leftMessage,
            [Values("", null, null, "", "dummy message", "garbled message 1")] string rightMessage,
            [Values(true, true, true, true, true, false)] bool expectedEquality)
        {
            //Arrage
            var leftMsg = new Msg(leftMessage);
            var rightMsg = new Msg(rightMessage);

            //Act
            var actualEquality = leftMsg.Equals(rightMsg);

            //Assert
            Assert.AreEqual(expectedEquality, actualEquality);
        }
        #endregion
    }
}
