using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Symbol sequence puzzle ("모양 맞추기").
    /// Place A: wall reliefs (<see cref="clues"/>) show the answer order, e.g. ○ → △ → □.
    /// Place B (can be far away): floor plates with mixed symbols, including decoys (e.g. ×).
    /// Stepping on the plates in the clue order fires <see cref="onSolved"/> (open the exit door).
    /// Any wrong plate resets the plates and fires <see cref="onWrong"/>.
    /// </summary>
    public class SymbolSequencePuzzle : MonoBehaviour
    {
        [Tooltip("Wall reliefs in answer order (place A).")]
        [SerializeField] SymbolRelief[] clues = new SymbolRelief[0];
        [Tooltip("Every plate that belongs to this puzzle, including decoys (place B).")]
        [SerializeField] SymbolPlate[] plates = new SymbolPlate[0];
        [Tooltip("Light up the matching wall relief after each correct plate. Off by default: " +
                 "place A is usually out of sight, and lit clues would make memorising pointless.")]
        [SerializeField] bool highlightClues;

        [Tooltip("Called once when the whole sequence is matched (e.g. PuzzleDoor.Open).")]
        public UnityEvent onSolved = new UnityEvent();
        [Tooltip("Called when a wrong plate is pressed. The plates are reset.")]
        public UnityEvent onWrong = new UnityEvent();
        [Tooltip("Called after each correct plate with the number of matched symbols.")]
        public UnityEvent<int> onProgress = new UnityEvent<int>();

        public int Progress { get; private set; }
        public bool IsSolved { get; private set; }
        public int Length => clues.Length;

        void OnEnable()
        {
            foreach (var plate in plates)
                if (plate != null) plate.Pressed += HandlePlatePressed;
        }

        void OnDisable()
        {
            foreach (var plate in plates)
                if (plate != null) plate.Pressed -= HandlePlatePressed;
        }

        void Start() => ResetPuzzle();

        void HandlePlatePressed(SymbolPlate plate)
        {
            if (IsSolved || clues.Length == 0) return;

            SymbolRelief expected = clues[Progress];
            if (expected != null && plate.Shape == expected.Shape)
            {
                plate.SetFeedback(SymbolPlate.Feedback.Correct);
                if (highlightClues) expected.SetMatched(true);
                Progress++;
                onProgress.Invoke(Progress);

                if (Progress >= clues.Length)
                {
                    IsSolved = true;
                    onSolved.Invoke();
                }
                return;
            }

            ResetPuzzle();
            plate.SetFeedback(SymbolPlate.Feedback.Wrong);
            onWrong.Invoke();
        }

        /// <summary>Clears progress and all highlights.</summary>
        public void ResetPuzzle()
        {
            Progress = 0;
            IsSolved = false;
            foreach (var clue in clues)
                if (clue != null) clue.SetMatched(false);
            foreach (var plate in plates)
                if (plate != null) plate.SetFeedback(SymbolPlate.Feedback.None);
        }
    }
}
