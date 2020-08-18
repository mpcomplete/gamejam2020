using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class PathSystem : MonoBehaviour {
    public struct Endpoint {
        public float3 Position;
        public float3 Heading;
        public float Speed;
        public Endpoint(float3 position, float3 heading, float speed) {
            Position = position;
            Heading = heading;
            Speed = speed;
        }
    }

    public struct Trajectory {
        public float3 p0;
        public float3 v0;
        public float3 A;
        public float3 B;
        public float duration;
    }

    public Transform Origin;
    public float OriginSpeed;
    public Transform Destination;
    public float DestinationSpeed;
    public int SegmentCount = 16;
    public float Duration = 10f;
    public float WaypointMarkerRadius = .2f;

    // Calculates a valid path assuming acceleration fits this polynomial: p''(t) = Att + Bt + C
    // Also assume that end acceleration is 0
    public static float3[] CalculatePath(int segmentCount, float duration, Endpoint start, Endpoint end) {
        float3[] path = new float3[segmentCount + 1];
        Trajectory trajectory = TrajectoryFrom(duration, start.Position, start.Heading * start.Speed, end.Position, end.Heading * end.Speed);

        for (int i = 0; i <= segmentCount; i++) {
            path[i] = PositionFromTrajectoryAtTime(trajectory, (float)i/(float)segmentCount * duration);
        }
        return path;
    }

    public static Trajectory TrajectoryFrom(float tf, float3 p0, float3 v0, float3 pf, float3 vf) {
        float3 A = 12 * (3*p0 - 3*pf + 2*tf*v0 + tf*vf) / (tf*tf*tf*tf);
        float3 B = -6 * (4*p0 - 4*pf + 3*tf*v0 + tf*vf) / (tf*tf*tf);

        return new Trajectory { p0 = p0, v0 = v0, A = A, B = B, duration = tf };
    }

    public static float3 PositionFromTrajectoryAtTime(Trajectory trajectory, float t) {
        return trajectory.p0 + (2f/12f)*trajectory.B*t*t*t + (1f/12f)*trajectory.A*t*t*t*t + trajectory.v0*t;
    }

    void OnDrawGizmos() {
        void RenderPath(float3[] p, Color color) {
            Gizmos.color = color;
            for (int i = 1; i < p.Length; i++) {
                Gizmos.DrawLine(p[i-1], p[i]);
            }
            for (int i = 0; i < p.Length; i++) {
                Gizmos.DrawSphere(p[i], WaypointMarkerRadius);
            }
        }

        Endpoint start = new Endpoint(Origin.transform.position, Origin.transform.forward, OriginSpeed);
        Endpoint end = new Endpoint(Destination.transform.position, Destination.transform.forward, DestinationSpeed);
        float3[] path = CalculatePath(SegmentCount, Duration, start, end);

        RenderPath(path, Color.green);
    }
}