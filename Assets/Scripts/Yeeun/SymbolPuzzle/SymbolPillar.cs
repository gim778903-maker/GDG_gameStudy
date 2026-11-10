using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// A pillar with a symbol on each of its four sides (variant 2). Interacting turns it
    /// 90 degrees. The "front" is this object's +Z direction (aim it at the player's side);
    /// <see cref="FrontShape"/> is the symbol currently facing that way.
    /// Hierarchy: this root stays still, <see cref="body"/> turns and holds the stone and
    /// the four face reliefs. <see cref="faces"/>[k] must sit on the body at k * 90 degrees.
    /// </summary>
    public class SymbolPillar : MonoBehaviour, ISymbolInteractable
    {
        [Tooltip("Turning part (stone + face reliefs).")]
        [SerializeField] Transform body;
        [Tooltip("Face reliefs: [0] front at rest, then each next one 90 degrees further around Y.")]
        [SerializeField] SymbolRelief[] faces = new SymbolRelief[4];
        [Tooltip("How many quarter turns the pillar starts with (also where a reset returns to).")]
        [SerializeField, Range(0, 3)] int startTurns;
        [SerializeField] float turnDuration = 0.35f;

        public UnityEvent onTurned = new UnityEvent();
        public event Action<SymbolPillar> Turned;

        public int Turns { get; private set; }
        public bool IsTurning { get; private set; }
        /// <summary>Set by the puzzle once solved so the pillars stay put.</summary>
        public bool IsLocked { get; set; }

        /// <summary>The face pointing along this object's +Z.</summary>
        public SymbolShape FrontShape
        {
            get
            {
                if (faces == null || faces.Length != 4) return SymbolShape.Circle;
                int k = (4 - Turns % 4) % 4;
                return faces[k] != null ? faces[k].Shape : SymbolShape.Circle;
            }
        }

        void Awake()
        {
            Turns = startTurns;
            ApplyTurns();
        }

        void OnValidate()
        {
            if (body != null && !Application.isPlaying)
                body.localRotation = Quaternion.Euler(0f, startTurns * 90f, 0f);
        }

        public string GetPrompt(SymbolInteractor interactor) =>
            IsTurning || IsLocked ? null : "[E] 기둥 돌리기";

        public void Interact(SymbolInteractor interactor) => Turn();

        /// <summary>Turns a quarter turn (ignored while turning or locked).</summary>
        public void Turn()
        {
            if (IsTurning || IsLocked || body == null) return;
            StartCoroutine(TurnTo(Turns + 1, true));
        }

        /// <summary>Turns back to the start position (used when the answer was wrong).</summary>
        public void ResetTurns()
        {
            if (IsLocked || body == null) return;
            StopAllCoroutines();
            if (Turns == startTurns && !IsTurning) return;
            StartCoroutine(TurnTo(startTurns, false));
        }

        IEnumerator TurnTo(int target, bool notify)
        {
            IsTurning = true;
            Quaternion from = body.localRotation;
            Quaternion to = Quaternion.Euler(0f, target * 90f, 0f);
            for (float t = 0f; t < turnDuration; t += Time.deltaTime)
            {
                body.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t / turnDuration));
                yield return null;
            }
            Turns = ((target % 4) + 4) % 4;
            ApplyTurns();
            IsTurning = false;
            if (!notify) yield break;
            onTurned.Invoke();
            Turned?.Invoke(this);
        }

        void ApplyTurns()
        {
            if (body != null) body.localRotation = Quaternion.Euler(0f, Turns * 90f, 0f);
        }
    }
}
