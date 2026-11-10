using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Yeeun.Effects;
using Yeeun.SymbolPuzzle;

namespace Yeeun.HiddenPassage
{
    /// <summary>
    /// Hidden passage / fake wall. The door part is flush with the wall and only its seam
    /// lines give it away. Sequence (about 1.5 s):
    ///   1. Approach: within <see cref="approachRadius"/> the seam flickers and glitches.
    ///      The player sees "[E] 밀기" (through <see cref="SymbolInteractor"/>).
    ///   2. Push: the door thuds <see cref="pushDepth"/> into the wall; short depth lines
    ///      appear between the opening and the door corners; the camera shakes.
    ///   3. Pause for a beat.
    ///   4. Slide: the door grinds sideways into the neighbouring wall while dust falls.
    ///   5. Stays open.
    /// The root's +Z is the front (the side the player comes from); the door slides along
    /// local X (<see cref="slideDirection"/>), so a solid wall must sit on that side.
    /// </summary>
    public class FakeWall : MonoBehaviour, ISymbolInteractable
    {
        public enum State { Closed, Pushing, Paused, Sliding, Open }

        [Header("Parts")]
        [SerializeField] Transform door;
        [SerializeField] Collider doorCollider;
        [Tooltip("Outline of the opening (left, top, right edges).")]
        [SerializeField] LineRenderer seam;
        [Tooltip("Outline of the door itself (child of the door, moves with it).")]
        [SerializeField] LineRenderer doorOutline;
        [Tooltip("Short depth lines from the opening's top corners to the pushed-in door.")]
        [SerializeField] LineRenderer[] depthLines = new LineRenderer[2];
        [SerializeField] ParticleSystem dust;

        [Header("Opening (local, front face at z = 0)")]
        [SerializeField] float openingWidth = 1.4f;
        [SerializeField] float openingHeight = 2.2f;

        [Header("Approach")]
        [SerializeField] string playerTag = "Player";
        [SerializeField] float approachRadius = 3.5f;
        [Tooltip("Seam colour when it is 'off' (should match the wall).")]
        [SerializeField] Color seamDark = new Color(0.16f, 0.16f, 0.18f);
        [SerializeField] Color seamBright = new Color(0.95f, 0.95f, 0.95f);
        [Tooltip("How visible the seam is from far away (0 = invisible).")]
        [SerializeField, Range(0f, 1f)] float idleVisibility = 0.08f;
        [SerializeField] float glitchJitter = 0.025f;
        [Tooltip("Seam brightness once the passage is open.")]
        [SerializeField, Range(0f, 1f)] float openVisibility = 0.75f;

        [Header("Timing")]
        [SerializeField] float pushDepth = 0.15f;
        [SerializeField] float pushDuration = 0.45f;
        [SerializeField] float pauseDuration = 0.2f;
        [SerializeField] float slideDistance = 1.5f;
        [Tooltip("+1 slides towards local +X, -1 towards local -X.")]
        [SerializeField] int slideDirection = 1;
        [SerializeField] float slideDuration = 0.8f;

        [Header("Feel")]
        [SerializeField] float pushShake = 0.045f;
        [SerializeField] float pushShakeTime = 0.3f;
        [SerializeField] float slideShake = 0.012f;
        [SerializeField] float slideRattle = 0.006f;
        [SerializeField] int thudDustBurst = 6;

        [Header("Events (sounds etc.)")]
        public UnityEvent onPushStarted = new UnityEvent();
        public UnityEvent onThud = new UnityEvent();
        public UnityEvent onSlideStarted = new UnityEvent();
        public UnityEvent onOpened = new UnityEvent();

        public State CurrentState { get; private set; } = State.Closed;
        public bool IsOpen => CurrentState == State.Open;
        /// <summary>0..1 seam brightness this frame (for tests / debugging).</summary>
        public float SeamVisibility { get; private set; }
        public bool PlayerNear { get; private set; }

        Vector3 doorClosedPos;
        Vector3[] seamBase;
        Transform player;
        float nextGlitch;
        int glitchFrames;

        void Awake()
        {
            if (door != null) doorClosedPos = door.localPosition;
            if (seam != null)
            {
                seamBase = new Vector3[seam.positionCount];
                seam.GetPositions(seamBase);
            }
            SetLineVisible(doorOutline, false);
            foreach (var l in depthLines) SetLineVisible(l, false);
            if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetSeam(idleVisibility, false);
        }

        void Update()
        {
            if (CurrentState != State.Closed) return;

            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) player = go.transform;
            }

