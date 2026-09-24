using UnityEngine;

/// <summary>
/// Animates a single part's world position, world rotation, and material opacity
/// from an initial value to a final value over a fixed duration.
/// Assign this script directly on each part you want to animate.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PartAnimationController : MonoBehaviour
{
    [Header("Target Values (Inspector)")]
    [Tooltip("Position offset added to the initial position (e.g. (2,0,0) moves 2 units further along world X from the start point).")]
    public Vector3 positionOffset;

    [Tooltip("Rotation offset (Euler angles) added to the initial rotation.")]
    public Vector3 rotationOffsetEuler;

    [Tooltip("Opacity at the start of the animation (0 = transparent, 1 = opaque).")]
    [Range(0f, 1f)] public float initialOpacity = 1f;

    [Tooltip("Opacity at the end of the animation (0 = transparent, 1 = opaque).")]
    [Range(0f, 1f)] public float finalOpacity = 1f;

    [Header("Timing")]
    [Tooltip("Duration of the animation in seconds.")]
    public float duration = 1f;

    [Tooltip("Shapes how t (0-1) progresses over the duration.")]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("End Phase")]
    [Tooltip("If true, the part's Renderer and Collider (if present) are disabled after reaching the final value.")]
    public bool endPhase = false;

    [Tooltip("Delay in seconds between reaching the final value and disabling the part.")]
    public float endPhaseDelay = 0f;

    [Header("Trigger")]
    [Tooltip("Delay in seconds before the animation starts after activate is set to true. 0 = instant.")]
    public float activationDelay = 0f;

    [Tooltip("Set to true to start the animation. Automatically turns false when the animation finishes.")]
    public bool activate = false;

    [Tooltip("Set to true to reset the part back to its original position, rotation and opacity. Only works while idle (not animating, not waiting). Always turns back to false.")]
    public bool reset = false;

    // Captured at scene load
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Computed once from initial values + inspector offsets
    private Vector3 finalPosition;
    private Quaternion finalRotation;

    private Renderer rend;
    private Collider col;
    private Material materialInstance;

    private bool isAnimating = false;
    private bool isWaitingToActivate = false;
    private bool isWaitingToEndPhase = false;
    private float elapsed = 0f;

    private void Awake()
    {
        // Capture current state as the initial position/rotation
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Final = initial + offset (world space)
        finalPosition = initialPosition + positionOffset;
        finalRotation = initialRotation * Quaternion.Euler(rotationOffsetEuler);

        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>(); // may be null, guarded on use
        materialInstance = rend.material; // Unity auto-instances this material

        SetOpacity(initialOpacity);
    }

    private void Update()
    {
        if (reset)
        {
            bool isIdle = !isAnimating && !isWaitingToActivate && !isWaitingToEndPhase;

            if (isIdle)
            {
                ResetToInitial();
            }

            reset = false;
        }

        if (activate && !isAnimating && !isWaitingToActivate)
        {
            if (activationDelay > 0f)
            {
                isWaitingToActivate = true;
                StartCoroutine(ActivateAfterDelay());
            }
            else
            {
                BeginAnimation();
            }
        }

        if (!isAnimating)
            return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float curvedT = easingCurve.Evaluate(t);

        transform.position = Vector3.Lerp(initialPosition, finalPosition, curvedT);
        transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, curvedT);
        SetOpacity(Mathf.Lerp(initialOpacity, finalOpacity, curvedT));

        if (t >= 1f)
        {
            isAnimating = false;
            activate = false;

            if (endPhase)
            {
                isWaitingToEndPhase = true;
                StartCoroutine(DisableAfterDelay());
            }
        }
    }

    private System.Collections.IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        isWaitingToActivate = false;

        // If activate was set back to false during the wait, cancel.
        if (activate)
        {
            BeginAnimation();
        }
    }

    private System.Collections.IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(endPhaseDelay);

        rend.enabled = false;
        if (col != null)
        {
            col.enabled = false;
        }

        isWaitingToEndPhase = false;
    }

    private void BeginAnimation()
    {
        elapsed = 0f;
        isAnimating = true;
    }

    private void ResetToInitial()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        SetOpacity(initialOpacity);
        activate = false;

        rend.enabled = true;
        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void SetOpacity(float alpha)
    {
        if (materialInstance.HasProperty("_Color"))
        {
            Color c = materialInstance.color;
            c.a = alpha;
            materialInstance.color = c;
        }
    }
}
