// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MDSDK.BinaryIO
{
    public static class SocketExtensionMethods
    {
        public static void Connect(this Socket socket, string hostNameOrAddress, int port, CancellationToken cancellationToken)
        {
            var ipAddresses = Dns.GetHostAddresses(hostNameOrAddress);
            if (ipAddresses.Length == 0)
            {
                throw new ArgumentException($"Failed to resolve '{hostNameOrAddress}'", nameof(hostNameOrAddress));
            }
            socket.ConnectAsync(ipAddresses, port, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
