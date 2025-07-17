using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFPC
{
    public class LeanField  : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private enum LeanDirection
        {
            Left = -1,
            Right = 1
        }
        
        public event Action<float> OnLeanDirectionChange;

        [SerializeField] private LeanDirection leanDirection;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnLeanDirectionChange?.Invoke((int)leanDirection);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnLeanDirectionChange?.Invoke(0);
        }
    }
}