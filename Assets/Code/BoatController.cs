using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Rowing Settings")]
    public float rowForce = 5f;          // target added speed per stroke
    public float maxSpeed = 5f;          // max forward speed
    public float drag = 1f;              // slowdown rate
    public float strokeDuration = 0.3f;  // how long it takes to "dig in" (in seconds)

    private float currentSpeed = 0f;
    private bool expectingLeft = true;

    private bool isStroking = false;
    private float strokeTimer = 0f;
    private float strokeTarget = 0f;

    void Update()
    {
        HandleInput();
        UpdateStroke();
        ApplyMovement();
    }

    void HandleInput()
    {
        if (isStroking) return; // can’t start a new stroke mid-stroke

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
        // Start a new rowing stroke
        isStroking = true;
        strokeTimer = 0f;
        strokeTarget = Mathf.Clamp(currentSpeed + rowForce, 0f, maxSpeed);
    }

    void UpdateStroke()
    {
        if (!isStroking) return;

        strokeTimer += Time.deltaTime;
        float t = strokeTimer / strokeDuration;

        // Smooth ease-in curve (starts slow, builds power)
        float curve = Mathf.SmoothStep(0f, 1f, t);

        // Interpolate current speed toward the stroke target
        currentSpeed = Mathf.Lerp(currentSpeed, strokeTarget, curve * Time.deltaTime * 5f);

        if (strokeTimer >= strokeDuration)
        {
            isStroking = false;
        }
    }

    void ApplyMovement()
    {
        // Move the boat to the right
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;

        // Apply drag (momentum loss)
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Time.deltaTime);
    }
}
