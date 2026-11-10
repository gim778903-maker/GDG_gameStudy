using UnityEngine;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// A loose symbol piece (variant 1) that the player picks up and fits into a
    /// <see cref="SymbolSocket"/> of the same shape.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SymbolPiece : SymbolVisual, ISymbolInteractable
    {
        Rigidbody body;
        Collider[] colliders;

        /// <summary>True once the piece sits in a socket; it can no longer be picked up.</summary>
        public bool IsLocked { get; private set; }
        public bool IsHeld { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            body = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public string GetPrompt(SymbolInteractor interactor)
        {
            if (IsLocked || IsHeld || interactor.IsCarrying) return null;
            return $"[E] {Shape.DisplayName()} 조각 줍기";
        }

        public void Interact(SymbolInteractor interactor) => interactor.PickUp(this);

        /// <summary>Holds the piece in front of the camera (face towards the viewer).</summary>
        public void AttachTo(Transform holder)
        {
            IsHeld = true;
            SetPhysics(false);
            transform.SetParent(holder, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        public void DropAt(Vector3 position, Quaternion rotation)
        {
            IsHeld = false;
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(position, rotation);
            SetPhysics(true);
        }

        /// <summary>Snaps into a socket and stays there.</summary>
        public void LockInto(Transform socket)
        {
            IsHeld = false;
            IsLocked = true;
            SetPhysics(false);
            transform.SetParent(socket, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        void SetPhysics(bool on)
        {
            body.isKinematic = !on;
            foreach (var c in colliders) c.enabled = on;
        }
    }
}
