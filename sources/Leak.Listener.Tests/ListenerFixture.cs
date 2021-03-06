﻿using Leak.Completion;
using Leak.Networking;
using Leak.Tasks;
using System;
using System.Threading.Tasks;
using Leak.Listener.Events;

namespace Leak.Listener.Tests
{
    public class ListenerFixture : IDisposable
    {
        private readonly LeakPipeline pipeline;
        private readonly CompletionThread worker;
        private readonly NetworkPool pool;
        private readonly PeerListenerHooks hooks;

        private PeerListener listener;

        public ListenerFixture()
        {
            pipeline = new LeakPipeline();
            pipeline.Start();

            worker = new CompletionThread();
            worker.Start();

            pool =
                new NetworkPoolBuilder()
                    .WithPipeline(pipeline)
                    .WithWorker(worker)
                    .WithMemory(new ListenerMemory())
                    .Build();

            pool.Start();

            hooks = new PeerListenerHooks();
        }

        public PeerListenerHooks Hooks
        {
            get { return hooks; }
        }

        public Task<ListenerSession> Start()
        {
            Action<ListenerStarted> onStarted = hooks.OnListenerStarted;
            TaskCompletionSource<ListenerSession> completion = new TaskCompletionSource<ListenerSession>();

            listener =
                new PeerListenerBuilder()
                    .WithNetwork(pool)
                    .Build(hooks);

            listener.Hooks.OnListenerStarted = data =>
            {
                onStarted?.Invoke(data);
                completion.SetResult(new ListenerSession(data.Port, pool));
            };

            listener.Start();

            return completion.Task;
        }

        public void Dispose()
        {
            listener?.Stop();
            listener?.Dispose();

            worker?.Dispose();
            pipeline?.Stop();
        }
    }
}