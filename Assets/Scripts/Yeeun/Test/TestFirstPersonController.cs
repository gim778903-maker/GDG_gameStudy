using UnityEngine;
using UnityEngine.InputSystem;

namespace Yeeun.Test
{
    /// <summary>
    /// Minimal first-person controller for testing puzzles in personal test scenes.
    /// Not the final player controller. WASD move, mouse look, Shift run,
    /// click to lock the cursor, Esc to unlock.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class TestFirstPersonController : MonoBehaviour
    {
        [SerializeField] Transform cameraPivot;
        [SerializeField] float walkSpeed = 3f;
        [SerializeField] float runSpeed = 5.5f;
        [SerializeField] float lookSensitivity = 0.12f;
        [SerializeField] float gravity = -20f;

        CharacterController controller;
        float pitch;
        float verticalVelocity;

        void Awake() => controller = GetComponent<CharacterController>();

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame) Cursor.lockState = CursorLockMode.Locked;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;

            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
                transform.Rotate(0f, look.x, 0f);
                pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
                if (cameraPivot != null) cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
            }

            Vector2 input = Vector2.zero;
            bool run = false;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed) input.x -= 1f;
                run = keyboard.leftShiftKey.isPressed;
            }

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            if (move.sqrMagnitude > 1f) move.Normalize();
            move *= run ? runSpeed : walkSpeed;

            verticalVelocity = controller.isGrounded ? -2f : verticalVelocity + gravity * Time.deltaTime;
            move.y = verticalVelocity;
            controller.Move(move * Time.deltaTime);
        }

        /// <summary>Moves the player instantly (for tests); keeps the CharacterController in sync.</summary>
        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }
    }
}
