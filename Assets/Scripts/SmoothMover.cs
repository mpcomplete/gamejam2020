using UnityEngine;

public class SmoothMover : MonoBehaviour
{
    public Vector3 TargetPosition;
    public float LerpEpsilon = 1e-6f;
}