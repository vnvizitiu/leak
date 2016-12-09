﻿using Leak.Common;
using Leak.Events;

namespace Leak.Networking
{
    public static class NetworkPoolExtensions
    {
        public static void CallConnectionAttached(this NetworkPoolHooks hooks, NetworkConnection connection)
        {
            hooks.OnConnectionAttached?.Invoke(new ConnectionAttached
            {
                Connection = connection,
                Remote = connection.Remote
            });
        }

        public static void CallConnectionTerminated(this NetworkPoolHooks hooks, NetworkConnection connection)
        {
            hooks.OnConnectionTerminated?.Invoke(new ConnectionTerminated
            {
                Connection = connection,
                Remote = connection.Remote
            });
        }

        public static void CallConnectionEncrypted(this NetworkPoolHooks hooks, NetworkConnection connection)
        {
            hooks.OnConnectionEncrypted?.Invoke(new ConnectionEncrypted
            {
                Connection = connection,
                Remote = connection.Remote
            });
        }
    }
}