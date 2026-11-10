using UnityEngine;
using UnityEngine.Events;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Variant 1: collect symbol pieces around the maze and fit them into the matching
    /// holes on a door. Fires <see cref="onSolved"/> once every socket is filled.
    /// </summary>
    public class SymbolSocketPuzzle : MonoBehaviour
    {
        [SerializeField] SymbolSocket[] sockets = new SymbolSocket[0];

        [Tooltip("Called once all sockets hold their piece (e.g. PuzzleDoor.Open).")]
        public UnityEvent onSolved = new UnityEvent();
        [Tooltip("Called when a piece with the wrong shape is tried (e.g. noise that draws a monster).")]
        public UnityEvent onWrong = new UnityEvent();
        [Tooltip("Called after each piece is fitted, with the number of filled sockets.")]
        public UnityEvent<int> onProgress = new UnityEvent<int>();

        public bool IsSolved { get; private set; }

        public int FilledCount
        {
            get
            {
                int n = 0;
                foreach (var s in sockets) if (s != null && s.IsFilled) n++;
                return n;
            }
        }

        public int Length => sockets.Length;

        void OnEnable()
        {
            foreach (var s in sockets)
            {
                if (s == null) continue;
                s.Filled += HandleFilled;
                s.Rejected += HandleRejected;
            }
        }

        void OnDisable()
        {
            foreach (var s in sockets)
            {
                if (s == null) continue;
                s.Filled -= HandleFilled;
                s.Rejected -= HandleRejected;
            }
        }

        void HandleFilled(SymbolSocket socket)
        {
            int filled = FilledCount;
            onProgress.Invoke(filled);
            if (!IsSolved && filled == sockets.Length)
            {
                IsSolved = true;
                onSolved.Invoke();
            }
        }

        void HandleRejected(SymbolSocket socket) => onWrong.Invoke();
    }
}
