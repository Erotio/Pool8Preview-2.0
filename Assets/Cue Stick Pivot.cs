using UnityEngine;

public class CueStickPivot : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The cue ball transform this pivot should follow")]
    public Transform cueBall;

    [Header("Rotation Settings")]
    [Tooltip("Degrees per second when rotating with keyboard input")]
    public float rotationSpeed = 90f;

    [Tooltip("Degrees per unit of mouse movement (if using mouse aiming)")]
    public float mouseSensitivity = 3f;

    [Header("Input Mode")]
    public bool useMouseAim = true;

    void Update()
    {
        FollowBall();
        HandleRotation();
    }


    private void FollowBall()
    {
        if (cueBall == null) return;

        transform.position = cueBall.position;
    }

    private void HandleRotation()
    {
        float rotationAmount = 0f;

        if (useMouseAim)
        {
            // Rotate based on horizontal mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            rotationAmount = mouseX * mouseSensitivity;
        }
        else
        {
            // Rotate using keyboard (A/D or Left/Right arrows)
            float horizontalInput = Input.GetAxis("Horizontal");
            rotationAmount = horizontalInput * rotationSpeed * Time.deltaTime;
        }

        transform.Rotate(Vector3.up, rotationAmount, Space.World);
    }
}