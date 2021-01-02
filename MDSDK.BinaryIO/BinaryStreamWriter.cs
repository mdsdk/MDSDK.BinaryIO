// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MDSDK.BinaryIO
{
    public sealed class BinaryStreamWriter
    {
        private readonly bool _byteOrderIsNative;

        private readonly Stream _sink;

        private readonly byte[] _buffer;

        private const int BufferSize = 4096;

        private const int MaxBufferWriteLength = BufferSize / 4;

        public BinaryStreamWriter(ByteOrder byteOrder, Stream sink)
        {
            _byteOrderIsNative = byteOrder == BinaryIOUtils.NativeByteOrder;
            _sink = sink;
            _buffer = new byte[BufferSize];
        }

        private int _bufferedDataLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSpaceInWriteBuffer(int count)
        {
            Debug.Assert(count <= MaxBufferWriteLength);
            
            var bytesAvailable = BufferSize - _bufferedDataLength;
            if (bytesAvailable < count)
            {
                Flush(FlushMode.Shallow);
            }
        }

        private void UpdateBytesWrittenToBuffer(int count)
        {
            _bufferedDataLength += count;
        }

        public void Write(byte datum)
        {
            EnsureSpaceInWriteBuffer(1);
            _buffer[_bufferedDataLength] = datum;
            UpdateBytesWrittenToBuffer(1);
        }

        private Span<byte> GetBufferWriteSpan(int count)
        {
            EnsureSpaceInWriteBuffer(count);
            var bufferWriteSpan = _buffer.AsSpan(_bufferedDataLength, count);
            UpdateBytesWrittenToBuffer(count);
            return bufferWriteSpan;
        }

        private void WriteBuffered<T>(T datum)
        {
            var bufferWriteSpan = GetBufferWriteSpan(Unsafe.SizeOf<T>());

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bufferWriteSpan), datum);

            if (!_byteOrderIsNative)
            {
                bufferWriteSpan.Reverse();
            }
        }

        public void Write(short datum) => WriteBuffered(datum);
        
        public void Write(ushort datum) => WriteBuffered(datum);
        
        public void Write(int datum) => WriteBuffered(datum);
        
        public void Write(uint datum) => WriteBuffered(datum);
        
        public void Write(long datum) => WriteBuffered(datum);
        
        public void Write(ulong datum) => WriteBuffered(datum);
        
        public void Write(float datum) => WriteBuffered(datum);
       
        public void Write(double datum) => WriteBuffered(datum);

        public void Write(ReadOnlySpan<byte> data)
        {
            var bytesToWriteToBuffer = Math.Min(BufferSize - _bufferedDataLength, data.Length);
            if (bytesToWriteToBuffer > 0)
            {
                var bufferWriteSpan = GetBufferWriteSpan(bytesToWriteToBuffer);
                data.CopyTo(bufferWriteSpan);
                data = data[bytesToWriteToBuffer..];
            }

            Flush(FlushMode.Shallow);

            while (data.Length > MaxBufferWriteLength)
            {
                _sink.Write(data);
                data = data.Slice(MaxBufferWriteLength);
            }

            if (data.Length > 0)
            {
                var bufferWriteSpan = GetBufferWriteSpan(data.Length);
                data.CopyTo(bufferWriteSpan);
            }
        }

        public void Write(ReadOnlySpan<short> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<ushort> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<int> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<uint> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<long> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<ulong> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<float> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Write(ReadOnlySpan<double> data)
        {
            if (_byteOrderIsNative)
            {
                Write(MemoryMarshal.AsBytes(data));
            }
            else
            {
                foreach (var datum in data)
                {
                    Write(datum);
                }
            }
        }

        public void Flush(FlushMode mode)
        {
            if (_bufferedDataLength > 0)
            {
                _sink.Write(_buffer.AsSpan(0, _bufferedDataLength));
                _bufferedDataLength = 0;

                if (mode == FlushMode.Deep)
                {
                    _sink.Flush();
                }
            }
        }
    }
}
