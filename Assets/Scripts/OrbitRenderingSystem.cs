using UnityEngine;
using static Unity.Mathematics.math;
using static MathUtils;

public class OrbitRenderingSystem : MonoBehaviour {
    public static int MAX_RESOLUTION = 256;

    public LineRenderer[] LineRenderers;
    public Orbiter[] Orbiters;
    public int Count;
    public int PathResolution = 8;

    public void Schedule() {
        for (int i = 0; i < Count; i++) {
            Execute(i, Orbiters[i], Orbiters[i].GetComponent<Transform>());
        }
    }

    public void Execute(int index, Orbiter orbiter, Transform transform) {
        LineRenderer lr = LineRenderers[index];

        lr.positionCount = PathResolution + 1;
        for (int i = 0; i <= PathResolution; i++) {
            Vector3 localPosition = orbiter.Radius * UnitCircle(2 * PI * (float)i / PathResolution);
            Vector3 worldPosition = orbiter.transform.parent.TransformPoint(localPosition);

            lr.SetPosition(i, worldPosition);
        }
    }
}