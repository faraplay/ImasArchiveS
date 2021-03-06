﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Imas.Streams
{
    internal class FlowbishStream : Stream
    {
        private const int DefaultBufferSize = 0x2000;
        private Stream _stream;
        private readonly FlowbishStreamMode _mode;
        private readonly FlowbishBox _box;
        private readonly string _key;
        private readonly bool leaveOpen;
        private bool disposed = false;

        private long _length;
        private long _offset;
        private long _position;
        private byte[] _buffer;
        private int _buffer_current_size;
        private long _buffer_offset;
        private int _avail_in;
        private int _avail_out;
        private static readonly string secret = "86F958D4-46A8-4F31-A90F-74A7F23609ED";

        private static uint[] ToUIntArray(string s, int length)
        {
            uint[] vs = new uint[length];
            int pos = 0;
            for (int i = 0; i < length; i++)
            {
                uint val = 0;
                for (int j = 0; j < 4; j++)
                {
                    if (pos >= s.Length)
                        pos -= s.Length;
                    val = (val << 8) | ((uint)s[pos]);
                    pos++;
                }
                vs[i] = val;
            }
            return vs;
        }

        /// <summary>
        /// Initialises a new instance of the FlowbishStream class.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        /// <param name="mode">The mode of operation.</param>
        /// <param name="key">The key used for encryption (usually the filename).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        ///
        public FlowbishStream(Stream stream, FlowbishStreamMode mode, string key) : this(stream, mode, key, false)
        {
        }

        /// <summary>
        /// Initialises a new instance of the FlowbishStream class.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        /// <param name="mode">The mode of operation.</param>
        /// <param name="key">The key used for encryption (usually the filename).</param>
        /// <param name="leaveOpen">Whether to leave the file open after disposal.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public FlowbishStream(Stream stream, FlowbishStreamMode mode, string key, bool leaveOpen)
        {
            if (stream == null || key == null)
                throw new ArgumentNullException(nameof(stream));

            _key = key;
            this.leaveOpen = leaveOpen;
            switch (mode)
            {
                case FlowbishStreamMode.Decipher:
                    if (!stream.CanRead)
                        throw new ArgumentException(Strings.NotSupported_UnreadableStream, nameof(stream));
                    if (!stream.CanSeek)
                        throw new ArgumentException(Strings.NotSupported_UnseekableStream, nameof(stream));
                    _box = new FlowbishBox(ToUIntArray(key + secret, FlowbishBox.keyUIntLength));
                    _stream = stream;
                    _mode = FlowbishStreamMode.Decipher;
                    ReadHeader();
                    _avail_in = 0;
                    _buffer_offset = 0;
                    InitialiseBuffer();
                    break;

                case FlowbishStreamMode.Encipher:
                    if (!stream.CanWrite)
                        throw new ArgumentException(Strings.NotSupported_UnwritableStream, nameof(stream));
                    _box = new FlowbishBox(ToUIntArray(key + secret, FlowbishBox.keyUIntLength));
                    _stream = stream;
                    _mode = FlowbishStreamMode.Encipher;
                    _avail_out = 0;
                    _buffer_offset = 0;
                    _position = 0;
                    _length = 0;
                    try
                    {
                        WriteHeader();
                    }
                    catch (EncoderFallbackException)
                    {
                        throw new ArgumentException(nameof(key));
                    }
                    break;

                default:
                    throw new ArgumentException(Strings.ArgumentOutOfRangeException_Enum, nameof(mode));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (_mode == FlowbishStreamMode.Encipher)
            {
                try
                {
                    Flush();
                }
                catch (IOException)
                {
                }
            }

            if (disposing)
            {
                if (!leaveOpen)
                {
                    _stream.Dispose();
                }
                _stream = null;
            }
            base.Dispose(disposing);
            disposed = true;
        }

        /// <exception cref="InvalidDataException"/>
        private void ReadHeader()
        {
            try
            {
                Binary binary = new Binary(_stream, true);
                if (binary.ReadUInt32() != 0x00464253)
                    throw new InvalidDataException(Strings.InvalidData_FbsHeader);
                if (binary.ReadUInt32() != 0)
                    throw new InvalidDataException(Strings.InvalidData_FbsHeader);
                _length = binary.ReadUInt32();
                int keyLength = _stream.ReadByte();
                if (keyLength != _key.Length + 1)
                    throw new InvalidDataException(Strings.InvalidData_FbsKey);
                if (_stream.ReadByte() != 0)
                    throw new InvalidDataException(Strings.InvalidData_FbsHeader);
                if (_stream.ReadByte() != 0)
                    throw new InvalidDataException(Strings.InvalidData_FbsHeader);
                if (_stream.ReadByte() != 0)
                    throw new InvalidDataException(Strings.InvalidData_FbsHeader);
                int padKeyLength = 8 * ((keyLength + 7) / 8);
                _offset = 16 + padKeyLength;
                byte[] keyBuffer = new byte[padKeyLength];
                _stream.Read(keyBuffer);
                _box.Decipher(keyBuffer);

                if (!(Encoding.ASCII.GetString(keyBuffer, 0, keyLength - 1) == _key && keyBuffer[keyLength - 1] == 0))
                    throw new InvalidDataException(Strings.InvalidData_FbsKey);

                if (_stream.Length != _length + _offset)
                    throw new InvalidDataException(Strings.InvalidData_FbsHeader);
                _position = 0;
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException(Strings.InvalidData_FbsHeader);
            }
        }

        /// <exception cref="IOException"/>
        /// <exception cref="EncoderFallbackException"/>
        /// <exception cref="ObjectDisposedException"/>
        private void WriteHeader()
        {
            Binary binary = new Binary(_stream, true);
            int keyLength = _key.Length + 1;
            binary.WriteUInt32(0x00464253);
            binary.WriteUInt32(0);
            binary.WriteUInt32((uint)_length);
            _stream.WriteByte((byte)keyLength);
            _stream.WriteByte(0);
            _stream.WriteByte(0);
            _stream.WriteByte(0);
            int padKeyLength = 8 * ((keyLength + 7) / 8);
            _offset = 16 + padKeyLength;
            byte[] keyBuffer = new byte[padKeyLength];
            Encoding.ASCII.GetBytes(_key).CopyTo(keyBuffer, 0);
            _box.Encipher(keyBuffer);
            _stream.Write(keyBuffer);
        }

        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private void UpdateHeader()
        {
            long pos = _stream.Position;
            _stream.Seek(8, SeekOrigin.Begin);
            Binary.WriteUInt32(_stream, true, (uint)_length);
            _stream.Position = pos;
        }

        private void InitialiseBuffer()
        {
            Debug.Assert(_buffer == null);
            _buffer = new Byte[DefaultBufferSize];
        }

        private void EnsureBufferInitialised()
        {
            if (_buffer == null)
                InitialiseBuffer();
        }

        public Stream BaseStream => _stream;

        public override bool CanRead
        {
            get
            {
                if (_stream == null)
                    return false;

                return (_mode == FlowbishStreamMode.Decipher && _stream.CanRead);
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_stream == null)
                    return false;

                return (_mode == FlowbishStreamMode.Encipher && _stream.CanWrite);
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (_stream == null)
                    return false;

                return (_stream.CanSeek);
            }
        }

        public override long Length
        {
            get => _length;
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_Negative);
                if (value > _length)
                    throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_StringLength);
                if (value % 8 != 0)
                    throw new NotSupportedException(Strings.NotSupported_PosNotMultipleOf8);

                if (_mode == FlowbishStreamMode.Decipher)
                {
                    _position = value;
                    if (_position >= _buffer_offset && _position < _buffer_offset + _buffer_current_size)
                    {
                        _avail_in = (int)(_buffer_offset + _buffer_current_size - _position);
                    }
                    else
                    {
                        _buffer_offset = _position;
                        FillAndDecryptBuffer();
                    }
                }
                else if (_mode == FlowbishStreamMode.Encipher)
                {
                    Flush();
                    _position = value;
                    _buffer_offset = _position;
                    _stream.Position = _position + _offset;
                }
            }
        }

        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Flush()
        {
            EnsureNotDisposed();
            EnsureBufferInitialised();
            if (_mode == FlowbishStreamMode.Encipher)
            {
                FlushBuffer();
                UpdateHeader();
            }
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long tempPos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), Strings.ArgumentOutOfRangeException_Enum)
            };

            Position = tempPos;
            return Position;
        }

        /// <exception cref="NotSupportedException"/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException(Strings.NotSupported);
        }

        /// <exception cref="IOException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override int ReadByte()
        {
            EnsureDecipherMode();
            EnsureNotDisposed();
            EnsureBufferInitialised();

            if (_avail_in == 0)
            {
                FillAndDecryptBuffer();
                if (_buffer_current_size == 0)
                    return -1;
            }
            byte b = _buffer[_position - _buffer_offset];
            _position++;
            _avail_in--;
            return b;
        }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateParameters(buffer, offset, count);
            return ReadCore(new Span<byte>(buffer, offset, count));
        }

        /// <exception cref="IOException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override int Read(Span<byte> buffer)
        {
            return ReadCore(buffer);
        }

        /// <exception cref="IOException"/>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        private int ReadCore(Span<Byte> buffer)
        {
            EnsureDecipherMode();
            EnsureNotDisposed();
            EnsureBufferInitialised();

            int totalRead = 0;
            while (true)
            {
                int bufferSize = buffer.Length - totalRead;
                int lengthToRead;
                if (bufferSize <= _avail_in)
                    lengthToRead = bufferSize;
                else
                    lengthToRead = _avail_in;

                Span<byte> _buf_remain = new Span<byte>(_buffer, (int)(_position - _buffer_offset), lengthToRead);
                _buf_remain.CopyTo(buffer.Slice(totalRead, lengthToRead));
                _avail_in -= lengthToRead;
                _position += lengthToRead;
                totalRead += lengthToRead;

                if (totalRead == buffer.Length)
                    break;

                FillAndDecryptBuffer();
                if (_buffer_current_size == 0)
                    break;
            }
            return totalRead;
        }

        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private void FillAndDecryptBuffer()
        {
            Debug.Assert(_position % 8 == 0);
            _buffer_offset = _position;
            _stream.Seek(_offset + _buffer_offset, SeekOrigin.Begin);
            int bytes = _stream.Read(_buffer, 0, _buffer.Length);
            if (bytes <= 0)
            {
                _buffer_current_size = 0;
                return;
            }
            else if (bytes > _buffer.Length)
                throw new InvalidDataException(Strings.GenericInvalidData);

            bytes = 8 * (bytes / 8);
            _buffer_current_size = bytes;
            _avail_in = bytes;
            _box.Decipher(new Span<byte>(_buffer, 0, _buffer_current_size));
        }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentException"/>
        private void ValidateParameters(byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (array.Length - offset < count)
                throw new ArgumentException(Strings.InvalidArgumentOffsetCount);
        }

        /// <exception cref="ObjectDisposedException"/>
        private void EnsureNotDisposed()
        {
            if (_stream == null)
                throw new ObjectDisposedException(Strings.ObjectDisposed_StreamClosed);
        }

        /// <exception cref="NotSupportedException"/>
        private void EnsureDecipherMode()
        {
            if (_mode != FlowbishStreamMode.Decipher)
                throw new NotSupportedException(Strings.CannotReadFromDecipherStream);
        }

        /// <exception cref="NotSupportedException"/>
        private void EnsureEncipherMode()
        {
            if (_mode != FlowbishStreamMode.Encipher)
                throw new NotSupportedException(Strings.CannotWriteToEncipherStream);
        }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateParameters(buffer, offset, count);
            WriteCore(new Span<byte>(buffer, offset, count));
        }

        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            WriteCore(buffer);
        }

        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        private void WriteCore(ReadOnlySpan<byte> buffer)
        {
            EnsureNotDisposed();
            EnsureEncipherMode();
            EnsureBufferInitialised();

            int totalWritten = 0;
            while (true)
            {
                int bufferSize = buffer.Length - totalWritten;
                int lengthToWrite;
                if (bufferSize <= _avail_out)
                    lengthToWrite = bufferSize;
                else
                    lengthToWrite = _avail_out;

                Span<byte> _buf_remain = new Span<byte>(_buffer, (int)(_position - _buffer_offset), lengthToWrite);
                buffer.Slice(totalWritten, lengthToWrite).CopyTo(_buf_remain);
                _avail_out -= lengthToWrite;
                _position += lengthToWrite;
                totalWritten += lengthToWrite;

                if (totalWritten == buffer.Length)
                    break;

                FlushBuffer();
            }
            if (_position > _length)
                _length = _position;
        }

        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private void FlushBuffer()
        {
            EnsureBufferInitialised();
            int bytes = (int)(_position - _buffer_offset);
            bytes += (-bytes & 7);
            _position += (-bytes & 7);
            Span<byte> toWrite = new Span<byte>(_buffer, 0, bytes);
            _box.Encipher(toWrite);
            _stream.Write(toWrite);

            _length = _stream.Length - _offset;
            _buffer_offset = _position;
            _buffer_current_size = _buffer.Length;
            _avail_out = _buffer_current_size;
            Array.Clear(_buffer, 0, _buffer_current_size);
        }
    }
}