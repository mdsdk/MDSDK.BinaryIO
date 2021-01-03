// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.PerformanceTesting;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MDSDK.BinaryIO.Test
{
    public static class MemoryMarshalPerformanceTest
    {
        private static readonly byte[] RandomBytes = PerformanceTestDataGenerator.MakeRandomByteArray(Marshal.SizeOf<PrimitiveStruct>());

        const int TestReadIterations = 100000000;
     
        private static PrimitiveStruct dummy;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T ReadUnaligned<T>(ReadOnlySpan<byte> source)
        {
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(source));
        }

        private static void TestReadPrimitives_ReadUnaligned()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct.Byte1 = ReadUnaligned<byte>(testInput);
                testStruct.Byte2 = ReadUnaligned<byte>(testInput);
                testStruct.Short = ReadUnaligned<short>(testInput);
                testStruct.UShort1 = ReadUnaligned<ushort>(testInput);
                testStruct.UShort2 = ReadUnaligned<ushort>(testInput);
                testStruct.Int = ReadUnaligned<int>(testInput);
                testStruct.UInt = ReadUnaligned<uint>(testInput);
                testStruct.Long = ReadUnaligned<long>(testInput);
                testStruct.ULong = ReadUnaligned<ulong>(testInput);
                testStruct.Float1 = ReadUnaligned<float>(testInput);
                testStruct.Float2 = ReadUnaligned<float>(testInput);
                testStruct.Double = ReadUnaligned<double>(testInput);
            }

            dummy = testStruct;
        }

        private static void TestReadStruct_ReadUnaligned()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct = ReadUnaligned<PrimitiveStruct>(testInput);
            }

            dummy = testStruct;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T ReadUnalignedWithSizeCheck<T>(ReadOnlySpan<byte> source)
        {
            if (Unsafe.SizeOf<T>() > source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(source));
        }

        private static void TestReadPrimitives_ReadUnalignedWithSizeCheck()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct.Byte1 = ReadUnalignedWithSizeCheck<byte>(testInput);
                testStruct.Byte2 = ReadUnalignedWithSizeCheck<byte>(testInput);
                testStruct.Short = ReadUnalignedWithSizeCheck<short>(testInput);
                testStruct.UShort1 = ReadUnalignedWithSizeCheck<ushort>(testInput);
                testStruct.UShort2 = ReadUnalignedWithSizeCheck<ushort>(testInput);
                testStruct.Int = ReadUnalignedWithSizeCheck<int>(testInput);
                testStruct.UInt = ReadUnalignedWithSizeCheck<uint>(testInput);
                testStruct.Long = ReadUnalignedWithSizeCheck<long>(testInput);
                testStruct.ULong = ReadUnalignedWithSizeCheck<ulong>(testInput);
                testStruct.Float1 = ReadUnalignedWithSizeCheck<float>(testInput);
                testStruct.Float2 = ReadUnalignedWithSizeCheck<float>(testInput);
                testStruct.Double = ReadUnalignedWithSizeCheck<double>(testInput);
            }

            dummy = testStruct;
        }

        private static void TestReadStruct_ReadUnalignedWithSizeCheck()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct = ReadUnalignedWithSizeCheck<PrimitiveStruct>(testInput);
            }

            dummy = testStruct;
        }

        private static void TestReadPrimitives_MemoryMarshalRead()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct.Byte1 = MemoryMarshal.Read<byte>(testInput);
                testStruct.Byte2 = MemoryMarshal.Read<byte>(testInput);
                testStruct.Short = MemoryMarshal.Read<short>(testInput);
                testStruct.UShort1 = MemoryMarshal.Read<ushort>(testInput);
                testStruct.UShort2 = MemoryMarshal.Read<ushort>(testInput);
                testStruct.Int = MemoryMarshal.Read<int>(testInput);
                testStruct.UInt = MemoryMarshal.Read<uint>(testInput);
                testStruct.Long = MemoryMarshal.Read<long>(testInput);
                testStruct.ULong = MemoryMarshal.Read<ulong>(testInput);
                testStruct.Float1 = MemoryMarshal.Read<float>(testInput);
                testStruct.Float2 = MemoryMarshal.Read<float>(testInput);
                testStruct.Double = MemoryMarshal.Read<double>(testInput);
            }

            dummy = testStruct;
        }

        private static void TestReadStruct_MemoryMarshalRead()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct = MemoryMarshal.Read<PrimitiveStruct>(testInput);
            }

            dummy = testStruct;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MemoryMarshalRead_WithoutSizeCheck<T>(ReadOnlySpan<byte> source) where T : struct
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                throw new ArgumentException($"InvalidTypeWithPointersNotSupported");
            }
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(source));
        }

        private static void TestReadPrimitives_MemoryMarshalRead_WithoutSizeCheck()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct.Byte1 = MemoryMarshalRead_WithoutSizeCheck<byte>(testInput);
                testStruct.Byte2 = MemoryMarshalRead_WithoutSizeCheck<byte>(testInput);
                testStruct.Short = MemoryMarshalRead_WithoutSizeCheck<short>(testInput);
                testStruct.UShort1 = MemoryMarshalRead_WithoutSizeCheck<ushort>(testInput);
                testStruct.UShort2 = MemoryMarshalRead_WithoutSizeCheck<ushort>(testInput);
                testStruct.Int = MemoryMarshalRead_WithoutSizeCheck<int>(testInput);
                testStruct.UInt = MemoryMarshalRead_WithoutSizeCheck<uint>(testInput);
                testStruct.Long = MemoryMarshalRead_WithoutSizeCheck<long>(testInput);
                testStruct.ULong = MemoryMarshalRead_WithoutSizeCheck<ulong>(testInput);
                testStruct.Float1 = MemoryMarshalRead_WithoutSizeCheck<float>(testInput);
                testStruct.Float2 = MemoryMarshalRead_WithoutSizeCheck<float>(testInput);
                testStruct.Double = MemoryMarshalRead_WithoutSizeCheck<double>(testInput);
            }

            dummy = testStruct;
        }

        private static void TestReadStruct_MemoryMarshalRead_WithoutSizeCheck()
        {
            var testInput = RandomBytes.AsSpan();
            var testStruct = new PrimitiveStruct();

            for (var i = 0; i < TestReadIterations; i++)
            {
                testStruct = MemoryMarshalRead_WithoutSizeCheck<PrimitiveStruct>(testInput);
            }

            dummy = testStruct;
        }

        private static void TestReadPrimitives()
        {
            var test = new PerformanceComparisonTest(5, 2);
            test.AddTest(nameof(TestReadPrimitives_ReadUnaligned), TestReadPrimitives_ReadUnaligned);
            test.AddTest(nameof(TestReadPrimitives_ReadUnalignedWithSizeCheck), TestReadPrimitives_ReadUnalignedWithSizeCheck);
            test.AddTest(nameof(TestReadPrimitives_MemoryMarshalRead), TestReadPrimitives_MemoryMarshalRead);
            test.AddTest(nameof(TestReadPrimitives_MemoryMarshalRead_WithoutSizeCheck), TestReadPrimitives_MemoryMarshalRead_WithoutSizeCheck);
            test.Run();
        }

        private static void TestReadStruct()
        {
            var test = new PerformanceComparisonTest(5, 2);
            test.AddTest(nameof(TestReadStruct_ReadUnaligned), TestReadStruct_ReadUnaligned);
            test.AddTest(nameof(TestReadStruct_ReadUnalignedWithSizeCheck), TestReadStruct_ReadUnalignedWithSizeCheck);
            test.AddTest(nameof(TestReadStruct_MemoryMarshalRead), TestReadStruct_MemoryMarshalRead);
            test.AddTest(nameof(TestReadStruct_MemoryMarshalRead_WithoutSizeCheck), TestReadStruct_MemoryMarshalRead_WithoutSizeCheck);
            test.Run();
        }

        public static void Run()
        {
            TestReadPrimitives();
            TestReadStruct();
        }
    }
}
