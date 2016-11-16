﻿using FluentAssertions;
using Leak.Completion;
using Leak.Core.Common;
using Leak.Core.Communicator;
using Leak.Core.Connector;
using Leak.Core.Core;
using Leak.Core.Events;
using Leak.Core.Listener;
using Leak.Core.Loop;
using Leak.Core.Messages;
using Leak.Core.Network;
using NUnit.Framework;

namespace Leak.Core.Tests.Network
{
    public class LoopyTests
    {
        private LeakPipeline pipeline;
        private CompletionThread worker;
        private NetworkPool pool;
        private PeerConnector connector;
        private PeerListener listener;
        private ConnectionLoop loopy;
        private ConnectionLoopHooks hooks;
        private ConnectionLoopConfiguration configuration;
        private CommunicatorService communicator;
        private Trigger<HandshakeCompleted> connected;
        private BufferedBlockFactory blocks;

        [SetUp]
        public void SetUp()
        {
            PeerConnectorHooks connectorHooks = new PeerConnectorHooks
            {
                OnHandshakeCompleted = connected = new Trigger<HandshakeCompleted>(data =>
                {
                    loopy.StartProcessing(data.Handshake.Remote, data.Connection);
                })
            };

            PeerListenerHooks listenerHooks = new PeerListenerHooks
            {
                OnHandshakeCompleted = data =>
                {
                    communicator = new CommunicatorService(data.Handshake.Remote, data.Connection, new CommunicatorHooks(), new CommunicatorConfiguration());
                }
            };

            pipeline = new LeakPipeline();
            worker = new CompletionThread();

            pool = new NetworkPool(worker, new NetworkPoolHooks());
            listener = new PeerListener(pool, listenerHooks, new PeerListenerConfiguration());
            connector = new PeerConnector(pool, connectorHooks, new PeerConnectorConfiguration());
            blocks = new BufferedBlockFactory();

            hooks = new ConnectionLoopHooks();
            configuration = new ConnectionLoopConfiguration();
            loopy = new ConnectionLoop(new BufferedBlockFactory(), hooks, configuration);

            worker.Start();
            pipeline.Start();
            pool.Start(pipeline);
            connector.Start(pipeline);
            listener.Start();
        }

        [TearDown]
        public void TearDown()
        {
            worker.Dispose();
            pipeline.Stop();
            listener.Stop();
        }

        [Test]
        public void ShouldTriggerMessageReceivedWhenKeepAlive()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("keep-alive");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendKeepAlive();

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedChoke()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("choke");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendChoke();

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedUnchoke()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("unchoke");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendUnchoke();

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedInterested()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("interested");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendInterested();

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedHave()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("have");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendHave(2);

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedBitfield()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("bitfield");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendBitfield(new Bitfield(20));

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedPiece()
        {
            FileHash hash = FileHash.Random();
            DataBlock block = blocks.New(10, null);

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("piece");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendPiece(new Piece(1, 2, block));

            handler.Complete().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerMessageReceivedExtended()
        {
            FileHash hash = FileHash.Random();

            var handler = hooks.OnMessageReceived.Trigger(data =>
            {
                data.Peer.Should().NotBeNull();
                data.Type.Should().Be("extended");
                data.Payload.Should().NotBeNull();
            });

            hooks.OnMessageReceived = handler;

            listener.Enable(hash);
            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", 8080));

            connected.Complete();
            communicator.SendExtended(new Extended(17, new byte[2]));

            handler.Complete().Should().BeTrue();
        }
    }
}