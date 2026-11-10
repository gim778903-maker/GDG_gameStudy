using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// A wall button the player presses with the interact key (e.g. "문 열기" next to the
    /// pillar puzzle door). Hook <see cref="onPressed"/> to the puzzle's Submit and the
    /// puzzle's result events to <see cref="FlashCorrect"/> / <see cref="FlashWrong"/>.
    /// The button's front faces this object's +Z.
    /// </summary>
    public class PuzzleButton : MonoBehaviour, ISymbolInteractable
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] string prompt = "[E] 문 열기";
        [Tooltip("Part that sinks when pressed and gets tinted.")]
        [SerializeField] Transform cap;
        [SerializeField] float pressDepth = 0.03f;
        [SerializeField] float pressTime = 0.15f;
        [SerializeField] Color correctColor = new Color(0.45f, 0.9f, 1f);
        [SerializeField] Color wrongColor = new Color(1f, 0.25f, 0.2f);
        [SerializeField] float flashTime = 0.8f;

        public UnityEvent onPressed = new UnityEvent();

        /// <summary>When false the button ignores presses and shows no prompt.</summary>
        public bool Interactable { get; set; } = true;
        public int PressCount { get; private set; }

        Vector3 capRestPos;
        MeshRenderer capRenderer;
        MaterialPropertyBlock block;
        bool animating;

        void Awake()
        {
            if (cap == null) return;
            capRestPos = cap.localPosition;
            capRenderer = cap.GetComponent<MeshRenderer>();
        }

        public string GetPrompt(SymbolInteractor interactor) => Interactable && !animating ? prompt : null;

        public void Interact(SymbolInteractor interactor) => Press();

        public void Press()
        {
            if (!Interactable || animating) return;
            PressCount++;
            StartCoroutine(PressAnimation());
            onPressed.Invoke();
        }

        /// <summary>Stops accepting presses (e.g. after the puzzle is solved).</summary>
        public void Lock() => Interactable = false;

        public void FlashCorrect() => StartCoroutine(Flash(correctColor, false));
        public void FlashWrong() => StartCoroutine(Flash(wrongColor, true));

        IEnumerator PressAnimation()
        {
            animating = true;
            if (cap != null) cap.localPosition = capRestPos + Vector3.back * pressDepth;
            yield return new WaitForSeconds(pressTime);
            if (cap != null) cap.localPosition = capRestPos;
            animating = false;
        }

        IEnumerator Flash(Color color, bool clearAfter)
        {
            if (capRenderer == null) yield break;
            block ??= new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color);
            capRenderer.SetPropertyBlock(block);
            if (!clearAfter) yield break;
            yield return new WaitForSeconds(flashTime);
            capRenderer.SetPropertyBlock(null);
        }
    }
}
