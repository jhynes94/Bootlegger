using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MFPC
{
    /// <summary>
    /// Add an additional way to control the character
    /// </summary>
    public class JoystickWithRunField : Joystick
    {
        /// <summary>
        /// Run button
        /// </summary>
        [SerializeField] private RunField runField;

        private Image imageRunField;
        private float currentColorAlphaField;

        #region MONO

        protected override void Awake()
        {
            imageRunField = runField.GetComponent<Image>();

            base.Awake();

            imageRunField.raycastTarget = false;
        }

        #endregion

        protected override void FadingJoystick()
        {
            if (joystick.color.a <= 0.01f && runField.InRunField) AppearanceRunField();

            base.FadingJoystick();
        }

        protected override void ChangeJoystickAlpha(float alpha)
        {
            base.ChangeJoystickAlpha(alpha);

            if(imageRunField != null) imageRunField.color = mainColor;
        }

        private void AppearanceRunField()
        {
            mainColor.a = Mathf.SmoothDamp(mainColor.a, 1f, ref currentColorAlphaField, timeToFadeIn);
            imageRunField.color = mainColor;
        }

        #region CALLBACK

        public override void OnPointerUp(PointerEventData point)
        {
            base.OnPointerUp(point);

            imageRunField.raycastTarget = false;
        }

        public override void OnPointerDown(PointerEventData point)
        {
            base.OnPointerDown(point);

            imageRunField.raycastTarget = true;
        }

        public override void OnDrag(PointerEventData point)
        {
            base.OnDrag(point);

            runField.OnDrag(point);
        }

        #endregion
    }
}