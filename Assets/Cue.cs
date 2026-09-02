using UnityEngine;

/// <summary>
/// Handles cue positioning, mouse-look aiming, charging and striking.
/// The cue orients itself toward the mouse cursor's world position
/// (raycast against the table plane), instead of accumulating a
/// relative rotation from mouse delta.
/// </summary>
public class Cue : MonoBehaviour
{
    [Header("References")]
    public Transform cueBall;
    [Tooltip("Child object holding the cue stick graphics only.")]
    public Transform stickVisual;
    [Tooltip("Reports whether every ball on the table is at rest.")]
    public Balls balls;
    [Tooltip("Leave empty to use Camera.main.")]
    public Camera aimCamera;

    [Header("Aiming")]
    [Tooltip("0 = snap instantly to the mouse direction. >0 = smoothed rotation speed.")]
    public float rotationSmoothing = 0f;

    [Header("Visual offsets")]
    public float ballRadius = 0.06f;
    public float tipGap = 0.02f;
    public float stickRestPullBack = 0.35f;
    public float maxPullBack = 0.9f;

    [Header("Striking")]
    public float minForce = 2f;
    public float maxForce = 25f;
    public float chargeSpeed = 18f;
    public float strikeAnimTime = 0.08f;

    private Rigidbody cueBallRb;
    private float currentForce;
    private bool isCharging;
    private bool isStriking;
    private float strikeTimer;
    private Vector3 strikeVisualStart;

    void Awake()
    {
        if (aimCamera == null) aimCamera = Camera.main;
        if (cueBall != null) cueBallRb = cueBall.GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool canStrike = balls == null || balls.AllBallsStopped;

        if (!canStrike)
        {
            SetVisualActive(false);
            isCharging = false;
            isStriking = false;
            return;
        }

        if (isStriking)
        {
            AnimateStrike();
            return;
        }

        FollowBall();
        AimAtMouse();
        SetVisualActive(true);
        HandleInput();
        UpdateVisualPullBack();
    }

    private void FollowBall()
    {
        if (cueBall == null) return;
        transform.position = cueBall.position;
    }

    /// <summary>
    /// Rotates the cue so it points from the cue ball toward wherever the
    /// mouse cursor is on the table plane - the "look at cursor" scheme
    /// instead of relative mouse-delta rotation.
    /// </summary>
    private void AimAtMouse()
    {
        if (cueBall == null || aimCamera == null) return;

        Plane tablePlane = new Plane(Vector3.up, cueBall.position);
        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (!tablePlane.Raycast(ray, out float hitDistance)) return;

        // Depth of the plane hit along the camera's view, needed so
        // ScreenToWorldPoint resolves to the same point the ray found.
        Vector3 rayHit = ray.GetPoint(hitDistance);
        float depth = Vector3.Distance(aimCamera.transform.position, rayHit);

        Vector3 worldPoint = aimCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));

        Vector3 direction = worldPoint - cueBall.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = rotationSmoothing > 0f
            ? Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime)
            : targetRotation;
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentForce = minForce;
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            currentForce = Mathf.Min(currentForce + chargeSpeed * Time.deltaTime, maxForce);
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            isCharging = false;
            Strike(currentForce);
        }
    }

    private void UpdateVisualPullBack()
    {
        if (stickVisual == null) return;

        float chargeT = isCharging ? Mathf.InverseLerp(minForce, maxForce, currentForce) : 0f;
        float pullBack = stickRestPullBack + chargeT * maxPullBack;
        float zOffset = -(ballRadius + tipGap + pullBack);

        stickVisual.localPosition = new Vector3(0f, 0f, zOffset);
    }

    private void Strike(float force)
    {
        if (cueBallRb == null) return;

        cueBallRb.AddForce(transform.forward * force, ForceMode.Impulse);

        isStriking = true;
        strikeTimer = 0f;
        strikeVisualStart = stickVisual != null ? stickVisual.localPosition : Vector3.zero;
    }

    private void AnimateStrike()
    {
        strikeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(strikeTimer / strikeAnimTime);

        if (stickVisual != null)
        {
            Vector3 forwardPoint = new Vector3(0f, 0f, -(ballRadius + tipGap));
            stickVisual.localPosition = Vector3.Lerp(strikeVisualStart, forwardPoint, t);
        }

        if (t >= 1f)
        {
            isStriking = false;
            SetVisualActive(false);
        }
    }

    private void SetVisualActive(bool active)
    {
        if (stickVisual != null && stickVisual.gameObject.activeSelf != active)
            stickVisual.gameObject.SetActive(active);
    }
}