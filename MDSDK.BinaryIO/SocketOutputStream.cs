// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Net.Sockets;
using System.Threading;

namespace MDSDK.BinaryIO
{
    public sealed class SocketOutputStream : OutputStreamBase
    {
        private readonly Socket _socket;

        private readonly CancellationToken _cancellationToken;

        public int CancellationPollIntervalInMicroseconds { get; set; } = 1000;

        public SocketOutputStream(Socket socket, CancellationToken cancellationToken)
        {
            _socket = socket;
            _cancellationToken = cancellationToken;
        }
        
        private void CancellableWaitUntilSocketIsWritable()
        {
            while (!_socket.Poll(CancellationPollIntervalInMicroseconds, SelectMode.SelectWrite))
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override void Write(ReadOnlySpan<byte> data)
        {
            while (data.Length > 0)
            {
                CancellableWaitUntilSocketIsWritable();
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
