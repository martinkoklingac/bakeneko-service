using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Commons.Data.Comm.Command;
using static Commons.Data.Comm.Packet;

namespace Commons.Data.Comm
{
    [TestFixture]
    [TestOf(typeof(Packet))]
    public class PacketTests
    {
        #region TESTS
        [Test]
        public void Packet_Constructor_ArgumentNullException_Test()
        {
            //Assert
            Assert.That(() => new PacketProxy(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Packet_ConstructorFromStream_Test(
            [Values]PacCmd expectedPacCmd, 
            [Values(0, int.MaxValue, 77)]int expectedDataSize)
        {
            //Arrange
            var buffer = new byte[HEADER_LENGTH];
            var expectedSize = expectedDataSize + HEADER_LENGTH;
            var expectedDataHash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes("DUMMY"));

            //Fill the buffer
            SetDelim(buffer);
            SetPacCmd(buffer, expectedPacCmd);
            SetDataSize(buffer, expectedDataSize);
            SetDataHash(buffer, expectedDataHash);

            //Act
            Packet result;
            using (var ms = new MemoryStream(buffer))
                result = new PacketProxy(ms);

            //Assert
            Assert.AreEqual(expectedPacCmd, result.PacCmd);
            Assert.AreEqual(expectedDataSize, result.DataSize);
            Assert.AreEqual(expectedSize, result.Size);
            Assert.AreEqual(expectedDataHash, result.DataHash);
        }

        [Test]
        public void Packet_GetBytes_Test(
            [Values] PacCmd pacCmd)
        {
            //Arrange
            var expectedBuffer = new byte[HEADER_LENGTH];
            SetDelim(expectedBuffer);
            SetPacCmd(expectedBuffer, pacCmd);
            SetDataSize(expectedBuffer, 0);
            SetDataHash(expectedBuffer, new byte[DATA_HASH_LENGTH]);

            //Act
            Packet result;
            using (var ms = new MemoryStream(expectedBuffer))
                result = new PacketProxy(ms);

            var actualBuffer = result.GetBytes();

            //Assert
            Assert.AreEqual(expectedBuffer, actualBuffer);
        }

        [Test, Sequential]
        public void Packet_IsValid_InvalidDelimiter_Test(
            [Values(byte.MaxValue, DELIM_BYTE)] byte delim,
            [Values(false, true)] bool expectedIsValid)
        {
            //Arrange
            var buffer = new byte[HEADER_LENGTH];
            buffer[DELIM_OFFSET] = delim;
            SetPacCmd(buffer, PacCmd.Ack);

            //Act
            Packet result;
            using (var ms = new MemoryStream(buffer))
                result = new PacketProxy(ms);

            var actualIsValid = result.IsValid();

            //Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }

        [Test, Sequential]
        public void Packet_IsValid_InvalidPacCmd_Test(
            [Values((PacCmd)byte.MaxValue, PacCmd.Ack)] PacCmd pacCmd,
            [Values(false, true)] bool expectedIsValid)
        {
            //Arrange
            var buffer = new byte[HEADER_LENGTH];
            SetDelim(buffer);
            SetPacCmd(buffer, pacCmd);

            //Act
            Packet result;
            using (var ms = new MemoryStream(buffer))
                result = new PacketProxy(ms);

            var actualIsValid = result.IsValid();

            //Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }

        [Test]
        public void Packet_IsValid_ZeroDataSize_Test()
        {
            //Arrange
            var buffer = new byte[HEADER_LENGTH];

            SetDelim(buffer);
            SetPacCmd(buffer, PacCmd.Ack);
            SetDataSize(buffer, 0);
            SetDataHash(buffer, new byte[DATA_HASH_LENGTH]);

            //Act
            Packet result;
            using (var ms = new MemoryStream(buffer))
                result = new PacketProxy(ms);

            var actualIsValid = result.IsValid();

            //Assert
            Assert.IsTrue(actualIsValid);
        }

        [Test]
        public void Packet_IsValid_ValidDataSize_Test(
            [Values(1, 10, 100)] int dataSize)
        {
            //Arrange
            var data = Enumerable.Range(0, dataSize).Select(x => (byte)x).ToArray();
            var buffer = new byte[HEADER_LENGTH + dataSize];

            Array.Copy(data, 0, buffer, DATA_OFFSET, dataSize);

            SetDelim(buffer);
            SetPacCmd(buffer, PacCmd.Ack);
            SetDataSize(buffer, dataSize);
            SetDataHash(buffer, GetHash(data));

            //Act
            Packet result;
            using (var ms = new MemoryStream(buffer))
                result = new PacketProxy(ms);

            var actualIsValid = result.IsValid();

            //Assert
            Assert.IsTrue(actualIsValid);
        }

        [Test]
        public void Packet_IsValid_InvalidDataSize_Test(
            [Values(int.MinValue, -1, int.MaxValue)] int dataSize)
        {
            //Arrange
            var buffer = new byte[HEADER_LENGTH];
            SetDelim(buffer);
            SetPacCmd(buffer, PacCmd.Ack);
            SetDataSize(buffer, dataSize);
            
            //Act
            Packet result;
            using (var ms = new MemoryStream(buffer))
                result = new PacketProxy(ms);

            var actualIsValid = result.IsValid();

            //Assert
            Assert.IsFalse(actualIsValid);
        }
        #endregion

        #region INTERNAL TYPES
        public class PacketProxy : Packet
        {
            public PacketProxy(Stream stream) : 
                base(stream)
            { }
        }

        #endregion
    }
}
