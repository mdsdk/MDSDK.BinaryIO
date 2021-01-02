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
        private readonly bool _byteOrderIsNative;

        private readonly Stream _source;

        private readonly byte[] _buffer;

        private const int BufferSize = 4096;

        private const int MaxBufferReadLength = BufferSize / 4;

        public BinaryStreamReader(ByteOrder byteOrder, Stream source)
        {
            _byteOrderIsNative = byteOrder == BinaryIOUtils.NativeByteOrder;
            _source = source;
            _buffer = new byte[4096];
        }

        private int _bufferedDataPointer;

        private int _bufferedDataLength;

        private void EnsureDataInReadBuffer(int count)
        {
            Debug.Assert(count <= MaxBufferReadLength);

            if (_bufferedDataLength < count)
            {
                if (_bufferedDataPointer + count > BufferSize)
                {
                    _buffer.AsSpan(_bufferedDataPointer, _bufferedDataLength).CopyTo(_buffer);
                    _bufferedDataPointer = 0;
                }

                do
                {
                    var bytesRead = _source.Read(_buffer.AsSpan(_bufferedDataPointer + _bufferedDataLength));
                    if (bytesRead == 0)
                    {
                        throw new IOException("Unexpected end of stream");
                    }
                    _bufferedDataLength += bytesRead;
                }
                while (_bufferedDataLength < count);
            }
        }

        private void UpdateBytesReadFromBuffer(int count)
        {
            _bufferedDataLength -= count;
            _bufferedDataPointer = (_bufferedDataLength == 0) ? 0 : _bufferedDataPointer + count;
        }

        public void Read(out byte datum)
        {
            EnsureDataInReadBuffer(1);
            datum = _buffer[_bufferedDataPointer];
            UpdateBytesReadFromBuffer(1);
        }

        private Span<byte> GetBufferReadSpan(int count)
        {
            EnsureDataInReadBuffer(count);
            var readSpan = _buffer.AsSpan(_bufferedDataPointer, count);
            UpdateBytesReadFromBuffer(count);
            return readSpan;
        }

        private void ReadBuffered<T>(out T datum)
        {
            var bufferReadSpan = GetBufferReadSpan(Unsafe.SizeOf<T>());

            if (!_byteOrderIsNative)
            {
                bufferReadSpan.Reverse();
            }

            datum = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(bufferReadSpan));
        }

        public void Read(out short datum) => ReadBuffered(out datum);

        public void Read(out ushort datum) => ReadBuffered(out datum);

        public void Read(out int datum) => ReadBuffered(out datum);

        public void Read(out uint datum) => ReadBuffered(out datum);

        public void Read(out long datum) => ReadBuffered(out datum);

        public void Read(out ulong datum) => ReadBuffered(out datum);

        public void Read(out float datum) => ReadBuffered(out datum);

        public void Read(out double datum) => ReadBuffered(out datum);

        public void Read(Span<byte> data)
        {
            var bytesToReadFromBuffer = Math.Min(_bufferedDataLength, data.Length);
            if (bytesToReadFromBuffer > 0)
            {
                var bufferReadSpan = GetBufferReadSpan(bytesToReadFromBuffer);
                bufferReadSpan.CopyTo(data);
                data = data[bytesToReadFromBuffer..];
            }

            while (data.Length > MaxBufferReadLength)
            {
                var bytesRead = _source.Read(data);
                if (bytesRead == 0)
                {
                    throw new IOException("Unexpected end of stream");
                }
                data = data.Slice(bytesRead);
            }

            if (data.Length > 0)
            {
                var bufferReadSpan = GetBufferReadSpan(data.Length);
                bufferReadSpan.CopyTo(data);
            }
        }
        public void Read(Span<short> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<ushort> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<int> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<uint> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<long> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<ulong> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<float> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<double> data)
        {
            if (_byteOrderIsNative)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }
    }
}
