using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private bool useSensitivityManager = true;
    
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
        
        // Force horizontal initial rotation (looking at horizon)
        rotationX = 0f;  // No vertical angle (horizontal)
        rotationY = 0f;  // No horizontal rotation (forward)
        
        // Apply the initial rotation immediately
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
    
    void Update()
    {
        if (mouse == null) return;
        
        Vector2 mouseDelta = mouse.delta.ReadValue();
        
        // Get sensitivity from manager or use local value
        float currentSensitivity = mouseSensitivity;
        if (useSensitivityManager && MouseSensitivityManager.Instance != null)
        {
            // Convert manager sensitivity (0.5-10) to camera sensitivity (0.01-0.2)
            currentSensitivity = MouseSensitivityManager.Instance.GetSensitivity() * 0.02f;
        }
        
        float mouseX = mouseDelta.x * currentSensitivity;
        float mouseY = mouseDelta.y * currentSensitivity;
        
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
