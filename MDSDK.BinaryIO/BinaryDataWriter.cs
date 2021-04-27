// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace MDSDK.BinaryIO
{
    public sealed class BinaryDataWriter
    {
        public BufferedStreamWriter Output { get; }

        public ByteOrder ByteOrder { get; }
        
        public BinaryDataWriter(BufferedStreamWriter output, ByteOrder byteOrder)
        {
            Output = output;
            ByteOrder = byteOrder;
        }

        public void Write(byte datum) => Output.WriteByte(datum);
        
        public void Write(sbyte datum) => Output.WriteByte((byte)datum);

        public void Write<T>(T datum) where T : unmanaged, IFormattable
        {
            Debug.Assert(BinaryIOUtils.IsByteSwappableType(typeof(T)));

            var datumSize = Unsafe.SizeOf<T>();

            var bufferWriteSpan = Output.GetBufferWriteSpan(datumSize);

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bufferWriteSpan), datum);

            if (ByteOrder != BinaryIOUtils.NativeByteOrder)
            {
                bufferWriteSpan.Reverse();
            }

            Output.Position += datumSize;
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            Output.WriteBytes(data);
        }

        public void Write(ReadOnlySpan<sbyte> data)
        {
            Output.WriteBytes(MemoryMarshal.AsBytes(data));
        }

        public void Write<T>(ReadOnlySpan<T> data) where T : unmanaged, IFormattable
        {
            Debug.Assert(BinaryIOUtils.IsByteSwappableType(typeof(T)));

            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Output.WriteBytes(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Write(data[i]);
                }
            }
        }

        public void WriteZeros(int count) => Output.WriteZeros(count);
    }
}
