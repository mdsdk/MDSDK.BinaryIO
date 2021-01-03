// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.PerformanceTesting
{
    public static class PerformanceTestDataGenerator
    {
        private static readonly Random s_random = new Random();

        public static byte[] MakeRandomByteArray(int length)
        {
            var bytes = new byte[length];
            s_random.NextBytes(bytes);
            return bytes;
        }
    }
}
