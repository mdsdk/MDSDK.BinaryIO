// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.BinaryIO
{
    public static class BinaryIOUtils
    {
        public static readonly ByteOrder NativeByteOrder = BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
        
        public static bool IsByteSwappableType(Type type)
        {
            return (type == typeof(short))
                || (type == typeof(ushort))
                || (type == typeof(int))
                || (type == typeof(uint))
                || (type == typeof(long))
                || (type == typeof(ulong))
                || (type == typeof(float))
                || (type == typeof(double));
        }
    }
}
