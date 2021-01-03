// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.BinaryIO.Test
{
    class Program
    {
        public static void Main()
        {
            FunctionalTest.Run();

#if DEBUG
            Console.WriteLine("Performance tests skipped (debug build)");
#else
            MemoryMarshalPerformanceTest.Run();
#endif
        }
    }
}
