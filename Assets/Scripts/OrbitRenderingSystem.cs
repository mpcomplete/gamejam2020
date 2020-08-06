using UnityEngine;
using static Unity.Mathematics.math;
using static MathUtils;

public class OrbitRenderingSystem : MonoBehaviour {
    public LineRenderer[] LineRenderers;
    public Orbiter[] Orbiters;
    public int Count;
    public int PathResolution = 8;

    public void Schedule() {
        for (int i = 0; i < Count; i++) {
            LineRenderers[i].gameObject.SetActive(true);
            Execute(i, Orbiters[i], Orbiters[i].GetComponent<Transform>());
        }
        for (int i = Count; i < LineRenderers.Length; i++) {
            LineRenderers[i].gameObject.SetActive(false);
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