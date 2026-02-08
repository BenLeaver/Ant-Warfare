using UnityEngine;

/// <summary>
/// Smoothly follows a target Transform with an optional positional offset.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;

	[Range(0,1)]
	public float smoothTime = 0.1f;

	public Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    [Header("Screen Shake")]
    public float maxShakeAmplitude = 0.3f;
    public float shakeDuration = 0.25f;
    public float maxEffectDistance = 15f;

    private float shakeTimer;
    private float currentShakeAmplitude;
    private Vector3 shakeOffset;

    /// <summary>
    /// Smoothly move camera towards the target.
    /// Updates camera position after all Update functions have been called.
    /// </summary>
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 basePosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            float strength = currentShakeAmplitude * (shakeTimer / shakeDuration);
            Vector2 randomOffset = Random.insideUnitCircle * strength;
            shakeOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
        transform.position = basePosition + shakeOffset;
    }

    /// <summary>
    /// Triggers a screen shake whose intensity depends on distance to the impact position.
    /// </summary>
    public void ShakeAtPosition(Vector3 impactPosition)
    {
        float distance = Vector2.Distance(transform.position, impactPosition);

        if (distance > maxEffectDistance) return;

        float t = 1f - Mathf.Clamp01(distance / maxEffectDistance);
        currentShakeAmplitude = maxShakeAmplitude * t;

        shakeTimer = shakeDuration;
    }
}