            float distance = player != null
                ? Vector3.Distance(player.position, transform.TransformPoint(0f, openingHeight * 0.5f, 0f))
                : float.MaxValue;
            PlayerNear = distance <= approachRadius;
            if (!PlayerNear)
            {
                SetSeam(idleVisibility, false);
                return;
            }

            // Closer = stronger. A: flicker (noisy on/off), C: glitch (short jitter bursts).
            float closeness = 1f - Mathf.Clamp01(distance / approachRadius);
            float noise = Mathf.PerlinNoise(Time.time * 9f, 0.37f);
            float flicker = noise > 0.5f - 0.2f * closeness ? 1f : 0.25f;
            float visibility = Mathf.Lerp(0.35f, 1f, closeness) * flicker;

            if (Time.time >= nextGlitch)
            {
                glitchFrames = Random.Range(1, 4);
                nextGlitch = Time.time + Random.Range(0.06f, 0.4f) * Mathf.Lerp(1.6f, 0.6f, closeness);
            }
            bool glitch = glitchFrames > 0;
            if (glitch)
            {
                glitchFrames--;
                visibility = Random.value < 0.5f ? 1f : 0.1f;
            }
            SetSeam(visibility, glitch);
        }

        // ---------------------------------------------------------------- interaction

        public string GetPrompt(SymbolInteractor interactor) => CurrentState == State.Closed ? "[E] 밀기" : null;

        public void Interact(SymbolInteractor interactor) => Open();

        /// <summary>Plays the whole push → pause → slide sequence (ignored once started).</summary>
        public void Open()
        {
            if (CurrentState != State.Closed) return;
            StartCoroutine(OpenSequence());
        }

        IEnumerator OpenSequence()
        {
            // 1) Push in: accelerate, then stop hard ("쿵").
            CurrentState = State.Pushing;
            SetSeam(1f, false);
            SetLineVisible(doorOutline, true);
            foreach (var l in depthLines) SetLineVisible(l, true);
            onPushStarted.Invoke();

            Vector3 pushedPos = doorClosedPos + Vector3.back * pushDepth;
            for (float t = 0f; t < pushDuration; t += Time.deltaTime)
            {
                float k = t / pushDuration;
                door.localPosition = Vector3.LerpUnclamped(doorClosedPos, pushedPos, k * k * k);
                UpdateDepthLines();
                yield return null;
            }
            door.localPosition = pushedPos;
            UpdateDepthLines();
            CameraShaker.ShakeAll(pushShake, pushShakeTime);
            if (dust != null && thudDustBurst > 0) dust.Emit(thudDustBurst);
            onThud.Invoke();

            // 2) A beat of silence.
            CurrentState = State.Paused;
            yield return new WaitForSeconds(pauseDuration);

            // 3) Grind sideways into the wall.
            CurrentState = State.Sliding;
            foreach (var l in depthLines) SetLineVisible(l, false);
            if (dust != null) dust.Play();
            onSlideStarted.Invoke();
            CameraShaker.ShakeAll(slideShake, slideDuration);

            Vector3 openPos = pushedPos + Vector3.right * (slideDistance * Mathf.Sign(slideDirection));
            for (float t = 0f; t < slideDuration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / slideDuration);
                Vector3 rattle = new Vector3(0f, Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * slideRattle;
                door.localPosition = Vector3.Lerp(pushedPos, openPos, k) + rattle;
                yield return null;
            }
            door.localPosition = openPos;

            // 4) Open for good.
            if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (doorCollider != null) doorCollider.enabled = false;
            SetLineVisible(doorOutline, false);
            SetSeam(openVisibility, false);
            CurrentState = State.Open;
            onOpened.Invoke();
        }

        // ---------------------------------------------------------------- lines

        void SetSeam(float visibility, bool jitter)
        {
            SeamVisibility = visibility;
            if (seam == null) return;
            Color c = Color.Lerp(seamDark, seamBright, visibility);
            seam.startColor = seam.endColor = c;
            if (seamBase == null) return;
            for (int i = 0; i < seamBase.Length; i++)
            {
                Vector3 p = seamBase[i];
                if (jitter) p += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * glitchJitter;
                seam.SetPosition(i, p);
            }
        }

        void UpdateDepthLines()
        {
            float half = openingWidth * 0.5f;
            Vector3 offset = door.localPosition - doorClosedPos;
            for (int i = 0; i < depthLines.Length && i < 2; i++)
            {
                if (depthLines[i] == null) continue;
                Vector3 corner = new Vector3(i == 0 ? -half : half, openingHeight, 0.003f);
                depthLines[i].SetPosition(0, corner);
                depthLines[i].SetPosition(1, corner + offset);
            }
        }

        static void SetLineVisible(LineRenderer line, bool visible)
        {
            if (line != null) line.enabled = visible;
        }
    }
}
