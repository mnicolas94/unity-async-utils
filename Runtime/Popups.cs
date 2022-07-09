﻿using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncUtils
{
    public static class Popups
    {
        public static async Task ShowPopup(AsyncPopup popupPrefab, CancellationToken ct)
        {
            var popup = Object.Instantiate(popupPrefab);
            try
            {
                popup.Initialize();
                await AnimatePopupShow(popup, ct);
                await popup.Show(ct);
            }
            finally
            {
                popup.StartCoroutine(AnimatePopupHide(popup));
            }
        }
        
        public static async Task ShowPopup<T>(AsyncPopupInitializable<T> popupPrefab, T popupData, CancellationToken ct)
        {
            var popup = Object.Instantiate(popupPrefab);
            try
            {
                popup.Initialize(popupData);
                await AnimatePopupShow(popup, ct);
                await popup.Show(ct);
            }
            finally
            {
                popup.StartCoroutine(AnimatePopupHide(popup));
            }
        }
        
        public static async Task<T> ShowPopup<T>(AsyncPopupReturnable<T> popupPrefab, CancellationToken ct)
        {
            var popup = Object.Instantiate(popupPrefab);
            try
            {
                popup.Initialize();
                await AnimatePopupShow(popup, ct);
                var popupResult = await popup.Show(ct);

                return popupResult;
            }
            finally
            {
                popup.StartCoroutine(AnimatePopupHide(popup));
            }
        }
        
        public static async Task<T> ShowPopup<T, TD>(AsyncPopup<T, TD> popupPrefab, TD popupData, CancellationToken ct)
        {
            var popup = Object.Instantiate(popupPrefab);
            try
            {
                popup.Initialize(popupData);
                await AnimatePopupShow(popup, ct);
                var popupResult = await popup.Show(ct);

                return popupResult;
            }
            finally
            {
                popup.StartCoroutine(AnimatePopupHide(popup));
            }
        }
        
        private static async Task AnimatePopupShow(MonoBehaviour popup, CancellationToken ct)
        {
            popup.gameObject.SetActive(true);
            await Task.Yield();
        }
        
        private static IEnumerator AnimatePopupHide(MonoBehaviour popup)
        {
            yield return null;
            Object.Destroy(popup.gameObject);
        }
    }
}