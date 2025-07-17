using MFPC.Utils.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace MFPC
{
    [System.Serializable]
    public class MobileUISetup
    {
        [SerializeField] private Image touchFieldImage;
        [SerializeField] private Button startCustomizeMobileUIButton;
        [SerializeField] private Button endCustomizeMobileUIButton;
        [SerializeField] private Button initialLoadCustomizeMobileUIButton;
        [SerializeField] private UIDragHandler dragHandler;
        
        private GameObject _settingsField;

        public void Initialize(GameObject settingsField)
        {
            _settingsField = settingsField;

            dragHandler.Initialize();
            
            startCustomizeMobileUIButton.onClick.AddListener(StartCustomizeMobileUI);
            endCustomizeMobileUIButton.onClick.AddListener(EndCustomizeMobileUI);
            initialLoadCustomizeMobileUIButton.onClick.AddListener(dragHandler.ReturnInitialPositions);
        }
                
        ~MobileUISetup()
        {
            startCustomizeMobileUIButton.onClick.RemoveListener(StartCustomizeMobileUI);
            endCustomizeMobileUIButton.onClick.RemoveListener(EndCustomizeMobileUI);
            initialLoadCustomizeMobileUIButton.onClick.RemoveListener(dragHandler.ReturnInitialPositions);
        }
        
        private void StartCustomizeMobileUI()
        {
            dragHandler.StartDrag();

            touchFieldImage.enabled = false;
            _settingsField.SetActive(false);
            endCustomizeMobileUIButton.gameObject.SetActive(true);
            initialLoadCustomizeMobileUIButton.gameObject.SetActive(true);
        }
        
        private void EndCustomizeMobileUI()
        {
            dragHandler.EndDrag();
            
            touchFieldImage.enabled = true;
            _settingsField.SetActive(true);
            endCustomizeMobileUIButton.gameObject.SetActive(false);
            initialLoadCustomizeMobileUIButton.gameObject.SetActive(false);
        }
    }
}