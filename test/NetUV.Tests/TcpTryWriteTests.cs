﻿// Copyright (c) Johnny Z. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetUV.Core.Tests
{
    using System;
    using System.Net;
    using System.Text;
    using NetUV.Core.Handles;
    using NetUV.Core.Native;
    using Xunit;

    public sealed class TcpTryWriteTests : IDisposable
    {
        const int Port = 9884;
        Loop loop;
        StreamListener<Tcp> server;
        int connectionCount;
        int connectedCount;
        int closeCount;
        int bytesRead;
        int bytesWritten;

        [Fact]
        public void Run()
        {
            this.loop = new Loop();

            this.StartServer();

            var endPoint = new IPEndPoint(IPAddress.Loopback, Port);
            this.loop.CreateTcp().ConnectTo(endPoint, this.OnConnected);

            this.loop.RunDefault();

            Assert.Equal(1, this.connectedCount);
            Assert.Equal(1, this.connectionCount);
            Assert.Equal(3, this.closeCount);
            Assert.True(this.bytesWritten > 0);
            Assert.True(this.bytesRead == this.bytesWritten);
        }

        void OnConnected(Tcp tcp, Exception exception)
        {
            if (exception != null)
            {
                tcp.CloseHandle(this.OnClose);
                this.server.Close(this.OnClose);

                return;
            }

            this.connectedCount++;

            // Send PING
            byte[] content = Encoding.UTF8.GetBytes("PING");
            do
            {
                try
                {
                    tcp.TryWrite(content);
                    this.bytesWritten += content.Length;
                    break; // Try write success
                }
                catch (OperationException error)
                {
                    if (error.ErrorCode != (int)uv_err_code.UV_EAGAIN)
                    {
                        this.bytesWritten = 0;
                        break;
                    }
                }
            }
            while (true);

            // Send Empty
            content = Encoding.UTF8.GetBytes("");
            do
            {
                try
                {
                    tcp.TryWrite(content);
                    break; // Try write success
                }
                catch (OperationException error)
                {
                    if (error.ErrorCode != (int)uv_err_code.UV_EAGAIN)
                    {
                        this.bytesWritten = 0;
                        break;
                    }
                }
            }
            while (true);

            tcp.CloseHandle(this.OnClose);
        }

        void StartServer()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, Port);
            this.server = this.loop
                .CreateTcp()
                .Listen(endPoint, this.OnConnection);
        }

        void OnConnection(Tcp tcp, Exception exception)
        {
            if (exception == null)
            {
                this.connectionCount++;
                tcp.RegisterRead(this.OnRead);
            }
            else
            {
                tcp.CloseHandle(this.OnClose);
            }
        }

        void OnRead(Tcp tcp, IStreamReadCompletion completion)
        {
            if (completion.Error != null 
                || completion.Completed)
            {
                tcp.CloseHandle(this.OnClose);
                this.server.Close(this.OnClose);
            }
            else
            {
                this.bytesRead += completion.Data.Count;
            }
        }

        void OnClose(Tcp handle)
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
