using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lionrise
{
    public sealed class HoldChoiceButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action completed;
        private Image fill;
        private bool holding;
        private float held;

        public void Configure(Image fillImage) => fill = fillImage;
        public void OnPointerDown(PointerEventData eventData) { holding = true; held = 0; }
        public void OnPointerUp(PointerEventData eventData) => Cancel();
        public void OnPointerExit(PointerEventData eventData) => Cancel();

        private void Update()
        {
            if (!holding) return;
            held += Time.unscaledDeltaTime;
            fill.fillAmount = Mathf.Clamp01(held / .65f);
            if (held < .65f) return;
            holding = false;
            fill.fillAmount = 0;
            completed?.Invoke();
        }

        private void Cancel()
        {
            holding = false;
            held = 0;
            if (fill != null) fill.fillAmount = 0;
        }
    }
}
