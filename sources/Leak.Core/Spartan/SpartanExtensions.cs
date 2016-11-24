﻿using Leak.Core.Common;
using Leak.Core.Events;

namespace Leak.Core.Spartan
{
    public static class SpartanExtensions
    {
        public static void CallTaskStarted(this SpartanHooks hooks, FileHash hash, SpartanTasks task)
        {
            hooks.OnTaskStarted?.Invoke(new TaskStarted
            {
                Hash = hash,
                Task = task
            });
        }

        public static void CallTaskCompleted(this SpartanHooks hooks, FileHash hash, SpartanTasks task)
        {
            hooks.OnTaskCompleted?.Invoke(new TaskCompleted
            {
                Hash = hash,
                Task = task
            });
        }

        public static void CallMetadataDiscovered(this SpartanHooks hooks, MetadataDiscovered data)
        {
            hooks.OnMetadataDiscovered?.Invoke(data);
        }
    }
}