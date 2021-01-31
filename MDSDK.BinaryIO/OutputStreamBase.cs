// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.IO;

namespace MDSDK.BinaryIO
{
    public abstract class OutputStreamBase : Stream
    {
        #region Derived class must provide Write functionality

        public sealed override bool CanWrite => true;

        public abstract override void Write(ReadOnlySpan<byte> buffer);

        public abstract override void Flush();

        public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        #endregion

        #region Derived class must not attempt to provide Read functionality

        public sealed override bool CanRead => false;

        public sealed override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public sealed override int Read(Span<byte> buffer) => throw new NotSupportedException();

        #endregion

        #region Derived class may provide Seek functionality

        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        #endregion
    }
}
