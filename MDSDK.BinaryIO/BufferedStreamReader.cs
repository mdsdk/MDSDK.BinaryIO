// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.IO;

namespace MDSDK.BinaryIO
{
    public sealed class BufferedStreamReader
    {
        public Stream Stream { get; }

        private readonly byte[] _buffer;

        private int _bufferReadPointer;

        private int _bufferedDataLength;

        public BufferedStreamReader(Stream stream, int bufferSize = 4096)
        {
             Stream = stream;

            _buffer = new byte[bufferSize];
            _bufferReadPointer = 0;
            _bufferedDataLength = 0;

            Position = stream.CanSeek ? stream.Position : 0;
            Length = stream.CanSeek ? stream.Length : long.MaxValue;
        }

        public BufferedStreamReader(byte[] data, int length)
        {
            _buffer = data;
            _bufferReadPointer = 0;
            _bufferedDataLength = length;

            Position = 0;
            Length = length;
        }

        public BufferedStreamReader(byte[] data)
            : this(data, data.Length)
        {
        }

        public long Position { get; private set; }

        public long Length { get; private set; }

        public long BytesRemaining => Length - Position;

        public bool AtEnd => Position == Length;

        internal void PreAdvancePosition(int count)
        {
            var newPosition = Position + count;
            if (newPosition > Length)
            {
                throw new IOException($"Attempt to consume {newPosition - Length} more bytes than available");
            }
            Position = newPosition;
        }

        internal void BeginReadFromBuffer(int count)
        {
            if (_bufferedDataLength < count)
            {
                if (Stream == null)
                {
                    throw new IOException("Attempt to read beyond end of data");
                }

                if (_bufferReadPointer + count > _buffer.Length)
                {
                    _buffer.AsSpan(_bufferReadPointer, _bufferedDataLength).CopyTo(_buffer);
                    _bufferReadPointer = 0;
                }

                do
                {
                    var bytesRead = Stream.Read(_buffer.AsSpan(_bufferReadPointer + _bufferedDataLength));
                    if (bytesRead == 0)
                    {
                        throw new IOException("Unexpected end of stream");
                    }
                    _bufferedDataLength += bytesRead;
                }
                while (_bufferedDataLength < count);
            }
        }

        private void EndReadFromBuffer(int count)
        {
            _bufferedDataLength -= count;
            _bufferReadPointer = (_bufferedDataLength == 0) ? 0 : _bufferReadPointer + count;
        }

        public byte ReadByte()
        {
            PreAdvancePosition(1);
            BeginReadFromBuffer(1);
            var datum = _buffer[_bufferReadPointer];
            EndReadFromBuffer(1);
            return datum;
        }

        internal Span<byte> GetBufferReadSpan(int count)
        {
            PreAdvancePosition(count);
            BeginReadFromBuffer(count);
            var readSpan = _buffer.AsSpan(_bufferReadPointer, count);
            EndReadFromBuffer(count);
            return readSpan;
        }

        public int ReadSome(Span<byte> data)
        {
            int n;

            var remainingBufferedDataLength = (int)Math.Min(BytesRemaining, _bufferedDataLength);
            if (remainingBufferedDataLength > 0)
            {
                n = Math.Min(remainingBufferedDataLength, data.Length);
                var bufferReadSpan = GetBufferReadSpan(n);
                bufferReadSpan.CopyTo(data);
            }
            else
            {
                var maxBytesToReadFromStream = (int)Math.Min(BytesRemaining, data.Length);
                n = Stream.Read(data.Slice(0, maxBytesToReadFromStream));
                Position += n;
            }

            return n;
        }

        public void ReadAll(Span<byte> data)
        {
            while (data.Length > 0)
            {
                var n = ReadSome(data);
                if (n == 0)
                {
                    throw new IOException("Unexpected end of stream");
                }
                data = data[n..];
            }
        }

        public void Read(long byteCount, Action readAction)
        {
            var newLength = Position + byteCount;
            if (newLength > Length)
            {
                throw new IOException($"Read {byteCount} bytes exceeds container by {newLength - Length} bytes");
            }
            var oldLength = Length;
            Length = newLength;
            readAction.Invoke();
            if (!AtEnd)
            {
                throw new Exception($"Read action left {BytesRemaining} of {byteCount} bytes unread");
            }
            Length = oldLength;
        }

        public T Read<T>(long length, Func<T> readFunc)
        {
            T result = default;
            Read(length, new Action(() => result = readFunc.Invoke()));
            return result;
        }

        public byte[] ReadBytes(long count)
        {
            var bytes = new byte[count];
            ReadAll(bytes);
            return bytes;
        }

        public byte[] ReadRemainingBytes()
        {
            return ReadBytes(BytesRemaining);
        }

        public void SkipBytes(long count)
        {
            Position += count;

            if (count <= _bufferedDataLength)
            {
                EndReadFromBuffer((int)count);
            }
            else
            {
                count -= _bufferedDataLength;
                EndReadFromBuffer(_bufferedDataLength);

                if (Stream.CanSeek)
                {
                    Stream.Seek(count, SeekOrigin.Current);
                }
                else
                {
                    while (count > 0)
                    {
                        var n = (int)Math.Min(count, _buffer.Length);
                        var bytesRead = Stream.Read(_buffer, 0, n);
                        if (bytesRead == 0)
                        {
                            throw new IOException("Unexpected end of stream");
                        }
                        count -= bytesRead;
                    }
                }
            }
        }

        public void SkipRemainingBytes()
        {
            SkipBytes(BytesRemaining);
        }
    }
}
