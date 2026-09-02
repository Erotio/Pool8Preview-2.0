using System.Collections.Generic;
using UnityEngine;

public class Balls : MonoBehaviour
{
    [Tooltip("Balls to track. Leave empty to auto-collect every Rigidbody tagged 'Ball'.")]
    public List<Rigidbody> balls = new List<Rigidbody>();

    [Header("Rest detection")]
    public float velocityThreshold = 0.03f;
    public float angularVelocityThreshold = 0.05f;
    public float holdTime = 0.25f;

    public bool AllBallsStopped { get; private set; } = true;
    private float slowTimer;

    void Awake()
    {
        if (balls.Count == 0)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Ball"))
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null) balls.Add(rb);
            }
        }
    }

    void FixedUpdate()
    {
        bool allSlow = true;
        foreach (Rigidbody rb in balls)
        {
            if (rb == null) continue;
            if (rb.linearVelocity.sqrMagnitude > velocityThreshold * velocityThreshold ||
                rb.angularVelocity.sqrMagnitude > angularVelocityThreshold * angularVelocityThreshold)
            {
                allSlow = false;
                break;
            }
        }

        if (allSlow)
        {
            slowTimer += Time.fixedDeltaTime;
            if (slowTimer >= holdTime)
            {
                ForceStopAll();
                AllBallsStopped = true;
            }
        }
        else
        {
            slowTimer = 0f;
            AllBallsStopped = false;
        }
    }

    void ForceStopAll()
    {
        foreach (Rigidbody rb in balls)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}