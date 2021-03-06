﻿// Copyright (c) Johnny Z. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetUV.Core.Tests
{
    using System;
    using System.Net;
    using System.Text;
    using NetUV.Core.Buffers;
    using NetUV.Core.Handles;
    using NetUV.Core.Native;
    using Xunit;

    public sealed class UdpTrySendTests : IDisposable
    {
        const int Port = 8993;

        Loop loop;
        Udp client;
        int closeCount;
        int serverReceiveCount;

        [Fact]
        public void Run()
        {
            if (Platform.IsWindows)
            {
                // As of libuv 1.9.1 on Windows, udp_try_send is not yet implemented.
                return;
            }

            this.closeCount = 0;
            this.serverReceiveCount = 0;

            this.loop = new Loop();

            var anyEndPoint = new IPEndPoint(IPAddress.Any, Port);
            this.loop
                .CreateUdp()
                .ReceiveStart(anyEndPoint, this.OnServerReceive);

            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, Port);

            this.client = this.loop.CreateUdp();

            // Message too big
            var data = new byte[64 * 1024];
            var error = Assert.Throws<OperationException>(() => this.client.TrySend(remoteEndPoint, data));
            Assert.Equal((int)uv_err_code.UV_EMSGSIZE, error.ErrorCode);

            // Normal message
            data = Encoding.UTF8.GetBytes("EXIT");
            this.client.TrySend(remoteEndPoint, data);

            this.loop.RunDefault();

            Assert.Equal(2, this.closeCount);
            Assert.Equal(1, this.serverReceiveCount);
        }

        void OnServerReceive(Udp udp, IDatagramReadCompletion completion)
        {
            if (completion.Error == null)
            {
                return;
            }

            ReadableBuffer data = completion.Data;
            if (data.Count == 0)
            {
                return;
            }

            string message = data.ReadString(data.Count, Encoding.UTF8);
            if (message == "EXIT")
            {
                this.serverReceiveCount++;
            }

            udp.CloseHandle(this.OnClose);
            this.client?.CloseHandle(this.OnClose);
        }

        void OnClose(Udp handle)
        {
            handle.Dispose();
            this.closeCount++;
        }

        public void Dispose()
        {
            this.loop?.Dispose();
            this.loop = null;
        }
    }
}
