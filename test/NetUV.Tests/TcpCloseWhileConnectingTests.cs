﻿// Copyright (c) Johnny Z. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetUV.Core.Tests
{
    using System;
    using System.Net;
    using NetUV.Core.Handles;
    using NetUV.Core.Native;
    using Xunit;

    public sealed class TcpCloseWhileConnectingTests : IDisposable
    {
        const int Port = 9885;

        Loop loop;
        int connectCount;
        int closeCount;
        Tcp tcpClient;
        Timer timer1;
        Timer timer2;

        bool timer1Callback;
        bool timer2Callback;

        public TcpCloseWhileConnectingTests()
        {
            this.loop = new Loop();
            this.connectCount = 0;
            this.closeCount = 0;
            this.timer1Callback = false;
            this.timer1Callback = false;
        }

        [Fact]
        public void Run()
        {
            IPAddress ipAddress = IPAddress.Parse("1.2.3.4");
            var endPoint = new IPEndPoint(ipAddress, Port);

            try
            {
                this.tcpClient = this.loop.CreateTcp()
                    .ConnectTo(endPoint, this.OnConnected);
            }
            catch (OperationException exception)
            {
                // Skip
                if (exception.ErrorCode == (int)uv_err_code.UV_ENETUNREACH)
                {
                    return;
                }
            }

            this.timer1 = this.loop.CreateTimer();
            this.timer1.Start(this.OnTimer1, 1, 0);

            this.timer2 = this.loop.CreateTimer();
            this.timer2.Start(this.OnTimer2, 86400 * 1000, 0);

            this.loop.RunDefault();
            Assert.True(this.timer1Callback);
            Assert.False(this.timer2Callback);
            Assert.Equal(2, this.closeCount);
            Assert.Equal(1, this.connectCount);
        }

        void OnTimer1(Timer timer)
        {
            Assert.Same(this.timer1, timer);
            timer.CloseHandle(this.OnClose);
            this.tcpClient.CloseHandle(this.OnClose);
            this.timer1Callback = true;
        }

        void OnTimer2(Timer timer) => 
            this.timer2Callback = true;

        void OnConnected(Tcp tcp, Exception exception)
        {
            Assert.IsType<OperationException>(exception);

            var operationException = (OperationException)exception;
            Assert.Equal((int)uv_err_code.UV_ECANCELED, operationException.ErrorCode);

            this.timer2.Stop();
            this.connectCount++;
        }

        void OnClose(Timer timer)
        {
            timer.Dispose();
            this.closeCount++;
        }

        void OnClose(Tcp tcp)
        {
            tcp.Dispose();
            this.closeCount++;
        }

        public void Dispose()
        {
            this.timer1.Dispose();
            this.timer1 = null;

            this.timer2.Dispose();
            this.timer2 = null;

            this.tcpClient.Dispose();
            this.tcpClient = null;

            this.loop?.Dispose();
            this.loop = null;
        }
    }
}
