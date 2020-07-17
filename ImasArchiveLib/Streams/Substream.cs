using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.Streams
{
    internal class Substream: Stream
    {
        private readonly Stream _stream;
        private readonly long _base_offset;
        private readonly long _length;
        private long _position;
        private static readonly object _lock = new object();

        #region Constructors
        /// <summary>
        /// Initialises a new instance of the Substream class with the specified stream, offset and length.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">stream does not support reading and seeking.</exception>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public Substream(Stream stream, long offset, long length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException(Strings.Argument_NotReadAndSeek, nameof(stream));
            }
            if (offset < 0 || offset + length > stream.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            _stream = stream;
            _base_offset = offset;
            _length = length;
            _position = 0;
        }
        #endregion
        #region Properties
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanSeek => true;

        public override long Length { get => _length; }
        /// <summary>
        /// The current position, relative to the substream offset.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value is negative or greater than the length of the stream.</exception>
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), Strings.ArgumentOutOfRangeException_Negative);
                if (value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value), Strings.ArgumentOutOfRangeException_StringLength);
                _position = value;
                lock (_lock)
                {
                    _stream.Position = value + _base_offset;
                }
            }
        }
        #endregion

        /// <summary>
        /// Sets the position of the stream.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns>The new position of the stream.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long tempPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length - offset,
                _ => throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_Enum),
            };
            if (tempPosition < 0)
                throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_Negative);
            if (tempPosition > _length)
                throw new ArgumentOutOfRangeException(Strings.ArgumentOutOfRangeException_StringLength);
            _position = tempPosition;
            return _position;
        }

        /// <summary>
        /// Reads count bytes from the stream into the buffer at position offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();
            int n = (int)(_length - _position);
            if (n > count)
                n = count;
            int result;
            lock (_lock)
            {
                _stream.Seek(_base_offset + _position, SeekOrigin.Begin);
                result = _stream.Read(buffer, offset, n);
            }
            _position += result;
            return result;
        }

        public override int ReadByte()
        {
            lock (_lock)
            {
                _stream.Seek(_base_offset + _position, SeekOrigin.Begin);
                _position++;
                return _stream.ReadByte();
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(Strings.NotSupported);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(Strings.NotSupported_UnwritableStream);
        }

        public override void Flush()
        {
        }
    }
}
