using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    [SerializeField] private float mouseSensitivity = 0.05f;
    
    [Header("Vertical Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Mouse mouse;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        mouse = Mouse.current;
        
        Vector3 currentRotation = transform.eulerAngles;
        rotationX = currentRotation.x;
        rotationY = currentRotation.y;
        
        if (rotationX > 180f)
            rotationX -= 360f;
    }
    
    void Update()
    {
        if (mouse == null) return;
        
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;
        
        rotationY += mouseX;
        rotationX -= mouseY;
        
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
