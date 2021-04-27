// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;

namespace MDSDK.BinaryIO.Test
{
    class FunctionalTest
    {
        private const byte TestByte = 123;
        private const short TestShort = -12345;
        private const ushort TestUShort = 54321;
        private const int TestInt = -123456789;
        private const uint TestUInt = 987654321;
        private const long TestLong = -12345678987654321;
        private const ulong TestULong = 98765432123456789;
        private const float TestFloat = -123456789876.54321f;
        private const double TestDouble = 98765.432123456789;

        private delegate void WriteSpan<T>(ReadOnlySpan<T> data);

        private static void TestWriteArray<T>(int n, T testValue, WriteSpan<T> writeSpan)
        {
            var array = new T[n];
            for (var i = 0; i < n; i++)
            {
                array[i] = testValue;
            }
            writeSpan(array);
        }

        private delegate void Read<T>(out T data);

        private static void TestRead<T>(Read<T> read, T testValue) where T : struct, IEquatable<T>
        {
            read(out T datum);
            Trace.Assert(datum.Equals(testValue));
        }

        private delegate void ReadSpan<T>(Span<T> data);

        private static void TestReadArray<T>(int n, ReadSpan<T> readSpan, T testValue) where T : struct, IEquatable<T>
        {
            var array = new T[n];
            readSpan(array);
            for (var i = 0; i < n; i++)
            {
                Trace.Assert(array[i].Equals(testValue));
            }
        }

        private static void TestWrite(Stream stream, ByteOrder byteOrder)
        {
            var output = new BufferedStreamWriter(stream);
            var dataWriter = new BinaryDataWriter(output, byteOrder);

            Trace.Assert(stream.Position == 0);

            dataWriter.Write(TestByte);
            output.Flush(FlushMode.Shallow);

            Trace.Assert(stream.Position == 1);

            dataWriter.Write(TestShort);
            dataWriter.Write(TestUShort);

            Trace.Assert(stream.Position == 1);

            output.Flush(FlushMode.Shallow);

            Trace.Assert(stream.Position == 5);

            dataWriter.Write(TestInt);
            dataWriter.Write(TestUInt);
            dataWriter.Write(TestLong);
            dataWriter.Write(TestULong);
            dataWriter.Write(TestFloat);
            dataWriter.Write(TestDouble);

            Trace.Assert(stream.Position == 5);

            output.Flush(FlushMode.Shallow);

            Trace.Assert(stream.Position == 41);

            TestWriteArray(9 * ushort.MaxValue, TestByte, dataWriter.Write);
            TestWriteArray(8 * ushort.MaxValue, TestShort, dataWriter.Write);
            TestWriteArray(7 * ushort.MaxValue, TestUShort, dataWriter.Write);
            TestWriteArray(6 * ushort.MaxValue, TestInt, dataWriter.Write);
            TestWriteArray(5 * ushort.MaxValue, TestUInt, dataWriter.Write);
            TestWriteArray(4 * ushort.MaxValue, TestLong, dataWriter.Write);
            TestWriteArray(3 * ushort.MaxValue, TestULong, dataWriter.Write);
            TestWriteArray(2 * ushort.MaxValue, TestFloat, dataWriter.Write);
            TestWriteArray(1 * ushort.MaxValue, TestDouble, dataWriter.Write);

            output.Flush(FlushMode.Deep);
        }

        private static void TestRead(Stream stream, ByteOrder byteOrder)
        {
            var input = new BufferedStreamReader(stream);
            var dataReader = new BinaryDataReader(input, byteOrder);

            TestRead(dataReader.Read, TestByte);
            TestRead(dataReader.Read, TestShort);
            TestRead(dataReader.Read, TestUShort);
            TestRead(dataReader.Read, TestInt);
            TestRead(dataReader.Read, TestUInt);
            TestRead(dataReader.Read, TestLong);
            TestRead(dataReader.Read, TestULong);
            TestRead(dataReader.Read, TestFloat);
            TestRead(dataReader.Read, TestDouble);

            TestReadArray(9 * ushort.MaxValue, dataReader.Read, TestByte);
            TestReadArray(8 * ushort.MaxValue, dataReader.Read, TestShort);
            TestReadArray(7 * ushort.MaxValue, dataReader.Read, TestUShort);
            TestReadArray(6 * ushort.MaxValue, dataReader.Read, TestInt);
            TestReadArray(5 * ushort.MaxValue, dataReader.Read, TestUInt);
            TestReadArray(4 * ushort.MaxValue, dataReader.Read, TestLong);
            TestReadArray(3 * ushort.MaxValue, dataReader.Read, TestULong);
            TestReadArray(2 * ushort.MaxValue, dataReader.Read, TestFloat);
            TestReadArray(1 * ushort.MaxValue, dataReader.Read, TestDouble);
        }

        private static void Test(ByteOrder byteOrder)
        {
            Console.WriteLine("Testing " + byteOrder);
            using var stream = new MemoryStream();

            TestWrite(stream, byteOrder);
            Console.WriteLine("    Write OK");

            stream.Seek(0, SeekOrigin.Begin);
            TestRead(stream, byteOrder);
            Console.WriteLine("    Read OK");
        }

        public static void Run()
        {
            Test(ByteOrder.LittleEndian);
            Test(ByteOrder.BigEndian);
        }
    }
}
