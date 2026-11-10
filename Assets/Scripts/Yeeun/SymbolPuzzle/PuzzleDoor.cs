using UnityEngine;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Simple door that slides by <see cref="openOffset"/> when opened.
    /// Hook <see cref="Open"/> to a puzzle's onSolved event.
    /// </summary>
    public class PuzzleDoor : MonoBehaviour
    {
        [SerializeField] Vector3 openOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] float speed = 1.5f;
        [SerializeField] bool startOpen;

        public bool IsOpen { get; private set; }

        Vector3 closedPos;

        void Awake()
        {
            closedPos = transform.localPosition;
            IsOpen = startOpen;
            if (startOpen) transform.localPosition = closedPos + openOffset;
        }

        void Update()
        {
            Vector3 target = IsOpen ? closedPos + openOffset : closedPos;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
        }

        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;
        public void Toggle() => IsOpen = !IsOpen;
    }
}
