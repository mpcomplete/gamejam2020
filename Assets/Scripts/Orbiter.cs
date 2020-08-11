using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static MathUtils;

public class Orbiter : MonoBehaviour {
    public static float Period(Orbiter o, float M, float G) {
        return TWO_PI * o.Radius / sqrt(M * G / o.Radius);
    }

    // solving p = 2pi * r / sqrt(M * G / r) for r
    public static float RequiredRadiusForPeriod(float M, float G, float period) {
        return pow(TWO_PI, -2f/3f) * pow(G, 1f/3f) * pow(M, 1f/3f) * pow(period, 2f/3f);
    }

    public enum RotationalDirection { Clockwise = -1, CounterClockwise = 1 }

    public RotationalDirection Direction = RotationalDirection.Clockwise;
    [Range(0, 100)] public float Radius = 10;
    [Range(0, 2 * PI)] public float Radians = 0;
}