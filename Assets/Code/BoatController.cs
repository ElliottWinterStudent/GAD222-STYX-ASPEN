using UnityEngine;
using System.Collections;

public class BoatController : MonoBehaviour
{
    [Header("Rowing Settings")]
    public float rowForce = 5f;
    public float maxSpeed = 5f;
    public float drag = 1f;
    public float strokeDuration = 0.3f;

    private float currentSpeed = 0f;
    private bool expectingLeft = true;

    private bool isStroking = false;
    private float strokeTimer = 0f;
    private float strokeTarget = 0f;

    private bool externallyPaused = false;
    private bool isSlowingToStop = false;

    [Header("Slow/Stop Settings")]
    public float slowStopDragMultiplier = 4f;
    public float slowStopMinSpeed = 0.02f;

    public float CurrentSpeed => currentSpeed;

    void Update()
    {
        if (!externallyPaused)
        {
            HandleInput();
            UpdateStroke();
        }

        ApplyMovement();
    }

    void HandleInput()
    {
        if (isStroking) return;

        if (expectingLeft && Input.GetKeyDown(KeyCode.A))
        {
            BeginStroke();
            expectingLeft = false;
        }
        else if (!expectingLeft && Input.GetKeyDown(KeyCode.D))
        {
            BeginStroke();
            expectingLeft = true;
        }
    }

    void BeginStroke()
    {
        isStroking = true;
        strokeTimer = 0f;
        strokeTarget = Mathf.Clamp(currentSpeed + rowForce, 0f, maxSpeed);
    }

    void UpdateStroke()
    {
        if (!isStroking) return;

        strokeTimer += Time.deltaTime;
        float t = strokeTimer / strokeDuration;
        float curve = Mathf.SmoothStep(0f, 1f, t);

        currentSpeed = Mathf.Lerp(currentSpeed, strokeTarget, curve * Time.deltaTime * 5f);

        if (strokeTimer >= strokeDuration)
            isStroking = false;
    }

    void ApplyMovement()
    {
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;

        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Time.deltaTime);
    }

    public void PauseControl()
    {
        externallyPaused = true;
    }

    public void ResumeControl()
    {
        externallyPaused = false;
        isStroking = false;
    }

    public void RequestSlowStop(System.Action onStopped)
    {
        if (!isSlowingToStop)
            StartCoroutine(SlowStopRoutine(onStopped, slowStopDragMultiplier, slowStopMinSpeed));
    }

    IEnumerator SlowStopRoutine(System.Action onStopped, float stopDragMultiplier, float minSpeed)
    {
        isSlowingToStop = true;
        externallyPaused = true;
        isStroking = false;

        while (currentSpeed > minSpeed)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                drag * stopDragMultiplier * Time.deltaTime
            );

            transform.position += Vector3.right * currentSpeed * Time.deltaTime;

            yield return null;
        }

        currentSpeed = 0f;
        isSlowingToStop = false;
        onStopped?.Invoke();
    }
}
