using UnityEngine;

/// <summary>
/// Attach to the parent of all animated parts.
/// Setting activate or reset here propagates it to every child PartAnimationController.
/// </summary>
public class MotorAnimationManager : MonoBehaviour
{
    [Tooltip("Set to true to activate the animation on all child parts at once.")]
    public bool activate = false;

    [Tooltip("Set to true to reset all child parts at once.")]
    public bool reset = false;

    private PartAnimationController[] parts;

    private void Awake()
    {
        parts = GetComponentsInChildren<PartAnimationController>();
    }

    private void Update()
    {
        if (activate)
        {
            foreach (PartAnimationController part in parts)
            {
                part.activate = true;
            }
            activate = false;
        }

        if (reset)
        {
            foreach (PartAnimationController part in parts)
            {
                part.reset = true;
            }
            reset = false;
        }
    }
}
