using UnityEngine;
using static MathUtils;

public class OrbitGroup : MonoBehaviour {
    public float Radius = 20f;

    [ContextMenu("Position")]
    public void Position() {
        Orbiter[] orbiters = GetComponentsInChildren<Orbiter>();

        for (int i = 0; i < orbiters.Length; i++) {
            orbiters[i].Radius = Radius;
            orbiters[i].Radians = TWO_PI * (float)i/(float)orbiters.Length;
            orbiters[i].transform.position = transform.position + Radius * UnitCircle(orbiters[i].Radians);
        }
    }
}