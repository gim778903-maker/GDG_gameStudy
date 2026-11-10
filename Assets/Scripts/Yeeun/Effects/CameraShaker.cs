using System.Collections.Generic;
using UnityEngine;

namespace Yeeun.Effects
{
    /// <summary>
    /// Small positional camera shake. Put it on the camera (or its pivot) and call
    /// <see cref="ShakeAll"/> from anywhere, e.g. when a heavy door thuds.
    /// Only the local position is offset, so it does not fight mouse-look rotation.
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        static readonly List<CameraShaker> Active = new List<CameraShaker>();

        [SerializeField] float frequency = 28f;

        Vector3 basePos;
        float amplitude;
        float duration;
        float elapsed;
        float seed;

        void Awake()
        {
            basePos = transform.localPosition;
            seed = Random.value * 100f;
        }

        void OnEnable() => Active.Add(this);

        void OnDisable()
        {
            Active.Remove(this);
            transform.localPosition = basePos;
            amplitude = 0f;
        }

        /// <summary>Shakes every active camera shaker (amplitude in metres).</summary>
        public static void ShakeAll(float amplitude, float duration)
        {
            foreach (var s in Active) s.Shake(amplitude, duration);
        }

        public void Shake(float newAmplitude, float newDuration)
        {
            // Keep the stronger of the running and the new shake.
            float remaining = amplitude * Mathf.Clamp01(1f - elapsed / Mathf.Max(duration, 0.0001f));
            if (newAmplitude < remaining) return;
            amplitude = newAmplitude;
            duration = newDuration;
            elapsed = 0f;
        }

        void LateUpdate()
        {
            if (amplitude <= 0f) return;
            elapsed += Time.deltaTime;
            float fade = 1f - elapsed / duration;
            if (fade <= 0f)
            {
                amplitude = 0f;
                transform.localPosition = basePos;
                return;
            }
            float t = Time.time * frequency;
            var offset = new Vector3(
                Mathf.PerlinNoise(seed, t) - 0.5f,
                Mathf.PerlinNoise(seed + 10f, t) - 0.5f,
                0f) * (2f * amplitude * fade * fade);
            transform.localPosition = basePos + offset;
        }
    }
}
