using UnityEngine;
using UnityEngine.EventSystems;

namespace MFPC
{
    public class DraggableImage : MonoBehaviour, IDragHandler
    {
        private RectTransform _rectTransform;
        
        private void Awake() => _rectTransform = GetComponent<RectTransform>();
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!IsOutOfScreen(eventData.delta))
            {
                _rectTransform.position += new Vector3(eventData.delta.x, eventData.delta.y);
            }
        }

        private bool IsOutOfScreen(Vector2 delta)
        {
            Vector3[] v = new Vector3[4];

            _rectTransform.GetWorldCorners(v);

            for (var i = 0; i < 4; i++)
            {
                if (v[i].x + delta.x < 0 || v[i].y + delta.y < 0 || v[i].x + delta.x > Screen.width ||
                    v[i].y + delta.y > Screen.height)
                {
                    return true;
                }
            }

            return false;
        }
    }
}