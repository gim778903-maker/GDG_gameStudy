using UnityEngine;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// A raised symbol on a wall. It is the clue: its shape only reads as an outline.
    /// The puzzle lights it up once the matching plate has been pressed.
    /// </summary>
    public class SymbolRelief : SymbolVisual
    {
        [SerializeField] Color matchedColor = new Color(0.45f, 0.9f, 1f);

        public bool IsMatched { get; private set; }

        public void SetMatched(bool matched)
        {
            IsMatched = matched;
            SetTint(matched ? matchedColor : (Color?)null);
        }
    }
}
