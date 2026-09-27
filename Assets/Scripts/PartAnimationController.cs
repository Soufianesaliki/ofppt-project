using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Animates a single part's world position and world rotation from an initial value
/// to a final value over a fixed duration (the "reveal"), and separately supports
/// disappearing (the "hide") — either instantly or as an opacity fade using the same
/// duration/easingCurve, reusing initialOpacity/finalOpacity for the fade range.
/// Assign this script directly on each part you want to animate.
///
/// Batch progression is driven strictly by the Next (B) button via
/// MotorAnimationManager: reveal moves the piece into view; Hide() is called by the
/// manager once this piece's batch is superseded by the next one (or, for the final
/// batch, once the extra press that closes out Scenario 1 happens).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PartAnimationController : MonoBehaviour
{
    [Header("Target Values (Inspector)")]
    [Tooltip("Position offset added to the initial position (e.g. (2,0,0) moves 2 units further along world X from the start point).")]
    public Vector3 positionOffset;

    [Tooltip("Rotation offset (Euler angles) added to the initial rotation.")]
    public Vector3 rotationOffsetEuler;

    [Header("Timing")]
    [Tooltip("Duration in seconds of both the reveal move and (if animated) the hide fade.")]
    public float duration = 1f;

    [Tooltip("Shapes how t (0-1) progresses over duration, for both the reveal move and the hide fade.")]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Hide")]
    [Tooltip("If true, Hide() fades opacity from initialOpacity to finalOpacity over duration/easingCurve before disabling. If false, Hide() disables immediately.")]
    public bool animateHide = false;

    [Tooltip("Opacity at the start of the hide fade (0 = transparent, 1 = opaque). Only used if animateHide is true.")]
    [Range(0f, 1f)] public float initialOpacity = 1f;

    [Tooltip("Opacity at the end of the hide fade (0 = transparent, 1 = opaque). Only used if animateHide is true.")]
    [Range(0f, 1f)] public float finalOpacity = 0f;

    [Header("Trigger")]
    [Tooltip("Set to true to start the reveal. Automatically turns false when the reveal finishes.")]
    public bool activate = false;

    [Tooltip("Set to true to reset the part back to its original position/rotation, visible again. Only works while idle (not animating, not hiding). Always turns back to false.")]
    public bool reset = false;

    [Header("Events")]
    [Tooltip("Invoked once the reveal move reaches its final value. Not invoked by reset().")]
    public UnityEvent onRevealComplete = new UnityEvent();

    [Tooltip("Invoked once the piece has fully disappeared (instantly, or after the hide fade finishes).")]
    public UnityEvent onHideComplete = new UnityEvent();

    // Captured at scene load
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Computed once from initial values + inspector offsets
    private Vector3 finalPosition;
    private Quaternion finalRotation;

    private Renderer rend;
    private Collider col; // may be null, guarded on use
    private Material materialInstance;

    private bool isAnimating = false;
    private bool isHiding = false;
    private float elapsed = 0f;
    private float hideElapsed = 0f;

    private void Awake()
    {
        // Capture current state as the initial position/rotation
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Final = initial + offset (world space)
        finalPosition = initialPosition + positionOffset;
        finalRotation = initialRotation * Quaternion.Euler(rotationOffsetEuler);

        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        materialInstance = rend.material; // Unity auto-instances this material
    }

    /// <summary>
    /// Called by the manager once this piece's batch is superseded. Always resolves
    /// on the next Update() tick — even the instant (non-animated) case — so
    /// onHideComplete never fires synchronously from within the caller's own call
    /// stack (e.g. MotorAnimationManager mid-loop).
    /// </summary>
    public void Hide()
    {
        if (isHiding) return;

        hideElapsed = 0f;
        isHiding = true;
    }

    private void Update()
    {
        if (reset)
        {
            if (!isAnimating && !isHiding)
            {
                ResetToInitial();
            }

            reset = false;
        }

        if (activate && !isAnimating && !isHiding)
        {
            BeginAnimation();
        }

        if (isAnimating)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = easingCurve.Evaluate(t);

            transform.position = Vector3.Lerp(initialPosition, finalPosition, curvedT);
            transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, curvedT);

            if (t >= 1f)
            {
                isAnimating = false;
                activate = false;
                onRevealComplete.Invoke();
            }

            return;
        }

        if (isHiding)
        {
            if (animateHide)
            {
                hideElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(hideElapsed / duration);
                float curvedT = easingCurve.Evaluate(t);

                SetOpacity(Mathf.Lerp(initialOpacity, finalOpacity, curvedT));

                if (t >= 1f)
                {
                    DisableNow();
                }
            }
            else
            {
                DisableNow();
            }
        }
    }

    private void BeginAnimation()
    {
        elapsed = 0f;
        isAnimating = true;
    }

    private void DisableNow()
    {
        isHiding = false;
        rend.enabled = false;
        if (col != null)
        {
            col.enabled = false;
        }

        onHideComplete.Invoke();
    }

    private void ResetToInitial()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        SetOpacity(initialOpacity);
        activate = false;
        isHiding = false;

        rend.enabled = true;
        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void SetOpacity(float alpha)
    {
        if (materialInstance.HasProperty("_BaseColor"))
        {
            Color c = materialInstance.GetColor("_BaseColor");
            c.a = alpha;
            materialInstance.SetColor("_BaseColor", c);
        }
        else if (materialInstance.HasProperty("_Color"))
        {
            Color c = materialInstance.color;
            c.a = alpha;
            materialInstance.color = c;
        }
    }
}
