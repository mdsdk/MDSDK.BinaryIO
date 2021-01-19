// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MDSDK.BinaryIO
{
    public class SocketStream : Stream
    {
        private readonly Socket _socket;

        private readonly CancellationToken _cancellationToken;

        public int CancellationPollIntervalInMicroseconds { get; set; } = 1000;

        public SocketStream(Socket socket, CancellationToken cancellationToken)
        {
            _socket = socket;
            _cancellationToken = cancellationToken;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        private void CancellableWaitFor(SelectMode selectMode)
        {
            while (!_socket.Poll(CancellationPollIntervalInMicroseconds, SelectMode.SelectRead))
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override int Read(Span<byte> buffer)
        {
            CancellableWaitFor(SelectMode.SelectRead);
            return _socket.Receive(buffer);
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> data)
        {
            while (data.Length > 0)
            {
                CancellableWaitFor(SelectMode.SelectWrite);
                var bytesSent = _socket.Send(data);
                data = data.Slice(bytesSent);
            }
        }

        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        public override void Flush()
        {
            // Nothing to do
        }

        #region Unsupported part of the Stream interface

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
