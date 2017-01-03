using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Commons.Data.Comm
{
    public class CommandPacket
    {
        #region PUBLIC FIELDS
        public const int PACKET_SIZE = 20;
        public const int PACKET_BODY_OFFSET = 3;
        public const int PACKET_BODY_SIZE = 14;
        public const int COMMAND_OFFSET = 3;
        public const int COMMAND_SIZE = 1;
        public const int DATA_LENGTH_OFFSET = 4;
        public const int DATA_LENGTH_SIZE = 4;
        public const int DATA_OFFSET = 8;
        public const int DATA_SIZE = 9;

        public const byte DELIMITER_START_BYTE = (byte)'<';
        public const byte DELIMITER_END_BYTE = (byte)'>';
        public const int DELIMITER_COUNT = 3;
        #endregion

        #region CONSTRUCTORS
        public CommandPacket(Command.Cmd cmd, string data)
        {
            if (data != null && data.Length > DATA_SIZE)
                throw new ArgumentOutOfRangeException("data", data, $"CommandPacket data component length cannot exceed {DATA_SIZE} bytes");

            this.Cmd = cmd;
            this.Data = data;
            this.DataLength = data == null 
                ? -1 
                : data.Length;
        }
        #endregion

        #region PUBLIC PROPERTIES
        public Command.Cmd Cmd { get; }
        public string Data { get; }
        public int DataLength { get; }
        #endregion

        #region PUBLIC METHODS
        public byte[] GetPacketBuffer()
        {
            //Create the packet buffer using the mask as a template
            var packetBuffer = GetEmptyPacketBuffer();

            //Set the command byte
            packetBuffer[COMMAND_OFFSET] = Command.Convert(this.Cmd);

            var dataLengthBuffer = BitConverter.GetBytes(this.DataLength);
            Array.Copy(dataLengthBuffer, 0, packetBuffer, DATA_LENGTH_OFFSET, DATA_LENGTH_SIZE);

            //If data is present, set the data bytes
            if (this.Data != null)
            {
                var dataBuffer = Encoding.ASCII.GetBytes(this.Data);
                Array.Copy(dataBuffer, 0, packetBuffer, DATA_OFFSET, dataBuffer.Length);
            }

            return packetBuffer;
        }

        public static byte[] GetEmptyPacketBuffer()
        {
            var packetBuffer = new byte[PACKET_SIZE];

            SetStartDelimiter(packetBuffer);
            SetEndDelimiter(packetBuffer);

            return packetBuffer;
        }

        public static CommandPacket Read(Stream stream)
        {
            if(stream == null)
                return null;

            //Create the packet buffer of the same size as the mask
            //and read as many bytes from the stream
            var packetBuffer = new byte[PACKET_SIZE];
            ReadToBuffer(stream, packetBuffer);

            //If the packet buffer does not comform to the mask, 
            //then the data read from the stream is garbage
            if (!CheckPacketFormat(packetBuffer))
                return null;

            //Get the command
            var cmd = Command.Convert(packetBuffer[COMMAND_OFFSET]);

            var dataLengthBuffer = new byte[DATA_LENGTH_SIZE];
            Array.Copy(packetBuffer, DATA_LENGTH_OFFSET, dataLengthBuffer, 0, DATA_LENGTH_SIZE);

            //Data length should not exceed the packet slot alotment defined by DATA_SIZE
            var dataLength = Math.Min(
                BitConverter.ToInt32(dataLengthBuffer, 0), 
                DATA_SIZE);

            string data = null;
            if (dataLength == 0)
            {
                data = string.Empty;
            }
            else if (dataLength == -1)
            {
                data = null;
            }
            else
            {
                //Parse out the data section
                var dataBuffer = new byte[dataLength];
                Array.Copy(packetBuffer, DATA_OFFSET, dataBuffer, 0, dataLength);

                data = HasData(dataBuffer)
                    ? Encoding.ASCII.GetString(dataBuffer)
                    : null;
            }

            return new CommandPacket(cmd, data);
        }
        #endregion

        #region PRIVATE METHODS
        private static void SetStartDelimiter(byte[] buffer)
        {
            for (var i = 0; i < CommandPacket.DELIMITER_COUNT; i++)
                buffer[i] = CommandPacket.DELIMITER_START_BYTE;
        }

        private static void SetEndDelimiter(byte[] buffer)
        {
            for (var i = CommandPacket.PACKET_SIZE - 1; i >= (CommandPacket.PACKET_SIZE - CommandPacket.DELIMITER_COUNT); i--)
                buffer[i] = CommandPacket.DELIMITER_END_BYTE;
        }

        private static void ReadToBuffer(
            Stream stream, 
            byte[] packetBuffer)
        {
            var index = 0;
            var count = packetBuffer.Length;
            //We must be able to read the stream
            //We also must potentially continue reading if the initial read has not read all the available bytes
            while (stream.CanRead && count > 0)
            {
                var read = stream.Read(packetBuffer, index, count);
                //Nothing was read from the stream - so there is nothing more to be read
                if (read == 0)
                    break;

                index += read;
                count -= read;
            }
        }

        private static bool HasData(byte[] buffer)
        {
            return buffer.Any(x => x != 0);
        }

        private static bool CheckPacketFormat(byte[] buffer)
        {
            var result = true;
            for (var i = 0; i < PACKET_SIZE; i++)
            {
                var isStartDelimiterByte = i < PACKET_BODY_OFFSET;
                if(isStartDelimiterByte && buffer[i] != DELIMITER_START_BYTE)
                {
                    result = false;
                    break;
                }

                var isEndDelimiterByte = i >= (PACKET_BODY_OFFSET + PACKET_BODY_SIZE);
                if (isEndDelimiterByte && buffer[i] != DELIMITER_END_BYTE)
                {
                    result = false;
                    break;
                }
            }//End for

            return result;
        }
        #endregion
    }
}
