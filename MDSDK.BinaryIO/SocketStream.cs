// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Net.Sockets;
using System.Threading;

namespace MDSDK.BinaryIO
{
    public class SocketStream : StreamBase
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
            while (!_socket.Poll(CancellationPollIntervalInMicroseconds, selectMode))
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override int Read(Span<byte> buffer)
        {
            CancellableWaitFor(SelectMode.SelectRead);
            return _socket.Receive(buffer);
        }

        public override void Write(ReadOnlySpan<byte> data)
        {
            while (data.Length > 0)
            {
                CancellableWaitFor(SelectMode.SelectWrite);
                var bytesSent = _socket.Send(data);
                data = data.Slice(bytesSent);
            }
        }

        public override void Flush()
        {
            // Nothing to do
        }
    }
}
