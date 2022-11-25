﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
 using UnityEngine;
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
        
        public static async Task<Button> WaitFirstButtonPressedAsync(CancellationToken ct, params Button[] buttons)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            try
            {
                var linkedCt = linkedCts.Token;
                var tasks = buttons.Select(button => WaitPressButtonAsync(button, linkedCt));
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
    }
}