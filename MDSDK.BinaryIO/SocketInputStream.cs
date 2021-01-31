// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MDSDK.BinaryIO
{
    public sealed class SocketInputStream : InputStreamBase
    {
        private readonly Socket _socket;

        private readonly CancellationToken _cancellationToken;

        public int CancellationPollIntervalInMicroseconds { get; set; } = 1000;

        public SocketInputStream(Socket socket, CancellationToken cancellationToken)
        {
            _socket = socket;
            _cancellationToken = cancellationToken;
        }
        
        private void CancellableWaitUntilSocketIsReadable()
        {
            while (!_socket.Poll(CancellationPollIntervalInMicroseconds, SelectMode.SelectRead))
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override int Read(Span<byte> buffer)
        {
            CancellableWaitUntilSocketIsReadable();
            return _socket.Receive(buffer);
        }
    }
}
