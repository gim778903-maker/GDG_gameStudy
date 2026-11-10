using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// A shaped hole ("홈") on a door or wall (variant 1). Accepts only a
    /// <see cref="SymbolPiece"/> with the same shape; a wrong piece is refused.
    /// The symbol mesh is shown in a dark material so it reads as a recess.
    /// </summary>
    public class SymbolSocket : SymbolVisual, ISymbolInteractable
    {
        [Tooltip("Where an inserted piece sits (defaults to this transform).")]
        [SerializeField] Transform pieceAnchor;
        [SerializeField] Color rejectColor = new Color(1f, 0.25f, 0.2f);
        [SerializeField] float rejectFlashTime = 0.5f;

        public UnityEvent onFilled = new UnityEvent();
        [Tooltip("Called when the player tries a piece with the wrong shape.")]
        public UnityEvent onRejected = new UnityEvent();

        public event Action<SymbolSocket> Filled;
        public event Action<SymbolSocket> Rejected;

        public SymbolPiece Inserted { get; private set; }
        public bool IsFilled => Inserted != null;

        public string GetPrompt(SymbolInteractor interactor)
        {
            if (IsFilled) return null;
            return interactor.IsCarrying
                ? $"[E] {interactor.Carried.Shape.DisplayName()} 조각 끼우기"
                : $"{Shape.DisplayName()} 모양 홈";
        }

        public void Interact(SymbolInteractor interactor)
        {
            if (IsFilled || !interactor.IsCarrying) return;
            if (TryInsert(interactor.Carried)) interactor.Release();
        }

        /// <summary>Inserts the piece if the shape matches. Returns false (and flashes) otherwise.</summary>
        public bool TryInsert(SymbolPiece piece)
        {
            if (IsFilled || piece == null) return false;
            if (piece.Shape != Shape)
            {
                StopAllCoroutines();
                StartCoroutine(FlashReject());
                onRejected.Invoke();
                Rejected?.Invoke(this);
                return false;
            }

            Inserted = piece;
            piece.LockInto(pieceAnchor != null ? pieceAnchor : transform);
            onFilled.Invoke();
            Filled?.Invoke(this);
            return true;
        }

        IEnumerator FlashReject()
        {
            SetTint(rejectColor);
            yield return new WaitForSeconds(rejectFlashTime);
            SetTint(null);
        }
    }
}
