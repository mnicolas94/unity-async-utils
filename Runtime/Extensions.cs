﻿using System.Threading;
 using System.Threading.Tasks;
using UnityEngine;

namespace AsyncUtils
{
    public static class Extensions
    {
        public static async Task AwaitAsync(this AsyncOperation operation, CancellationToken ct)
        {
            while (!operation.isDone && !ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }
    }
}