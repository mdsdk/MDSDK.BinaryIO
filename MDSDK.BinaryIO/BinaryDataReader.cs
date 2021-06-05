// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace MDSDK.BinaryIO
{
    public sealed class BinaryDataReader
    {
        public BufferedStreamReader Input { get; }

        public ByteOrder ByteOrder { get; }
        
        public BinaryDataReader(BufferedStreamReader input, ByteOrder byteOrder)
        {
            Input = input;
            ByteOrder = byteOrder;
        }

        public byte ReadByte() => Input.ReadByte();

        public sbyte ReadSByte() => (sbyte)Input.ReadByte();

        public T Read<T>() where T : unmanaged, IFormattable
        {
            Debug.Assert(BinaryIOUtils.IsByteSwappableType(typeof(T)));

            var datumSize = Unsafe.SizeOf<T>();

            var bufferReadSpan = Input.GetBufferReadSpan(datumSize);

            if (ByteOrder != BinaryIOUtils.NativeByteOrder)
            {
                bufferReadSpan.Reverse();
            }

            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(bufferReadSpan));
        }

        public long GetRemainingDatumCount(int datumSize)
        {
            if ((Input.BytesRemaining % datumSize) != 0)
            {
                throw new InvalidOperationException($"Bytes remaining {Input.BytesRemaining} is not a multiple of {datumSize}");
            }
            return Input.BytesRemaining / datumSize;
        }

        public long GetRemainingDatumCount<T>() where T : unmanaged, IFormattable 
        {
            var datumSize = Unsafe.SizeOf<T>();
            return GetRemainingDatumCount(datumSize);
        }

        public void Read(out byte datum) => datum = ReadByte();

        public void Read(out sbyte datum) => datum = ReadSByte();

        public void Read<T>(out T datum) where T : unmanaged, IFormattable => datum = Read<T>();

        public void Read(Span<byte> data)
        {
            Input.ReadAll(data);
        }

        public void Read(Span<sbyte> data)
        {
            Input.ReadAll(MemoryMarshal.AsBytes(data));
        }

        public void Read<T>(Span<T> data) where T : unmanaged, IFormattable
        {
            Debug.Assert(BinaryIOUtils.IsByteSwappableType(typeof(T)));

            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Input.ReadAll(MemoryMarshal.AsBytes(data));
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
