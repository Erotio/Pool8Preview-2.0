using UnityEngine;

/// <summary>
/// Add this as a SECOND component on the same GameObject that already has
/// CueStickPivot. It does NOT rotate or reposition anything itself - it reads
/// the rotation CueStickPivot already produced (transform.forward) and only
/// adds: the visual pull-back/thrust animation, charging, striking, and
/// gating everything behind "are the balls actually stopped".
/// </summary>
[RequireComponent(typeof(CueStickPivot))]
public class CueStickStriker : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Child object holding the cue stick graphics only.")]
    public Transform stickVisual;
    [Tooltip("Reports whether every ball on the table is at rest.")]
    public BallsRestChecker restChecker;

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

    private CueStickPivot pivot;
    private Rigidbody cueBallRb;
    private float currentForce;
    private bool isCharging;
    private bool isStriking;
    private float strikeTimer;
    private Vector3 strikeVisualStart;

    void Awake()
    {
        pivot = GetComponent<CueStickPivot>();
        if (pivot.cueBall != null)
            cueBallRb = pivot.cueBall.GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool canStrike = restChecker == null || restChecker.AllBallsStopped;

        // Freeze your existing aim/rotation script while balls are moving,
        // and hide the stick entirely.
        pivot.enabled = canStrike && !isStriking;

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

        SetVisualActive(true);
        HandleInput();
        UpdateVisualPullBack();
    }

    void HandleInput()
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

    void UpdateVisualPullBack()
    {
        if (stickVisual == null) return;

        float chargeT = isCharging ? Mathf.InverseLerp(minForce, maxForce, currentForce) : 0f;
        float pullBack = stickRestPullBack + chargeT * maxPullBack;
        float zOffset = -(ballRadius + tipGap + pullBack);

        stickVisual.localPosition = new Vector3(0f, 0f, zOffset);
    }

    /// <summary>
    /// Direction comes from THIS transform's forward - i.e. whatever rotation
    /// your CueStickPivot script already set via mouse/keyboard input.
    /// </summary>
    void Strike(float force)
    {
        if (cueBallRb == null) return;

        cueBallRb.AddForce(transform.forward * force, ForceMode.Impulse);

        isStriking = true;
        strikeTimer = 0f;
        strikeVisualStart = stickVisual != null ? stickVisual.localPosition : Vector3.zero;
    }

    void AnimateStrike()
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

    void SetVisualActive(bool active)
    {
        if (stickVisual != null && stickVisual.gameObject.activeSelf != active)
            stickVisual.gameObject.SetActive(active);
    }
}
