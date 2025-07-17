using System.Collections.Generic;
using MFPC.Utils.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace MFPC
{
    [System.Serializable]
    public class UIDragHandler
    {
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField] private Image[] dragImages;

        private Dictionary<Image, DraggableImage> _draggableImages = new Dictionary<Image, DraggableImage>();
        private ISaver _initialSaver;
        private ISaver _newSaver;

        public void Initialize()
        {
            _initialSaver = new PlayerPrefsSaver("InitialMobileUISetup");
            _newSaver = new PlayerPrefsSaver("NewMobileUISetup");
            Save(_initialSaver);
            Load(_newSaver);
        }

        /// <summary>
        /// Sets up buttons for shuffling
        /// </summary>
        public void StartDrag()
        {
            _draggableImages.Clear();

            foreach (var image in dragImages)
            {
                Component[] components = image.GetComponentsInChildren<Component>();

                foreach (Component component in components)
                {
                    if (component is MonoBehaviour monoBehaviour && !(component is Image))
                    {
                        monoBehaviour.enabled = false;
                    }
                }

                if (image.sprite == null) image.color = outlineColor;
                _draggableImages.Add(image, image.gameObject.AddComponent<DraggableImage>());
            }
        }

        /// <summary>
        /// Returns the button components to the state before the movement began
        /// <remarks> Does not apply to RectTransform </remarks>
        /// </summary>
        public void EndDrag()
        {
            foreach (var draggableImage in _draggableImages)
            {
                Component[] components = draggableImage.Key.GetComponentsInChildren<Component>();

                foreach (Component component in components)
                {
                    if (component is MonoBehaviour monoBehaviour)
                    {
                        monoBehaviour.enabled = true;
                    }
                }

                if (draggableImage.Key.sprite == null) draggableImage.Key.color = Color.clear;
                Object.Destroy(draggableImage.Value as Object);
            }

            Save(_newSaver);
        }

        public void ReturnInitialPositions()
        {
            Load(_initialSaver);
        }

        private void Save(ISaver saver)
        {
            for (var i = 0; i < dragImages.Length; i++)
            {
                saver.Save($"dragImages{i}", dragImages[i].rectTransform.anchoredPosition);
            }
        }

        private void Load(ISaver saver)
        {
            for (var i = 0; i < dragImages.Length; i++)
            {
                saver.Load<Vector2>($"dragImages{i}", _ => { dragImages[i].rectTransform.anchoredPosition = _; });
            }
        }
    }
}