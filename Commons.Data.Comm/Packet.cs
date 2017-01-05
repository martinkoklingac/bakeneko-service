using Commons.Ext;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using static Commons.Data.Comm.Command;

namespace Commons.Data.Comm
{
    /// <summary>
    /// Provides base fields that constitute the header of the packet that
    /// can be used to send messages and commands to BakenekoService. 
    /// Also provides utility methods for parsing out & validating the structured data 
    /// from a raw byte buffer structure.
    /// </summary>

    /*
    
    +-------------------+---------------------------------------+---------------+-------------------+
    | Field             | Field Offset                          | Field Size    |                   |
    +-------------------+---------------------------------------+---------------+-------------------+
    | DELIM_START_BYTE  | DELIM_OFFSET                          | 1 byte        | Type Info Segment |
    +-------------------+---------------------------------------+---------------+                   |
    | Packet.PacCmd     | PAC_CMD_OFFSET                        | 1 byte        |                   |
    +-------------------+---------------------------------------+---------------+-------------------+
    | Packet.DataSize   | DATA_SIZE_OFFSET                      | 4 bytes       | Data Info Segment |
    +-------------------+---------------------------------------+---------------+                   |
    | Packet.DataHash   | DATA_HASH_OFFSET                      | 16 bytes      |                   |
    +-------------------+---------------------------------------+---------------+-------------------+
    | <data>            | DATA_OFFSET                           | N bytes       | Data Segment      |
    +-------------------+---------------------------------------+---------------+-------------------+
     
    */
    public abstract class Packet
    {
        #region PUBLIC FIELDS
        public const int DELIM_OFFSET = 0;
        public const int DELIM_LENGTH = 1;
        public const int PAC_CMD_OFFSET = 1;
        public const int PAC_CMD_LENGTH = 1;
        public const int DATA_SIZE_OFFSET = 2;
        public const int DATA_SIZE_LENGTH = 4;
        public const int DATA_HASH_OFFSET = 6;
        public const int DATA_HASH_LENGTH = 16;
        public const int DATA_OFFSET = 22;

        public const byte DELIM_BYTE = (byte)'<';

        public const int HEADER_LENGTH =
            DELIM_LENGTH +
            PAC_CMD_LENGTH +
            DATA_SIZE_LENGTH +
            DATA_HASH_LENGTH;
        #endregion

        #region PRIVATE FIELDS
        /// <summary>
        /// Indicates if the packet contains valid data in entirety
        /// </summary>
        protected bool? _isValid;
        
        /// <summary>
        /// Indicates if <see cref="DataHash"/> matches computed data hash from raw buffer
        /// </summary>
        private readonly bool _isDataValid;

        /** Type segment fields */
        private PacCmd _pacCmd;
        private byte _delim;

        /** Data segment fields */
        private byte[] _dataHash;
        private int _size;
        private int _dataSize;
        #endregion

        #region CONSTRUCTORS
        protected Packet(Stream stream)
        {
            var buffer = GetHeaderBuffer(stream);
            this.ParseHeader(buffer);

            if (this._size > 0 && this._dataSize > 0)
            {
                buffer = GetDataBuffer(stream, this._dataSize);
                //Compare the computed data hash with header DataHash field
                this._isDataValid = this._dataHash
                    .SequenceEqual(GetHash(buffer));

                //If two hashes don't match, don't parse the data since the packet is already invalid
                if (this._isDataValid)
                    this.ParseData(buffer);
            }
            else
            {
                this._isDataValid = true;
            }
        }

        protected Packet(PacCmd pacCmd) :
            this(pacCmd, null)
        { }

        protected Packet(PacCmd pacCmd, byte[] data)
        {
            this.SetTypeSegment(DELIM_BYTE, pacCmd);
            this.SetDataSegment(data);
            this._isDataValid = true;   //Header DataHash field is set to computed hash from data buffer
        }
        #endregion

        #region PUBLIC PROPERTIES
        public int Size { get { return this._size; } }
        public int DataSize { get { return this._dataSize; } }
        public PacCmd PacCmd { get { return this._pacCmd; } }
        public byte[] DataHash { get { return this._dataHash; } }
        #endregion

        #region PRIVATE PROPERTIES
        protected byte Delim { get; }
        #endregion

