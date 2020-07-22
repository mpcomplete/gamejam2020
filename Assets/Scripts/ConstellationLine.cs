using UnityEngine;

[ExecuteInEditMode]
public class ConstellationLine : MonoBehaviour {
    public Transform T1;
    public Transform T2;
    public LineRenderer LineRenderer;

    void Update() {
        LineRenderer.positionCount = 2;
        LineRenderer.SetPosition(0, T1.transform.position);
        LineRenderer.SetPosition(1, T2.transform.position);
    }
}