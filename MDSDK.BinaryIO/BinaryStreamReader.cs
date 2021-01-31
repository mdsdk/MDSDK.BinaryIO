// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace MDSDK.BinaryIO
{
    public sealed class BinaryStreamReader
    {
        public Stream Stream { get; }

        public ByteOrder ByteOrder { get; set; }

        private readonly byte[] _buffer;

        private int _bufferReadPointer;

        private int _bufferedDataLength;

        private long _position;

        private long _length;

        public BinaryStreamReader(Stream stream, ByteOrder byteOrder, int bufferSize = 4096)
        {
            Stream = stream;

            ByteOrder = byteOrder;

            _buffer = new byte[bufferSize];
            _bufferReadPointer = 0;
            _bufferedDataLength = 0;

            _position = stream.CanSeek ? stream.Position : 0;
            _length = stream.CanSeek ? stream.Length : long.MaxValue;
        }

        public BinaryStreamReader(ByteOrder byteOrder, byte[] data)
        {
            ByteOrder = byteOrder;

            _buffer = data;
            _bufferReadPointer = 0;
            _bufferedDataLength = data.Length;

            _position = 0;
            _length = data.Length;
        }

        public long Position
        {
            get { return _position; }
            private set
            {
                if (value > _length)
                {
                    throw new IOException($"Attempt to consume {value - _length} more bytes than available");
                }
                _position = value;
            }
        }

        public long BytesRemaining => _length - _position;

        public bool AtEnd => _position == _length;

        private void BeginReadFromBuffer(int count)
        {
            if (_bufferedDataLength < count)
            {
                if (_bufferReadPointer + count > _buffer.Length)
                {
                    _buffer.AsSpan(_bufferReadPointer, _bufferedDataLength).CopyTo(_buffer);
                    _bufferReadPointer = 0;
                }

                do
                {
                    var bytesRead = (Stream == null) ? 0 : Stream.Read(_buffer.AsSpan(_bufferReadPointer + _bufferedDataLength));
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
            Position++;
            BeginReadFromBuffer(1);
            var datum = _buffer[_bufferReadPointer];
            EndReadFromBuffer(1);
            return datum;
        }

        public sbyte ReadSByte() => (sbyte)ReadByte();

        private Span<byte> GetBufferReadSpan(int count)
        {
            BeginReadFromBuffer(count);
            var readSpan = _buffer.AsSpan(_bufferReadPointer, count);
            EndReadFromBuffer(count);
            return readSpan;
        }

        public T Read<T>() where T : unmanaged, IFormattable
        {
            Debug.Assert(BinaryIOUtils.IsByteSwappableType(typeof(T)));

            var datumSize = Unsafe.SizeOf<T>();

            Position += datumSize;

            var bufferReadSpan = GetBufferReadSpan(datumSize);

            if (ByteOrder != BinaryIOUtils.NativeByteOrder)
            {
                bufferReadSpan.Reverse();
            }

            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(bufferReadSpan));
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
            }

            Position += n;
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

        public void Read<T>(Span<T> data) where T : unmanaged, IFormattable
        {
            Debug.Assert(BinaryIOUtils.IsByteSwappableType(typeof(T)));

            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                ReadAll(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = Read<T>();
                }
            }
        }

        public void Read(long byteCount, Action readAction)
        {
            var newLength = Position + byteCount;
            if (newLength > _length)
            {
                throw new IOException($"Read {byteCount} bytes exceeds container by {newLength - _length} bytes");
            }
            var oldLength = _length;
            _length = newLength;
            readAction.Invoke();
            if (!AtEnd)
            {
                throw new Exception($"Read action left {BytesRemaining} of {byteCount} bytes unread");
            }
            _length = oldLength;
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
