using UnityEngine;
using static Unity.Mathematics.math;

public class Rotator : MonoBehaviour {
    public enum RotationalDirection { Clockwise = 1, CounterClockwise = -1 }

    public RotationalDirection Direction = RotationalDirection.Clockwise;
    [Range(0, 100)] public float Period = 10;
    [Range(0, 2 * PI)] public float Radians = 0;
}