using System.Threading;
using UnityEngine;

namespace AsyncUtils
{
    /// <summary>
    /// Component to facilitate popups creation with UnityEvents
    /// </summary>
    public class PopupLauncher : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            
            _cts.Dispose();
        }

        public void LaunchPopup(AsyncPopup popup)
        {
            var ct = _cts.Token;
            Popups.ShowPopup(popup, ct);
        }
        
        public void LaunchPopup<T, TD>(AsyncPopup<T, TD> popup, TD data)
        {
            var ct = _cts.Token;
            Popups.ShowPopup(popup, data, ct);
        }
        
        public void LaunchPopup<TD>(AsyncPopupInitializable<TD> popup, TD data)
        {
            var ct = _cts.Token;
            Popups.ShowPopup(popup, data, ct);
        }

        public void LaunchPopup<T>(AsyncPopupReturnable<T> popup)
        {
            var ct = _cts.Token;
            Popups.ShowPopup(popup, ct);
        }
    }
}