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

    public sealed class RefTests : IDisposable
    {
        const int Port = 9887;

        Loop loop;
        int closeCount;
        int callbackCount;

        public RefTests()
        {
            this.loop = new Loop();
        }

        [Fact]
        public void Empty()
        {
            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
        }

        [Fact]
        public void HasRef()
        {
            Idle idle = this.loop.CreateIdle();
            idle.AddReference();
            Assert.True(idle.HasReference());
            idle.RemoveReference();
            Assert.False(idle.HasReference());
            idle.CloseHandle(this.OnClose);
        }

        [Fact]
        public void Idle()
        {
            Idle idle = this.loop.CreateIdle().Start(this.OnCallback);
            idle.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(idle);
        }

        [Fact]
        public void Async()
        {
            Async async = this.loop.CreateAsync(this.OnCallback);
            async.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(async);
        }

        [Fact]
        public void Prepare()
        {
            Prepare prepare = this.loop.CreatePrepare().Start(this.OnCallback);
            prepare.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(prepare);
        }

        [Fact]
        public void PrepareCallback()
        {
            Prepare prepare = this.loop
                .CreatePrepare()
                .Start(this.OnPrepare);

            this.loop.RunDefault();
            Assert.Equal(1, this.callbackCount);

            this.CloseHandle(prepare);
        }

        void OnPrepare(Prepare handle)
        {
            handle.RemoveReference();
            this.callbackCount++;
        }

        [Fact]
        public void Check()
        {
            Check check = this.loop
                .CreateCheck()
                .Start(this.OnCallback);
            check.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(check);
        }

        [Fact]
        public void Timer()
        {
            Timer timer = this.loop.CreateTimer();
            timer.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(timer);
        }

        [Fact]
        public void Timer2()
        {
            Timer timer = this.loop.CreateTimer().Start(this.OnCallback, 42, 42);
            timer.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(timer);
        }

        [Fact]
        public void FSEvent()
        {
            FSEvent fsEvent = this.loop
                .CreateFSEvent()
                .Start(".", FSEventMask.None, this.OnFSEvent);
            fsEvent.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(fsEvent);
        }

        void OnFSEvent(FSEvent fsEvent, FileSystemEvent fileSystemEvent) => this.callbackCount++;

        [Fact]
        public void FSPoll()
        {
            FSPoll fsPoll = this.loop
                .CreateFSPoll()
                .Start(".", 999, this.OnFSPoll);
            fsPoll.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(fsPoll);
        }

        void OnFSPoll(FSPoll fsPoll, FSPollStatus fsPollStatus) => this.callbackCount++;

        [Fact]
        public void Tcp()
        {
            Tcp tcp = this.loop.CreateTcp();
            tcp.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(tcp);
        }

        [Fact]
        public void TcpListen()
        {
            Tcp tcp = this.loop.CreateTcp();
            var anyEndPoint = new IPEndPoint(IPAddress.Loopback, IPEndPoint.MinPort);
            StreamListener<Tcp> listener = tcp.Listen(anyEndPoint, this.OnConnection);

            tcp.RemoveReference();
            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            listener.Close(this.OnClose);
        }

        [Fact]
        public void TcpListen2()
        {
            Tcp tcp = this.loop.CreateTcp();
            var anyEndPoint = new IPEndPoint(IPAddress.Loopback, IPEndPoint.MinPort);
            StreamListener<Tcp> listener = tcp.Listen(anyEndPoint, this.OnConnection);
            tcp.RemoveReference();
            tcp.CloseHandle(this.OnClose);

            this.loop.RunDefault();
            Assert.Equal(1, this.closeCount);

            listener.Close(this.OnClose);
        }

        void OnConnection(StreamHandle stream, Exception exception) => this.callbackCount++;

        [Fact]
        public void TcpConnectNoServer()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, Port);
            Tcp tcp = this.loop
                .CreateTcp()
                .ConnectTo(endPoint, this.OnConnectedAndShutdown);
            tcp.RemoveReference();

            this.loop.RunDefault();
            Assert.Equal(2, this.callbackCount);

            this.CloseHandle(tcp);
        }

        void OnConnectedAndShutdown(StreamHandle stream, Exception exception)
        {
            stream.ShutdownStream(this.OnShutdown);
            this.callbackCount++;
        }

        void OnShutdown(StreamHandle handle, Exception exception) => this.callbackCount++;

        [Fact]
        public void TcpConnect2NoServer()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, Port);
            Tcp tcp = this.loop
                .CreateTcp()
                .ConnectTo(endPoint, this.OnConnectedAndWrite);
            tcp.RemoveReference();

            this.loop.RunDefault();
            Assert.Equal(1, this.callbackCount);

            this.CloseHandle(tcp);
        }

        void OnConnectedAndWrite(StreamHandle handle, Exception exception)
        {
            this.callbackCount++;
            if (exception == null)
            {
                var data = new byte[1];
                handle.QueueWriteStream(data, 0, data.Length, this.OnWriteCompleted);
            }
        }

        void OnWriteCompleted(StreamHandle handle, Exception exception) => this.callbackCount++;

        [Fact]
        public void Pipe()
        {
            Pipe pipe = this.loop.CreatePipe();
            pipe.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(pipe);
        }

        [Fact]
        public void PipeListen()
        {
            Pipe pipe = this.loop.CreatePipe();
            StreamListener<Pipe> listener = pipe.Listen(GetPipeName(), this.OnConnection);

            pipe.RemoveReference();
            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            listener.Close(this.OnClose);
        }

        [Fact]
        public void PipeListen2()
        {
            Pipe pipe = this.loop.CreatePipe();
            StreamListener<Pipe> listener = pipe.Listen(GetPipeName(), this.OnConnection);
            pipe.RemoveReference();
            pipe.CloseHandle(this.OnClose);

            this.loop.RunDefault();
            Assert.Equal(1, this.closeCount);

            listener.Close(this.OnClose);
        }

        [Fact]
        public void PipeConnectNoServer()
        {
            Pipe pipe = this.loop
                .CreatePipe()
                .ConnectTo(GetPipeName(), this.OnConnectedAndShutdown);
            pipe.RemoveReference();

            this.loop.RunDefault();
            Assert.Equal(2, this.callbackCount);

            this.CloseHandle(pipe);
        }

        [Fact]
        public void PipeConnect2NoServer()
        {
            Pipe pipe = this.loop
                .CreatePipe()
                .ConnectTo(GetPipeName(), this.OnConnectedAndWrite);
            pipe.RemoveReference();

            this.loop.RunDefault();
            Assert.Equal(1, this.callbackCount);

            this.CloseHandle(pipe);
        }

        static string GetPipeName() => Platform.IsWindows
                ? "\\\\?\\pipe\\uv-test5"
                : "/tmp/uv-test5-sock";

        [Fact]
        public void Udp()
        {
            Udp udp = this.loop.CreateUdp();
            udp.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(udp);
        }

        [Fact]
        public void UdpReceive()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            Udp udp = this.loop
                .CreateUdp()
                .ReceiveStart(endPoint, this.OnReceive);
            udp.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(0, this.callbackCount);

            this.CloseHandle(udp);
        }

        void OnReceive(Udp udp, IDatagramReadCompletion datagramReadCompletion) => this.callbackCount++;

        [Fact]
        public void UdpSend()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, Port);
            byte[] data = Encoding.UTF8.GetBytes("PING");

            Udp udp = this.loop.CreateUdp();
            udp.QueueSend(data, endPoint, this.OnSendCompleted);
            udp.RemoveReference();

            long diff = this.loop.NowInHighResolution;
            this.loop.RunDefault();
            diff = this.loop.NowInHighResolution - diff;
            Assert.True(diff >= 0 && diff < 5000000);  // High resolution 
            Assert.Equal(1, this.callbackCount);

            this.CloseHandle(udp);
        }

        void OnSendCompleted(Udp udp, Exception exception) => this.callbackCount++;

        void CloseHandle(ScheduleHandle handle)
        {
            handle.CloseHandle(this.OnClose);
            this.loop.RunDefault();
            Assert.Equal(1, this.closeCount);
        }

        void OnCallback(ScheduleHandle handle) => this.callbackCount++;

        void OnClose(ScheduleHandle handle)
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
