using Moq;
using NUnit.Framework;
using System;

namespace Commons.Data.Comm
{
    [TestFixture]
    [TestOf(typeof(CommandPacket))]
    class CommandPacketTests
    {
        #region TESTS
        [Test]
        public void CmdProperty_IsSet_Test()
        {
            //Arrange
            const Command.Cmd expectedCommand = Command.Cmd.DumpStats;
            var commandPacket = new CommandPacket(expectedCommand, It.IsAny<string>());

            //Act
            var actualCommand = commandPacket.Cmd;

            //Assert
            Assert.AreEqual(expectedCommand, actualCommand);
        }

        [Test]
        public void Constructor_DataComponentsExceedsMaxLength_Test()
        {
            //Arrange
            const string data = "data_is_too_long";

            //Assert
            Assert.That(() => new CommandPacket(It.IsAny<Command.Cmd>(), data), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void DataProperty_IsSet_Test()
        {
            //Arrange
            const string expectedData = "Data";
            var commandPacket = new CommandPacket(It.IsAny<Command.Cmd>(), expectedData);

            //Act
            var actualData = commandPacket.Data;

            //Assert
            Assert.AreEqual(expectedData, actualData);
        }

        [Test]
        public void DataLength_IsSet_GeneratedFromNullData_Test()
        {
            //Arrange
            const string data = null;
            const int expectedDataLength = -1;
            var commandPacket = new CommandPacket(It.IsAny<Command.Cmd>(), data);

            //Act
            var actualDataLength = commandPacket.DataLength;

            //Assert
            Assert.AreEqual(expectedDataLength, actualDataLength);
        }

        [Test]
        public void DataLength_IsSet_GeneratedFromEmptyData_Test()
        {
            //Arrange
            const string data = "";
            const int expectedDataLength = 0;
            var commandPacket = new CommandPacket(It.IsAny<Command.Cmd>(), data);

            //Act
            var actualDataLength = commandPacket.DataLength;

            //Assert
            Assert.AreEqual(expectedDataLength, actualDataLength);
        }

        [Test]
        public void DataLength_IsSet_GeneratedFromnonTrivialData_Test()
        {
            //Arrange
            const string data = "Data"; //data.Length = 4
            const int expectedDataLength = 4;
            var commandPacket = new CommandPacket(It.IsAny<Command.Cmd>(), data);

            //Act
            var actualDataLength = commandPacket.DataLength;

            //Assert
            Assert.AreEqual(expectedDataLength, actualDataLength);
        }
        #endregion
    }
}
