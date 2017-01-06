using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace Commons.Data.Comm
{
    [TestFixture]
    [TestOf(typeof(CommandPacket))]
    class CommandPacketReadTests
    {
        #region TESTS
        [Test]
        public void Read_StreamIsNUll_ReturnsNull_Test()
        {
            //Arrange
            Stream stream = null;

            //Act
            var result = CommandPacket.Read(stream);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Read_EmptyDataStream_ReturnsNull_Test()
        {
            //Arrange
            var buffer = new byte[0];

            //Act
            CommandPacket result = null;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Read_MalformedDataStream_ReturnsNull_Test()
        {
            //Arrange
            var buffer = new byte[] { 0, 0, 0, 0, 0 };

            //Act
            CommandPacket result = null;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Read_MalformedStartDelimiters_ReturnsNull_Test(
            [Random(0, CommandPacket.DELIMITER_COUNT - 1, 1)]int startDelimiterOffset)
        {
            //Arrange
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();
            
            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Act
            //Break one delimiter start byte
            buffer[startDelimiterOffset] = 0;

            CommandPacket result = null;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Read_MalformedEndDelimiters_ReturnsNull_Test(
            [Random(CommandPacket.PACKET_SIZE - CommandPacket.DELIMITER_COUNT, CommandPacket.PACKET_SIZE - 1, 1)]int endDelimiterOffset)
        {
            //Arrange
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();

            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Act
            //Break one delimiter start byte
            buffer[endDelimiterOffset] = 0;

            CommandPacket result = null;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Read_PacketBuffer_CommandWithNullData_Test()
        {
            //Arrange
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();

            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Set up data length bytes
            var dataLengthBuffer = BitConverter.GetBytes(-1);
            Array.Copy(dataLengthBuffer, 0, buffer, CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_SIZE);

            //Act
            CommandPacket result;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Data);
            Assert.AreEqual(command, result.Cmd);
        }

        [Test]
        public void Read_PacketBuffer_CommandWithEmptyData_Test()
        {
            //Arrange
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();

            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Set up data length bytes
            var dataLengthBuffer = BitConverter.GetBytes(0);
            Array.Copy(dataLengthBuffer, 0, buffer, CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_SIZE);

            //Act
            CommandPacket result;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Data);
            Assert.AreEqual(command, result.Cmd);
        }

        [Test]
        public void Read_PacketBuffer_CommandWithMinimalData_Test()
        {
            //Arrange
            const string data = "X";
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();

            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Set up data length bytes
            var dataLengthBuffer = BitConverter.GetBytes(1);    //Smallest data should be at most 1 byte
            Array.Copy(dataLengthBuffer, 0, buffer, CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_SIZE);

            //Set up data bytes
            var dataBuffer = Encoding.ASCII.GetBytes(data);
            Array.Copy(dataBuffer, 0, buffer, CommandPacket.DATA_OFFSET, dataBuffer.Length);

            //Act
            CommandPacket result;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(data, result.Data);
            Assert.AreEqual(command, result.Cmd);
        }

        [Test]
        public void Read_PacketBuffer_CommandWithMaximalData_Test()
        {
            //Arrange
            const string data = ".a_b5D&X|";    //this should be the same length as CommandPacket.DATA_SIZE
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();

            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Set up data length bytes
            var dataLengthBuffer = BitConverter.GetBytes(CommandPacket.DATA_SIZE);
            Array.Copy(dataLengthBuffer, 0, buffer, CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_SIZE);

            //Set up data bytes
            var dataBuffer = Encoding.ASCII.GetBytes(data);
            Array.Copy(dataBuffer, 0, buffer, CommandPacket.DATA_OFFSET, CommandPacket.DATA_SIZE);

            //Act
            CommandPacket result;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(data, result.Data);
            Assert.AreEqual(command, result.Cmd);
        }

        [Test]
        public void Read_PacketBuffer_CommandWithOverflowData_Test()
        {
            //Arrange
            const string data = ".a_b5D&X|z";    //this should be the same length as CommandPacket.DATA_SIZE + 1
            var truncatedData = data.Substring(0, CommandPacket.DATA_SIZE); //Truncated data should be exactly CommandPacket.DATA_SIZE in length
            const Command.ReqCmd command = Command.ReqCmd.DumpStats;
            var buffer = CommandPacket.GetEmptyPacketBuffer();

            //Set up command byte
            buffer[CommandPacket.COMMAND_OFFSET] = Command.Convert(command);

            //Set up data length bytes
            var dataLengthBuffer = BitConverter.GetBytes(CommandPacket.DATA_SIZE + 1);
            Array.Copy(dataLengthBuffer, 0, buffer, CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_SIZE);

            //Set up data bytes
            var dataBuffer = Encoding.ASCII.GetBytes(data);
            Array.Copy(dataBuffer, 0, buffer, CommandPacket.DATA_OFFSET, CommandPacket.DATA_SIZE);

            //Act
            CommandPacket result;
            using (var ms = new MemoryStream(buffer))
                result = CommandPacket.Read(ms);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(truncatedData, result.Data);
            Assert.AreEqual(command, result.Cmd);
        }
        #endregion
    }
}
