using UnityEngine;

/// <summary>
/// Smoothly follows a target Transform with an optional positional offset.
/// </summary>
public class CameraFollow : MonoBehaviour
{
	public Transform target;

	[Range(0,1)]
	public float smoothTime = 1f;

	public Vector3 offset;
    private Vector3 velocity = Vector3.zero;

	/// <summary>
    /// Smoothly move camera towards the target.
    /// Updates camera position after all Update functions have been called.
    /// </summary>
	private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
    }
}
