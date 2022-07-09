﻿using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncUtils
{
    public abstract class AsyncPopup : MonoBehaviour
    {
        public abstract Task Show(CancellationToken ct);
        public abstract void Initialize();
    }
    
    public abstract class AsyncPopup<T, TD> : MonoBehaviour
    {
        public abstract Task<T> Show(CancellationToken ct);
        public abstract void Initialize(TD popupData);
    }
    
    public abstract class AsyncPopupInitializable<T> : MonoBehaviour
    {
        public abstract Task Show(CancellationToken ct);
        public abstract void Initialize(T popupData);
    }
    
    public abstract class AsyncPopupReturnable<T> : MonoBehaviour
    {
        public abstract Task<T> Show(CancellationToken ct);
        public abstract void Initialize();
    }
}