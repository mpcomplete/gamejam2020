using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    public float MaximumMovementSpeed = 1f;

    public void MoveTowards(float dt, Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = targetPosition - currentPosition;
        float distance = delta.magnitude;
        float moveableDistance = Mathf.Min(MaximumMovementSpeed * dt, distance);
        Vector3 direction = delta.normalized;

        transform.position = currentPosition + direction * moveableDistance;
    }
}