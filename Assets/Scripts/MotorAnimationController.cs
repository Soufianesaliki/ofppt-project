using UnityEngine;

public class MotorAnimationController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool isLoop;
    [SerializeField] private bool activateAnimation;

    [Header("Final Values")]
    [SerializeField] private Vector3 finalPosition;
    [SerializeField] private Vector3 finalRotation;
    [SerializeField, Range(0f, 1f)] private float finalOpacity = 1f;

    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private float initialOpacity = 1f;
    private float animationTime;
    private bool wasActivated;
    private Renderer[] renderers;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localEulerAngles;
        renderers = GetComponentsInChildren<Renderer>(true);
        initialOpacity = GetCurrentOpacity();
    }

    private void Update()
    {
        if (activateAnimation && !wasActivated)
        {
            animationTime = 0f;
            SetValues(initialPosition, initialRotation, initialOpacity);
        }

        wasActivated = activateAnimation;

        if (!activateAnimation)
            return;

        animationTime += Time.deltaTime;
        float progress = duration <= 0f ? 1f : Mathf.Clamp01(animationTime / duration);
        SetValues(
            Vector3.Lerp(initialPosition, finalPosition, progress),
            Vector3.Lerp(initialRotation, finalRotation, progress),
            Mathf.Lerp(initialOpacity, finalOpacity, progress));

        if (progress >= 1f)
        {
            if (isLoop)
            {
                animationTime = 0f;
                SetValues(initialPosition, initialRotation, initialOpacity);
            }
            else
            {
                activateAnimation = false;
                wasActivated = false;
            }
        }
    }

    private void SetValues(Vector3 position, Vector3 rotation, float opacity)
    {
        transform.localPosition = position;
        transform.localEulerAngles = rotation;
        SetOpacity(opacity);
    }

    private void SetOpacity(float opacity)
    {
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    Color color = material.GetColor("_BaseColor");
                    color.a = opacity;
                    material.SetColor("_BaseColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = opacity;
                    material.color = color;
                }
            }
        }
    }

    private float GetCurrentOpacity()
    {
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                    return material.GetColor("_BaseColor").a;

                if (material.HasProperty("_Color"))
                    return material.color.a;
            }
        }

        return 1f;
    }
}
