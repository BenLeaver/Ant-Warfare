using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float maxShakeAmplitude = 0.4f;
    public float shakeDuration = 0.25f;
    public float maxEffectDistance = 15f;

    private float shakeTimer;
    private float currentAmplitude;
    private Vector3 originalPos;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (shakeTimer <= 0f)
        {
            transform.localPosition = originalPos;
            return;
        }

        shakeTimer -= Time.deltaTime;

        float strength = currentAmplitude * (shakeTimer / shakeDuration);
        Vector2 offset = Random.insideUnitCircle * strength;

        transform.localPosition = originalPos + new Vector3(offset.x, offset.y, 0f);
    }

    /// <summary>
    /// Triggers a screen shake based on distance to an impact point.
    /// </summary>
    public void ShakeAtPosition(Vector3 impactPos)
    {
        float dist = Vector2.Distance(transform.position, impactPos);

        if (dist > maxEffectDistance)
            return;

        float t = 1f - Mathf.Clamp01(dist / maxEffectDistance);
        currentAmplitude = maxShakeAmplitude * t;

        shakeTimer = shakeDuration;
    }
}
