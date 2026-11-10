using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Variant 2: turn the pillars so their front symbols match the hint wall, left to right
    /// (<see cref="clues"/>[i] is the target for <see cref="pillars"/>[i]).
    /// Matching alone does nothing: the player has to press the door button, which calls
    /// <see cref="Submit"/>. Right → <see cref="onSolved"/> (open the door, pillars lock).
    /// Wrong → every pillar turns back to its start position and <see cref="onWrong"/> fires.
    /// </summary>
    public class SymbolPillarPuzzle : MonoBehaviour
    {
        [Tooltip("Pillars from left to right (as the player faces them).")]
        [SerializeField] SymbolPillar[] pillars = new SymbolPillar[0];
        [Tooltip("Hint wall reliefs, same order as the pillars.")]
        [SerializeField] SymbolRelief[] clues = new SymbolRelief[0];

        [Tooltip("Called when the button is pressed with the right arrangement (e.g. PuzzleDoor.Open).")]
        public UnityEvent onSolved = new UnityEvent();
        [Tooltip("Called when the button is pressed with a wrong arrangement; the pillars reset.")]
        public UnityEvent onWrong = new UnityEvent();

        public bool IsSolved { get; private set; }

        /// <summary>True when every pillar's front symbol equals its clue.</summary>
        public bool Matches()
        {
            if (pillars.Length == 0 || pillars.Length != clues.Length) return false;
            for (int i = 0; i < pillars.Length; i++)
                if (pillars[i] == null || clues[i] == null || pillars[i].FrontShape != clues[i].Shape)
                    return false;
            return true;
        }

        /// <summary>Checks the arrangement (call from the "문 열기" button).</summary>
        public void Submit()
        {
            if (IsSolved) return;
            foreach (var p in pillars)
                if (p != null && p.IsTurning) return; // wait until every pillar has stopped

            if (Matches())
            {
                IsSolved = true;
                foreach (var p in pillars) p.IsLocked = true;
                onSolved.Invoke();
                return;
            }

            ResetPillars();
            onWrong.Invoke();
        }

        /// <summary>Turns every pillar back to where it started.</summary>
        public void ResetPillars()
        {
            foreach (var p in pillars)
                if (p != null) p.ResetTurns();
        }
    }
}
