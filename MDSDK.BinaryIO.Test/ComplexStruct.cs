// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.BinaryIO.Test
{
    public struct ComplexStruct : IEquatable<ComplexStruct>
    {
        public DateTime DateTime;
        public PrimitiveStruct PrimitiveStruct1;
        public TimeSpan TimeSpan;
        public PrimitiveStruct PrimitiveStruct2;

        public bool Equals(ComplexStruct other)
        {
            return base.Equals(other);
        }
    }
}
