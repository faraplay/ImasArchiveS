using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    internal class SegsStream : Stream
    {
        private Stream _stream;
        private readonly SegsStreamMode _mode;

        private long _length;
        private long _offset;
        private long _position;
        private int _block_count;
        private long[] blockOffsets;
        private int[] blockCompSizes;
        private int[] blockUncompSizes;
        private bool[] blockIsCompressed;

        private const int MaxBlockSize = 0x10000;
        private byte[] _buffer;
        private int _buffer_size;
        private int _avail_in;

        public SegsStream(Stream stream, SegsStreamMode mode)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            switch (mode)
            {
                case SegsStreamMode.Decompress:
                    if (!stream.CanRead)
                        throw new ArgumentException(Strings.NotSupported_UnreadableStream, nameof(stream));
                    if (!stream.CanSeek)
                        throw new ArgumentException(Strings.NotSupported_UnseekableStream, nameof(stream));
                    _stream = stream;
                    _mode = mode;
                    ReadHeader();
                    _position = 0;
                    _buffer_size = 0;
                    _avail_in = 0;
                    break;
                case SegsStreamMode.Compress:
                    if (!stream.CanWrite)
                        throw new ArgumentException(Strings.NotSupported_UnwritableStream, nameof(stream));
                    if (!stream.CanSeek)
                        throw new ArgumentException(Strings.NotSupported_UnseekableStream, nameof(stream));
                    _stream = stream;
                    _mode = mode;
                    _position = 0;
                    _buffer_size = 0;
                    break;
                default:
                    throw new ArgumentException(Strings.ArgumentOutOfRangeException_Enum, nameof(mode));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_stream == null)
                return;

            if (disposing)
            {
                _stream.Dispose();
                _stream = null;
            }

            base.Dispose(disposing);
        }

        private void EnsureNotDisposed()
        {
            if (_stream == null)
                throw new ObjectDisposedException(Strings.ObjectDisposed_StreamClosed);
        }

        private void ReadHeader()
        {
            if (Utils.GetUInt(_stream) != 0x73656773u)
                throw new InvalidDataException(Strings.InvalidData_SegsHeader);
            if (Utils.GetUShort(_stream) != 5)
                throw new InvalidDataException(Strings.InvalidData_SegsHeader);
            _block_count = Utils.GetUShort(_stream);
            _length = Utils.GetUInt(_stream);
            if (Utils.GetUInt(_stream) != _stream.Length)
                throw new InvalidDataException(Strings.InvalidData_SegsHeader);

            blockOffsets = new long[_block_count];
            blockCompSizes = new int[_block_count];
            blockUncompSizes = new int[_block_count];
            blockIsCompressed = new bool[_block_count];
            for (int i = 0; i < _block_count; i++)
            {
                blockCompSizes[i] = Utils.GetUShort(_stream);
                if (blockCompSizes[i] == 0)
                    blockCompSizes[i] = MaxBlockSize;
                blockUncompSizes[i] = Utils.GetUShort(_stream);
                if (blockUncompSizes[i] != 0 && i != _block_count - 1)
                    throw new InvalidDataException(Strings.InvalidData_SegsHeader);
                if (blockUncompSizes[i] == 0)
                    blockUncompSizes[i] = MaxBlockSize;
                uint blockOffset = Utils.GetUInt(_stream);
                blockIsCompressed[i] = (blockOffset % 2 == 1);
                blockOffsets[i] = blockOffset & -2;
            }

        }

        private void ReadBlockIntoBuffer(int index)
        {
            EnsureBufferInitialised();

            _buffer_size = blockUncompSizes[index];
            _stream.Seek(blockOffsets[index], SeekOrigin.Begin);
            if (blockIsCompressed[index])
            {
                using DeflateStream deflateStream = new DeflateStream(_stream, CompressionMode.Decompress, true);
                deflateStream.Read(_buffer, 0, _buffer_size);
                if (deflateStream.ReadByte() != -1)
                    throw new InvalidDataException(Strings.InvalidData_Segs);
            }
            else
            {
                _stream.Read(_buffer, 0, _buffer_size);
            }
        }

        private void FillBuffer()
        {
            if (_position >= _length)
            {
                _buffer_size = 0;
                _avail_in = 0;
            }
            else
            {
                int index = (int)(_position / MaxBlockSize);
                ReadBlockIntoBuffer(index);
                _avail_in = _buffer_size - (int)(_position % MaxBlockSize);
            }
        }

        private void InitialiseBuffer()
        {
            if (_block_count == 1)
                _buffer = new byte[blockUncompSizes[0]];
            else
                _buffer = new byte[MaxBlockSize];
        }

        private void EnsureBufferInitialised()
        {
            if (_buffer == null)
                InitialiseBuffer();
        }

        public override bool CanRead
        {
            get
            {
                if (_stream == null)
                    return false;
                return (_mode == SegsStreamMode.Decompress && _stream.CanRead);
            }
        }
        public override bool CanWrite
        {
            get
            {
                if (_stream == null)
                    return false;
                return (_mode == SegsStreamMode.Compress && _stream.CanWrite);
            }
        }
        public override bool CanSeek
        {
            get
            {
                if (_stream == null)
                    return false;
                return (_mode == SegsStreamMode.Decompress && _stream.CanSeek);
            }
        }

        public override long Length
        {
            get
            {
                EnsureNotDisposed();
                if (_mode == SegsStreamMode.Decompress)
                    return _length;
                else
                    throw new NotSupportedException(Strings.NotSupported);
            }
        }

        public override long Position
        {
            get
            {
                EnsureNotDisposed();
                if (_mode == SegsStreamMode.Decompress)
                    return _position;
                else
                    throw new NotSupportedException(Strings.NotSupported);
            }

            set
            {
                EnsureNotDisposed();
                if (_mode != SegsStreamMode.Decompress)
                    throw new NotSupportedException(Strings.NotSupported);

                if (value < 0)
                    throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_Negative);
                if (value > _length)
                    throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_StringLength);

                _position = value;
                FillBuffer();
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(Strings.NotSupported);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_mode != SegsStreamMode.Decompress)
                throw new NotSupportedException(Strings.NotSupported);

            EnsureNotDisposed();

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

        public override int ReadByte()
        {
            EnsureNotDisposed();
            EnsureBufferInitialised();

            if (_avail_in == 0)
            {
                FillBuffer();
                if (_buffer_size == 0)
                    return -1;
            }
            byte b = _buffer[_position % MaxBlockSize];
            _position++;
            _avail_in--;
            return b;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateParameters(buffer, offset, count);
            return ReadCore(new Span<byte>(buffer, offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            return ReadCore(buffer);
        }

        private int ReadCore(Span<byte> buffer)
        {
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

                Span<byte> _buf_remain = new Span<byte>(_buffer, (int)(_position % MaxBlockSize), lengthToRead);
                _buf_remain.CopyTo(buffer.Slice(totalRead, lengthToRead));
                _avail_in -= lengthToRead;
                _position += lengthToRead;
                totalRead += lengthToRead;

                if (totalRead == buffer.Length)
                    break;

                FillBuffer();
                if (_buffer_size == 0)
                    break;
            }
            return totalRead;

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
        }
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

        private SegsStream()
        {

        }

        public static async Task CompressStream(Stream inStream, Stream outStream)
        {
            if (!inStream.CanRead)
                throw new NotSupportedException(Strings.NotSupported_UnreadableStream);
            if (!outStream.CanWrite || !outStream.CanSeek)
                throw new NotSupportedException(Strings.NotSupported_UnwritableStream);

            using SegsStream segsStream = new SegsStream
            {
                _stream = outStream
            };
            segsStream.WriteHeader((int)inStream.Length);
            segsStream._buffer = new byte[MaxBlockSize];
            for (int i = 0; i < segsStream._block_count; i++)
                await segsStream.WriteBlock(inStream);
            segsStream.UpdateHeader((int)inStream.Length);
            segsStream._stream = null;
        }

        private void WriteHeader(int fileLength)
        {
            _block_count = (fileLength + 0xFFFF) / 0x10000;
            Utils.PutUInt(_stream, 0x73656773u);
            Utils.PutUShort(_stream, 5);
            Utils.PutUShort(_stream, (ushort)_block_count);
            Utils.PutUInt(_stream, (uint)fileLength);
            Utils.PutUInt(_stream, 0);

            _stream.Write(new byte[8 * _block_count]);

            _offset = 16 + (8 * _block_count);
            long pad = (-_offset) & 15;
            _stream.Write(new byte[pad]);
            _offset += pad;

            blockOffsets = new long[_block_count];
            blockCompSizes = new int[_block_count];
            blockUncompSizes = new int[_block_count];
            blockIsCompressed = new bool[_block_count];
        }

        private async Task WriteBlock(Stream inStream)
        {
            int blockIndex = (int)(_position / MaxBlockSize);
            byte[] inBuffer = new byte[MaxBlockSize];
            int uncompSize = inStream.Read(inBuffer);
            blockUncompSizes[blockIndex] = uncompSize;
            _position += blockUncompSizes[blockIndex];
            int compSize;
            using (MemoryStream memoryStream = new MemoryStream(inBuffer, 0, uncompSize))
            {
                using MemoryStream memoryStream1 = new MemoryStream();
                using (DeflateStream deflateStream = new DeflateStream(memoryStream1, CompressionLevel.Optimal, true))
                {
                    await memoryStream.CopyToAsync(deflateStream);
                }
                compSize = (int)memoryStream1.Length;
                if (compSize >= uncompSize)
                {
                    inBuffer.CopyTo(_buffer, 0);
                    blockIsCompressed[blockIndex] = false;
                    compSize = uncompSize;
                }
                else
                {
                    memoryStream1.Position = 0;
                    memoryStream1.Read(_buffer);
                    blockIsCompressed[blockIndex] = true;
                }
            }
            blockCompSizes[blockIndex] = compSize;
            blockOffsets[blockIndex] = _offset;
            _stream.Write(_buffer, 0, compSize);
            int pad = (-compSize) & 15;
            _stream.Write(new byte[pad]);
            _offset += compSize + pad;
        }

        private void UpdateHeader(int fileLength)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            Utils.PutUInt(_stream, 0x73656773u);
            Utils.PutUShort(_stream, 5);
            Utils.PutUShort(_stream, (ushort)_block_count);
            Utils.PutUInt(_stream, (uint)fileLength);
            Utils.PutUInt(_stream, (uint)_offset);

            for (int i = 0; i < _block_count; i++)
            {
                if (blockCompSizes[i] == MaxBlockSize)
                    blockCompSizes[i] = 0;
                Utils.PutUShort(_stream, (ushort)blockCompSizes[i]);
                if (blockUncompSizes[i] == MaxBlockSize)
                    blockUncompSizes[i] = 0;
                Utils.PutUShort(_stream, (ushort)blockUncompSizes[i]);
                if (blockIsCompressed[i])
                    blockOffsets[i] += 1;
                Utils.PutUInt(_stream, (uint)blockOffsets[i]);
            }
        }
    }
}
