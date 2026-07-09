using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lionrise
{
    public sealed class CardDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public Action<ChoiceSide, float> previewChanged;
        public Action previewCleared;
        public Action<ChoiceSide> committed;

        private RectTransform rectTransform;
        private RectTransform movementBounds;
        private Canvas canvas;
        private Text leftLabel;
        private Text rightLabel;
        private HoloCardGraphic graphic;
        private Vector2 startPosition;
        private Vector2 rawOffset;
        private float dragStarted;
        private bool enabledForInput;
        private bool dragging;

        public void Configure(RectTransform bounds, Canvas owningCanvas, Text left, Text right, HoloCardGraphic cardGraphic)
        {
            rectTransform = (RectTransform)transform;
            movementBounds = bounds;
            canvas = owningCanvas;
            leftLabel = left;
            rightLabel = right;
            graphic = cardGraphic;
            startPosition = rectTransform.anchoredPosition;
        }

        public void SetEnabled(bool value) => enabledForInput = value;

        public void ResetCard()
        {
            StopAllCoroutines();
            dragging = false;
            rawOffset = Vector2.zero;
            rectTransform.anchoredPosition = startPosition;
            rectTransform.localRotation = Quaternion.identity;
            graphic.SetCommitted(false);
            SetAlpha(leftLabel, 0);
            SetAlpha(rightLabel, 0);
            previewCleared?.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!enabledForInput) return;
            dragging = true;
            rawOffset = Vector2.zero;
            dragStarted = Time.unscaledTime;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!enabledForInput || !dragging) return;
            rawOffset += eventData.delta / canvas.scaleFactor;
            var normalized = Mathf.Clamp01(Mathf.Abs(rawOffset.x) / movementBounds.rect.width * 2.4f);
            var quintOut = 1f - Mathf.Pow(1f - normalized, 5f);
            var next = startPosition + new Vector2(Mathf.Sign(rawOffset.x) * quintOut * 80f,
                Mathf.Clamp(rawOffset.y * .16f, -18f, 24f));
            rectTransform.anchoredPosition = next;
            UpdateFeedback();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enabledForInput || !dragging) return;
            dragging = false;
            var ratio = Mathf.Abs(rawOffset.x) / movementBounds.rect.width;
            if (ratio >= .035f && Time.unscaledTime - dragStarted >= .05f)
            {
                enabledForInput = false;
                graphic.SetCommitted(true);
                committed?.Invoke(rectTransform.anchoredPosition.x < startPosition.x ? ChoiceSide.Left : ChoiceSide.Right);
            }
            else StartCoroutine(SnapBack());
        }

        public void CommitFromKeyboard(ChoiceSide side)
        {
            if (!enabledForInput) return;
            enabledForInput = false;
            rawOffset = new Vector2(side == ChoiceSide.Left ? -movementBounds.rect.width * .2f : movementBounds.rect.width * .2f, 0f);
            rectTransform.anchoredPosition = startPosition + new Vector2(side == ChoiceSide.Left ? -80f : 80f, 0);
            UpdateFeedback();
            graphic.SetCommitted(true);
            committed?.Invoke(side);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!enabledForInput || dragging) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                    eventData.pressEventCamera, out var localPoint)) return;
            CommitFromKeyboard(localPoint.x < 0f ? ChoiceSide.Left : ChoiceSide.Right);
        }

        private void UpdateFeedback()
        {
            var offset = rectTransform.anchoredPosition.x - startPosition.x;
            var ratio = Mathf.Abs(offset) / movementBounds.rect.width;
            var vertical = rectTransform.anchoredPosition.y - startPosition.y;
            var angle = -offset * .042f - offset * vertical * .0012f;
            rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp(angle, -8f, 8f));
            var alpha = Mathf.InverseLerp(.015f, .08f, ratio);
            SetAlpha(leftLabel, offset < 0 ? alpha : 0);
            SetAlpha(rightLabel, offset > 0 ? alpha : 0);
            graphic.borderColor = ratio >= .15f ? new Color32(255, 246, 190, 255) : new Color32(224, 194, 130, 225);
            graphic.SetVerticesDirty();
            if (ratio >= .02f) previewChanged?.Invoke(offset < 0 ? ChoiceSide.Left : ChoiceSide.Right, Mathf.Clamp01(ratio / .285f));
            else previewCleared?.Invoke();
        }

        private IEnumerator SnapBack()
        {
            var from = rectTransform.anchoredPosition;
            var rotation = rectTransform.localRotation;
            for (var elapsed = 0f; elapsed < .16f; elapsed += Time.unscaledDeltaTime)
            {
                var t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / .16f), 3f);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, startPosition, t);
                rectTransform.localRotation = Quaternion.Slerp(rotation, Quaternion.identity, t);
                yield return null;
            }
            ResetCard();
        }

        private static void SetAlpha(Graphic target, float alpha)
        {
            var color = target.color;
            color.a = alpha;
            target.color = color;
        }
    }
}
