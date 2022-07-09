﻿using System.Threading.Tasks;
using UnityEngine;

namespace AsyncUtils
{
    public static class Extensions
    {
        public static async Task AwaitAsync(this AsyncOperation operation)
        {
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }
}