﻿using Leak.Core.Common;
using Leak.Core.Messages;
using Leak.Core.Network;

namespace Leak.Core.Loop
{
    public class ConnectionLoopHandler
    {
        private readonly PeerHash peer;
        private readonly DataBlockFactory factory;
        private readonly ConnectionLoopConnection connection;
        private readonly ConnectionLoopHooks hooks;

        public ConnectionLoopHandler(PeerHash peer, DataBlockFactory factory, ConnectionLoopConnection connection, ConnectionLoopHooks hooks)
        {
            this.peer = peer;
            this.factory = factory;
            this.hooks = hooks;
            this.connection = connection;
        }

        public void Execute()
        {
            connection.Receive(OnMessageHeader, 4);
        }

        private void OnMessageHeader(NetworkIncomingMessage message)
        {
            connection.Receive(OnMessageData, message.GetSize() + 4);
        }

        private void OnMessageData(NetworkIncomingMessage message)
        {
            if (message.Length == 4)
            {
                hooks.CallMessageReceived(peer, "keep-alive", message);
                hooks.CallOnPeerKeepAliveMessageReceived(peer);
                message.Acknowledge(4);
            }
            else
            {
                switch (message[4])
                {
                    case 0:
                        hooks.CallOnPeerChokeMessageReceived(peer);
                        break;

                    case 1:
                        hooks.CallOnPeerUnchokeMessageReceived(peer);
                        break;

                    case 2:
                        hooks.CallOnPeerInterestedMessageReceived(peer);
                        break;

                    case 4:
                        hooks.CallOnPeerHaveMessageReceived(peer, message);
                        break;

                    case 5:
                        hooks.CallOnPeerBitfieldMessageReceived(peer, message);
                        break;

                    case 7:
                        hooks.CallOnPeerPieceMessageReceived(peer, message, factory);
                        break;

                    case 20:
                        hooks.CallOnPeerExtendedMessageReceived(peer, message);
                        break;
                }

                Acknowledge(message);
            }

            Next();
        }

        private void Acknowledge(NetworkIncomingMessage message)
        {
            message.Acknowledge(message.GetSize() + 4);
        }

        private void Next()
        {
            connection.Receive(OnMessageHeader, 4);
        }
    }
}