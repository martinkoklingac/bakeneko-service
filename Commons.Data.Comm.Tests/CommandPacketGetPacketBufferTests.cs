using Commons.Ext;
using NUnit.Framework;
using System;

namespace Commons.Data.Comm
{
    [TestFixture]
    [TestOf(typeof(CommandPacket))]
    class CommandPacketGetPacketBufferTests
    {
        #region TESTS
        [Test]
        public void GetPacketBuffer_CommandWithNullData_Test()
        {
            //Arrange
            const Command.ReqCmd cmd = Command.ReqCmd.Status;
            var command = new CommandPacket(cmd, null);
            var expectedCommandByte = Command.Convert(cmd);
            var expectedDataLengthBuffer = new byte[CommandPacket.DATA_LENGTH_SIZE] { 0xFF, 0xFF, 0xFF, 0xFF };

            //Act
            var resultBuffer = command.GetPacketBuffer();
            Func<byte> actualCommandByte = () => resultBuffer[CommandPacket.COMMAND_OFFSET];
            Func<byte[]> actualdataLengthBuffer = () => resultBuffer.Slice(CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_OFFSET + CommandPacket.DATA_LENGTH_SIZE - 1);

            //Assert
            Assert.That(resultBuffer, Is.Not.Null.And.Length.EqualTo(CommandPacket.PACKET_SIZE));
            Assert.That(actualCommandByte(), Is.EqualTo(expectedCommandByte));
            Assert.That(actualdataLengthBuffer(), Is.EqualTo(expectedDataLengthBuffer));
        }

        [Test]
        public void GetPacketBuffer_CommandWithEmptyData_Test()
        {
            //Arrange
            const Command.ReqCmd cmd = Command.ReqCmd.Status;
            var command = new CommandPacket(cmd, string.Empty);
            var expectedCommandByte = Command.Convert(cmd);
            var expectedDataLengthBuffer = new byte[CommandPacket.DATA_LENGTH_SIZE] { 0x00, 0x00, 0x00, 0x00 };

            //Act
            var resultBuffer = command.GetPacketBuffer();
            Func<byte> actualCommandByte = () => resultBuffer[CommandPacket.COMMAND_OFFSET];
            Func<byte[]> actualDataLengthBuffer = () => resultBuffer.Slice(CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_OFFSET + CommandPacket.DATA_LENGTH_SIZE - 1);

            //Assert
            Assert.That(resultBuffer, Is.Not.Null.And.Length.EqualTo(CommandPacket.PACKET_SIZE));
            Assert.That(actualCommandByte(), Is.EqualTo(expectedCommandByte));
            Assert.That(actualDataLengthBuffer(), Is.EqualTo(expectedDataLengthBuffer));
        }

        [Test]
        public void GetPacketBuffer_CommandWithMinData_Test()
        {
            //Arrange
            const Command.ReqCmd cmd = Command.ReqCmd.Status;
            var command = new CommandPacket(cmd, "X");
            var expectedCommandByte = Command.Convert(cmd);
            var expectedDataLengthBuffer = new byte[CommandPacket.DATA_LENGTH_SIZE] { 0x01, 0x00, 0x00, 0x00 };
            var expectedDataBuffer = new byte[] { (byte)'X' };

            //Act
            var resultBuffer = command.GetPacketBuffer();
            Func<byte> actualCommandByte = () => resultBuffer[CommandPacket.COMMAND_OFFSET];
            Func<byte[]> actualDataLengthBuffer = () => resultBuffer.Slice(CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_OFFSET + CommandPacket.DATA_LENGTH_SIZE - 1);
            Func<byte[]> actualDataBuffer = () => resultBuffer.Slice(CommandPacket.DATA_OFFSET, CommandPacket.DATA_OFFSET); //Data should have length of 1

            //Assert
            Assert.That(resultBuffer, Is.Not.Null.And.Length.EqualTo(CommandPacket.PACKET_SIZE));
            Assert.That(actualCommandByte(), Is.EqualTo(expectedCommandByte));
            Assert.That(actualDataLengthBuffer(), Is.EqualTo(expectedDataLengthBuffer));
            Assert.That(actualDataBuffer(), Is.EqualTo(expectedDataBuffer));
        }

        [Test]
        public void GetPacketBuffer_CommandWithData_Test()
        {
            //Arrange
            const Command.ReqCmd cmd = Command.ReqCmd.Status;
            var command = new CommandPacket(cmd, "XYZ");
            var expectedCommandByte = Command.Convert(cmd);
            var expectedDataLengthBuffer = new byte[CommandPacket.DATA_LENGTH_SIZE] { 0x03, 0x00, 0x00, 0x00 };
            var expectedDataBuffer = new byte[] { (byte)'X', (byte)'Y', (byte)'Z' };

            //Act
            var resultBuffer = command.GetPacketBuffer();
            Func<byte> actualCommandByte = () => resultBuffer[CommandPacket.COMMAND_OFFSET];
            Func<byte[]> actualDataLengthBuffer = () => resultBuffer.Slice(CommandPacket.DATA_LENGTH_OFFSET, CommandPacket.DATA_LENGTH_OFFSET + CommandPacket.DATA_LENGTH_SIZE - 1);
            Func<byte[]> actualDataBuffer = () => resultBuffer.Slice(CommandPacket.DATA_OFFSET, CommandPacket.DATA_OFFSET + 3 - 1); //Data should have length of 3

            //Assert
            Assert.That(resultBuffer, Is.Not.Null.And.Length.EqualTo(CommandPacket.PACKET_SIZE));
            Assert.That(actualCommandByte(), Is.EqualTo(expectedCommandByte));
            Assert.That(actualDataLengthBuffer(), Is.EqualTo(expectedDataLengthBuffer));
            Assert.That(actualDataBuffer(), Is.EqualTo(expectedDataBuffer));
        }
        #endregion
    }
}
