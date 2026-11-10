using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// A floor plate carrying a symbol. Stepping on it (trigger collider) reports a press.
    /// The plate itself does not know the answer; <see cref="SymbolSequencePuzzle"/> decides.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SymbolPlate : SymbolVisual
    {
        public enum Feedback { None, Correct, Wrong }

        [Tooltip("Only colliders with this tag press the plate. Leave empty to accept anything with a Rigidbody or CharacterController.")]
        [SerializeField] string presserTag = "Player";
        [Tooltip("Object that sinks while the plate is pressed (usually the symbol child).")]
        [SerializeField] Transform pressVisual;
        [SerializeField] float pressDepth = 0.04f;
        [SerializeField] Color correctColor = new Color(0.45f, 0.9f, 1f);
        [SerializeField] Color wrongColor = new Color(1f, 0.25f, 0.2f);
        [SerializeField] float wrongFlashTime = 0.6f;

        [Tooltip("Called every time the plate is stepped on (useful for sounds).")]
        public UnityEvent onPressed = new UnityEvent();

        /// <summary>Raised when something starts pressing the plate.</summary>
        public event Action<SymbolPlate> Pressed;

        public bool IsPressed => pressers.Count > 0;
        public Feedback CurrentFeedback { get; private set; }

        // Colliders currently on the plate. Unity skips OnTriggerExit when a collider is
        // disabled or destroyed (respawn, teleport, despawn), so stale entries are pruned.
        readonly HashSet<Collider> pressers = new HashSet<Collider>();
        Collider trigger;
        Vector3 visualRestPos;
        Coroutine flashRoutine;

        protected override void Awake()
        {
            base.Awake();
            trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            if (pressVisual != null) visualRestPos = pressVisual.localPosition;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsPresser(other)) return;
            bool wasEmpty = pressers.Count == 0;
            if (pressers.Add(other) && wasEmpty) Press();
        }

        void OnTriggerExit(Collider other)
        {
            if (pressers.Remove(other) && pressers.Count == 0) UpdateVisualHeight();
        }

        void FixedUpdate()
        {
            if (pressers.Count == 0) return;
            Bounds area = trigger.bounds;
            int removed = pressers.RemoveWhere(c =>
                c == null || !c.enabled || !c.gameObject.activeInHierarchy || !area.Intersects(c.bounds));
            if (removed > 0 && pressers.Count == 0) UpdateVisualHeight();
        }

        void OnDisable()
        {
            pressers.Clear();
            UpdateVisualHeight();
        }

        /// <summary>Simulates stepping on the plate (for tests or other scripts).</summary>
        public void Press()
        {
            UpdateVisualHeight();
            onPressed.Invoke();
            Pressed?.Invoke(this);
        }

        public void SetFeedback(Feedback feedback)
        {
            if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
            CurrentFeedback = feedback;
            switch (feedback)
            {
                case Feedback.Correct: SetTint(correctColor); break;
                case Feedback.Wrong:
                    SetTint(wrongColor);
                    if (isActiveAndEnabled) flashRoutine = StartCoroutine(ClearAfter(wrongFlashTime));
                    break;
                default: SetTint(null); break;
            }
        }

        IEnumerator ClearAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            flashRoutine = null;
            CurrentFeedback = Feedback.None;
            SetTint(null);
        }

        bool IsPresser(Collider other)
        {
            if (!string.IsNullOrEmpty(presserTag)) return other.CompareTag(presserTag);
            return other.attachedRigidbody != null || other is CharacterController;
        }

        void UpdateVisualHeight()
        {
            if (pressVisual == null) return;
            pressVisual.localPosition = visualRestPos + (IsPressed ? Vector3.down * pressDepth : Vector3.zero);
        }
    }
}
