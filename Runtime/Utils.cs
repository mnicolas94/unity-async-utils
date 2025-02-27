﻿﻿using System;
 using System.Linq;
using System.Threading;
using System.Threading.Tasks;
 using UnityEngine;
 using UnityEngine.Events;
 using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils.Input;

namespace AsyncUtils
{
    public static class Utils
    {
        /// <summary>
        /// Cross-platform delay that works on WebGL builds
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task Delay(float seconds, CancellationToken ct)
        {
#if UNITY_WEBGL
            var start = Time.realtimeSinceStartup;
            var stop = start + seconds;
            while (Time.realtimeSinceStartup < stop && !ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
#else
            int waitMillis = (int) (seconds * 1000);
            await Task.Delay(waitMillis, ct);
#endif
        }
        
        public static async Task<T> NeverEndTaskAsync<T>(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Yield();
            }

            return default;
        }

        /// <summary>
        /// Get the first task to finish among several ones and cancel the others. The goal of this functions is to
        /// reduce nesting produced by the try-finally block needed to properly cancel and dispose the linked
        /// cancellation token source.
        /// Common usage:
        ///     var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ///     var linkedCt = linkedCts.Token;
        ///
        ///     Task task1 = Task1(linkedCt);
        ///     Task task2 = Task2(linkedCt);
        ///     var finishedTask = await GetFirstToFinish(linkedCts, task1, task2);
        /// </summary>
        /// <param name="linkedCts"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task<Task> GetFirstToFinish(CancellationTokenSource linkedCts, params Task[] tasks)
        {
            try
            {
                var finishedTask = await Task.WhenAny(tasks);
                return finishedTask;
            }
            finally
            {
                linkedCts.Cancel();
                linkedCts.Dispose();
            }
        }
        
        /// <summary>
        /// Wait for the first task to finish, execute a corresponding callback and cancel the remaining tasks.
        /// The goal of these functions is to reduce nesting produced by the try-finally block needed to properly cancel
        /// and dispose the linked cancellation token source.
        /// Common usage:
        ///     CancellationToken ct = ...
        ///     await WaitFirstToFinish(ct,
        ///         (
        ///             linkedCt => Task1(linkedCt);,
        ///             task1 =>
        ///             {
        ///                 // do something
        ///             }
        ///         ),
        ///         (
        ///             linkedCt => Task2(linkedCt),
        ///             task2 =>
        ///             {
        ///                 // do something
        ///             }
        ///         )
        ///     );
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="tasksAndResults"></param>
        public static async Task WaitFirstToFinish(CancellationToken ct,
            params (Func<CancellationToken, Task>, Action<Task>)[] tasksAndResults)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var linkedCt = linkedCts.Token;

            async Task<(Task, Action<Task>)> WrappedTask(Func<CancellationToken, Task> taskCreator,
                Action<Task> resultFunction)
            {
                var task = taskCreator(linkedCt);
                await task;
                return (task, resultFunction);
            }
            
            try
            {
                var tasks = tasksAndResults.Select(tuple =>
                {
                    var (taskCreator, resultFunction) = tuple;
                    return WrappedTask(taskCreator, resultFunction);
                });
                
                var finishedWrappedTask = await Task.WhenAny(tasks);
                var (finishedTask, resultFunction) = await finishedWrappedTask;
                resultFunction?.Invoke(finishedTask);
            }
            finally
            {
                linkedCts.Cancel();
                linkedCts.Dispose();
            }
        }
        
        public static async Task<Button> WaitFirstButtonPressedAsync(CancellationToken ct, params Button[] buttons)
        {
            // return early if all buttons are null
            var areAllNull = buttons.All(button => button == null);
            if (areAllNull)
            {
                return null;
            }
            
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var linkedCt = linkedCts.Token;
            
            try
            {
                var tasks = buttons
                    .Where(button => button != null)
                    .Select(button => WaitPressButtonAsync(button, linkedCt));
                var finishedTask = await Task.WhenAny(tasks);

                await finishedTask;  // propagate exception if the task finished because of it
                linkedCts.Cancel();
                
                return finishedTask.Result;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }
        
        public static async Task<Button> WaitPressButtonAsync(Button button, CancellationToken ct)
        {
            bool isPressed = false;
            void PressedAction()
            {
                isPressed = true;
            }

            try
            {
                button.onClick.AddListener(PressedAction);
            
                while (!isPressed && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }

                return button;
            }
            finally
            {
                button.onClick.RemoveListener(PressedAction);
            }
        }
        
        public static async Task WaitPressBackButton(CancellationToken ct)
        {
            bool isPressed = false;
            void OnPressedAction(InputAction.CallbackContext callbackContext) => isPressed = true;
            var backAction = InputActionUtils.GetBackAction();
            try
            {
                backAction.Enable();
                backAction.performed += OnPressedAction;
                while (!isPressed && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
            finally
            {
                backAction.performed -= OnPressedAction;
                backAction.Disable();
                backAction.Dispose();
            }
        }

        public static async Task WaitForInputAction(InputAction inputAction, CancellationToken ct)
        {
            bool isPerformed = false;
            void OnPerformedAction(InputAction.CallbackContext callbackContext)
            {
                isPerformed = true;
            }

            try
            {
                inputAction.Enable();
                inputAction.performed += OnPerformedAction;
                while (!isPerformed && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
            finally
            {
                inputAction.performed -= OnPerformedAction;
            }
        }
        
        public static async Task<T> WaitForInputAction<T>(InputAction inputAction, CancellationToken ct) where T : struct
        {
            bool isPerformed = false;
            T result = default;
            void OnPerformedAction(InputAction.CallbackContext callbackContext)
            {
                isPerformed = true;
                result = callbackContext.ReadValue<T>();
            }

            try
            {
                inputAction.Enable();
                inputAction.performed += OnPerformedAction;
                while (!isPerformed && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }

                return result;
            }
            finally
            {
                inputAction.performed -= OnPerformedAction;
            }
        }
        
        public static async Task WaitUnityEventAsync(UnityEvent evt, CancellationToken ct)
        {
            bool isTriggered = false;
            void TriggeredAction()
            {
                isTriggered = true;
            }
        
            try
            {
                evt.AddListener(TriggeredAction);
                    
                while (!isTriggered && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
            finally
            {
                evt.RemoveListener(TriggeredAction);
            }
        }
    }
}