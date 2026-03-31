using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopDebugCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float boostMultiplier = 5f;
    
    [Header("Look")]
    public float lookSpeed = 0.15f;

    private float pitch = 0f;
    private float yaw = 0f;

    private void Start()
    {
        // Initialer Cursor-Lock für ungestörtes Trackpad/Maus-Feedback
        LockCursor(true);
        
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        HandleCursorState();

        // Nur steuern, wenn der Cursor gelockt ist (verhindert versehentliches Drehen bei UI-Interaktion)
        if (Cursor.lockState != CursorLockMode.Locked) return;

        HandleRotation();
        HandleMovement();
    }

    private void HandleCursorState()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor(true);
        }
    }

    private void LockCursor(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    private void HandleRotation()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        yaw += mouseDelta.x * lookSpeed;
        pitch -= mouseDelta.y * lookSpeed;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        // WASD für horizontale Ebene, Q/E für vertikale Achse (Höhe)
        if (Keyboard.current.wKey.isPressed) moveDir += transform.forward;
        if (Keyboard.current.sKey.isPressed) moveDir -= transform.forward;
        if (Keyboard.current.aKey.isPressed) moveDir -= transform.right;
        if (Keyboard.current.dKey.isPressed) moveDir += transform.right;
        if (Keyboard.current.eKey.isPressed) moveDir += Vector3.up;
        if (Keyboard.current.qKey.isPressed) moveDir -= Vector3.up;

        float currentSpeed = moveSpeed;
        if (Keyboard.current.leftShiftKey.isPressed) currentSpeed *= boostMultiplier;

        // Bewegung normalisieren, um bei diagonalem Input nicht schneller zu sein
        transform.position += moveDir.normalized * (currentSpeed * Time.deltaTime);
    }
}