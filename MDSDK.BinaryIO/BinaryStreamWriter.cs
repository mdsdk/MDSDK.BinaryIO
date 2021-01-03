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

        private readonly Stream _stream;

        private readonly byte[] _buffer;

        private int _bufferWritePointer;

        private const int BufferSize = 4096;

        private const int MaxWriteToBufferByteCount = 1024;

        public BinaryStreamWriter(Stream stream, ByteOrder byteOrder)
        {
            _stream = stream;
            _byteOrderIsNative = byteOrder == BinaryIOUtils.NativeByteOrder;
            _buffer = new byte[BufferSize];
            _bufferWritePointer = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginWriteToBuffer(int byteCount)
        {
            Debug.Assert(byteCount <= MaxWriteToBufferByteCount);

            if (_bufferWritePointer + byteCount > BufferSize)
            {
                Flush(FlushMode.Shallow);
            }
        }

        private void EndWriteToBuffer(int byteCount)
        {
            _bufferWritePointer += byteCount;
        }

        public void Write(byte datum)
        {
            BeginWriteToBuffer(1);
            _buffer[_bufferWritePointer] = datum;
            EndWriteToBuffer(1);
        }

        private Span<byte> GetBufferWriteSpan(int count)
        {
            BeginWriteToBuffer(count);
            var bufferWriteSpan = _buffer.AsSpan(_bufferWritePointer, count);
            EndWriteToBuffer(count);
            return bufferWriteSpan;
        }

        private void WritePrimitive<T>(T datum) where T : struct
        {
            var bufferWriteSpan = GetBufferWriteSpan(Unsafe.SizeOf<T>());

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bufferWriteSpan), datum);

            if (!_byteOrderIsNative)
            {
                bufferWriteSpan.Reverse();
            }
        }

        public void Write(short datum) => WritePrimitive(datum);

        public void Write(ushort datum) => WritePrimitive(datum);

        public void Write(int datum) => WritePrimitive(datum);

        public void Write(uint datum) => WritePrimitive(datum);

        public void Write(long datum) => WritePrimitive(datum);

        public void Write(ulong datum) => WritePrimitive(datum);

        public void Write(float datum) => WritePrimitive(datum);

        public void Write(double datum) => WritePrimitive(datum);

        public void Write(ReadOnlySpan<byte> data)
        {
            if (data.Length <= MaxWriteToBufferByteCount)
            {
                var bufferWriteSpan = GetBufferWriteSpan(data.Length);
                data.CopyTo(bufferWriteSpan);
            }
            else
            {
                Flush(FlushMode.Shallow);
                _stream.Write(data);
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

        public void Write<T>(ReadOnlySpan<T> data) where T : struct
        {
            if (!_byteOrderIsNative)
            {
                throw new NotSupportedException("Non-primitive data types can only be written in native byte order");
            }

            Write(MemoryMarshal.AsBytes(data));
        }

        public void Write<T>(ref T datum) where T : struct
        {
            Write(MemoryMarshal.CreateReadOnlySpan(ref datum, 1));
        }

        public void Write<T>(T datum) where T : struct
        {
            Write(ref datum);
        }

        public void Flush(FlushMode mode)
        {
            if (_bufferWritePointer > 0)
            {
                _stream.Write(_buffer.AsSpan(0, _bufferWritePointer));
                _bufferWritePointer = 0;

                if (mode == FlushMode.Deep)
                {
                    _stream.Flush();
                }
            }
        }
    }
}