        #region PUBLIC METHODS
        public virtual bool IsValid()
        {
            if (this._isValid.HasValue)
                return this._isValid.Value;

            if(!this._isDataValid)
                return (this._isValid = false).Value;

            if (this._delim != DELIM_BYTE)
                return (this._isValid = false).Value;

            if (this._size < HEADER_LENGTH || this._dataSize < 0)
                return (this._isValid = false).Value;

            if (!EnumExt.GetValues<PacCmd>().Any(x => x == this._pacCmd))
                return (this._isValid = false).Value;

            return (this._isValid = true).Value;
        }

        public virtual byte[] GetBytes()
        {
            var buffer = new byte[this._size];
            SetDelim(buffer);
            SetPacCmd(buffer, this._pacCmd);
            SetDataSize(buffer, this._dataSize);
            SetDataHash(buffer, this._dataHash);

            return buffer;
        }

        public override bool Equals(object obj)
        {
            var packet = obj as Packet;
            return packet != null
                && packet.Delim == this.Delim
                && packet.PacCmd == this.PacCmd
                && packet.DataSize == this.DataSize
                && packet.Size == this.Size
                && packet.DataHash.SequenceEqual(this.DataHash);
        }

        public static void SetDelim(byte[] buffer)
        {
            buffer[DELIM_OFFSET] = DELIM_BYTE;
        }

        public static void SetPacCmd(byte[] buffer, PacCmd pacCmd)
        {
            buffer[PAC_CMD_OFFSET] = (byte)pacCmd;
        }

        public static void SetDataSize(byte[] buffer, int dataSize)
        {
            Array.Copy(BitConverter.GetBytes(dataSize), 0, buffer, DATA_SIZE_OFFSET, DATA_SIZE_LENGTH);
        }

        public static void SetDataHash(byte[] buffer, byte[] dataHash)
        {
            Array.Copy(dataHash, 0, buffer, DATA_HASH_OFFSET, DATA_HASH_LENGTH);
        }

        public static byte[] GetHash(byte[] buffer)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(buffer);
        }
        #endregion

        #region PROTECTED METHODS
        protected virtual void ParseData(byte[] buffer) { }
        #endregion

        #region PRIVATE METHODS
        private void ParseHeader(byte[] buffer)
        {
            this.SetTypeSegment(
                buffer[DELIM_OFFSET],
                buffer[PAC_CMD_OFFSET]);

            this.SetDataSegment(
                BitConverter.ToInt32(buffer, DATA_SIZE_OFFSET),
                buffer.Slice(DATA_HASH_OFFSET, DATA_HASH_OFFSET + DATA_HASH_LENGTH - 1));
        }

        private void SetTypeSegment(byte delim, PacCmd pacCmd)
        {
            this._delim = delim;
            this._pacCmd = pacCmd;
        }

        private void SetTypeSegment(byte delim, byte pacCmd) { this.SetTypeSegment(delim, (PacCmd)pacCmd); }

        private void SetDataSegment(byte[] data)
        {
            if (data.IsNullOrEmpty())
                this.SetDataSegment(0, new byte[DATA_HASH_LENGTH]);
            else
                this.SetDataSegment(data.Length, GetHash(data));
        }

        private void SetDataSegment(int dataSize, byte[] hashBuffer)
        {
            this._dataSize = dataSize;
            this._size = this._dataSize + HEADER_LENGTH;
            this._dataHash = hashBuffer;
        }

        private static byte[] GetHeaderBuffer(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var buffer = new byte[HEADER_LENGTH];

            if (stream.CanRead)
                stream.Read(buffer, 0, HEADER_LENGTH);

            return buffer;
        }

        private static byte[] GetDataBuffer(Stream stream, int bufferSize)
        {
            try
            {
                var buffer = new byte[bufferSize];

                var index = 0;
                var count = buffer.Length;
                //We must be able to read the stream
                //We also must potentially continue reading if the initial read has not read all the available bytes
                while (stream.CanRead && count > 0)
                {
                    //The buffer is an empty array so we can't read anything into it
                    if (count == 0)
                        break;

                    var read = stream.Read(buffer, index, count);
                    //Nothing was read from the stream - so there is nothing more to be read
                    if (read == 0)
                        break;

                    index += read;
                    count -= read;
                }//End while

                return buffer;
            }//End try
            catch (Exception e)
            {
                return new byte[0];
            }
        }
        #endregion
    }

}
