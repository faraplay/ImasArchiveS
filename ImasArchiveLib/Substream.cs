using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    internal class Substream: Stream
    {
        private readonly Stream _stream;
        private readonly long _base_offset;
        private readonly long _length;
        private long _position;
        private static readonly object _lock = new object();
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
            _stream = stream;
            _base_offset = offset;
            _length = length;
            _position = 0;
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanSeek => true;

        public override long Length { get => _length; }
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
            //lock (_lock)
            //{
            //    _stream.Position = _position + _base_offset;
            //}
            return _position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
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
