using UnityEngine;
using UnityEngine.InputSystem;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Player-side interaction for the symbol puzzles: looks along the camera, finds an
    /// <see cref="ISymbolInteractable"/> and uses it with E. Also carries one
    /// <see cref="SymbolPiece"/> at a time (E on empty space drops it).
    /// Put it on the player; it does not depend on any specific movement controller.
    /// </summary>
    public class SymbolInteractor : MonoBehaviour
    {
        [SerializeField] Transform viewCamera;
        [Tooltip("Where a carried piece is held (child of the camera).")]
        [SerializeField] Transform holdPoint;
        [SerializeField] float reach = 2.5f;
        [Tooltip("Show a simple on-screen prompt (debug UI until the real HUD exists).")]
        [SerializeField] bool showDebugPrompt = true;
        [Tooltip("Prompt text height as a fraction of the screen height (scales with resolution).")]
        [SerializeField, Range(0.02f, 0.1f)] float promptSize = 0.05f;
        [Tooltip("Gap between the prompt and the bottom edge, as a fraction of the screen height.")]
        [SerializeField, Range(0f, 0.2f)] float promptBottomMargin = 0.05f;

        GUIStyle promptStyle, shadowStyle;

        public SymbolPiece Carried { get; private set; }
        public bool IsCarrying => Carried != null;
        public Transform HoldPoint => holdPoint;

        ISymbolInteractable target;
        string prompt;
        readonly RaycastHit[] hits = new RaycastHit[16];

        void Update()
        {
            FindTarget();
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame) TryInteract();
        }

        /// <summary>Same as pressing the interact key (public for tests and other input setups).</summary>
        public bool TryInteract()
        {
            FindTarget();
            if (target != null && prompt != null)
            {
                target.Interact(this);
                return true;
            }
            if (IsCarrying)
            {
                Drop();
                return true;
            }
            return false;
        }

        public void PickUp(SymbolPiece piece)
        {
            if (IsCarrying || piece == null) return;
            Carried = piece;
            piece.AttachTo(holdPoint != null ? holdPoint : viewCamera);
        }

        /// <summary>Lets go of the carried piece without dropping it (e.g. it went into a socket).</summary>
        public SymbolPiece Release()
        {
            var piece = Carried;
            Carried = null;
            return piece;
        }

        public void Drop()
        {
            var piece = Release();
            if (piece == null) return;
            Vector3 pos = viewCamera.position + viewCamera.forward * 0.7f;
            piece.DropAt(pos, Quaternion.LookRotation(Vector3.ProjectOnPlane(viewCamera.forward, Vector3.up)));
        }

        void FindTarget()
        {
            target = null;
            prompt = null;
            if (viewCamera == null) return;

            int count = Physics.RaycastNonAlloc(viewCamera.position, viewCamera.forward, hits, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, 0, count, HitDistanceComparer.Instance);
            for (int i = 0; i < count; i++)
            {
                Collider col = hits[i].collider;
                if (col.transform.IsChildOf(transform)) continue;                 // our own body / held piece
                var interactable = col.GetComponentInParent<ISymbolInteractable>();
                if (interactable != null)
                {
                    target = interactable;
                    prompt = interactable.GetPrompt(this);
                    return;
                }
                if (!col.isTrigger) return;                                        // a wall blocks the view
            }
        }

        void OnGUI()
        {
            if (!showDebugPrompt) return;
            string text = prompt ?? (IsCarrying ? $"[E] {Carried.Shape.DisplayName()} 조각 내려놓기" : null);
            if (text == null) return;

            int fontSize = Mathf.Max(18, Mathf.RoundToInt(Screen.height * promptSize));
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerCenter, fontStyle = FontStyle.Bold };
                promptStyle.normal.textColor = Color.white;
                shadowStyle = new GUIStyle(promptStyle);
                shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            }
            promptStyle.fontSize = shadowStyle.fontSize = fontSize;

            // Bottom-centre of the screen, with a drop shadow so it reads on any background.
            float height = fontSize * 1.6f;
            var rect = new Rect(0f, Screen.height * (1f - promptBottomMargin) - height, Screen.width, height);
            int s = Mathf.Max(2, fontSize / 14);
            GUI.Label(new Rect(rect.x + s, rect.y + s, rect.width, rect.height), text, shadowStyle);
            GUI.Label(rect, text, promptStyle);
        }

        sealed class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitDistanceComparer Instance = new HitDistanceComparer();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }
}
