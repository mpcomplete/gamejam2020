using UnityEngine;

public class GridLines : MonoBehaviour
{
    [SerializeField] LineRenderer[] LineRenderers = null;

    public Vector2Int Min;
    public Vector2Int Max;

    [ContextMenu("Layout")]
    public void Layout() {
        int index = 0;

        // layout the z-axis
        for (var i = Min.x; i <= Max.x; i++) {
            LineRenderers[index].gameObject.SetActive(true);
            LineRenderers[index].SetPosition(0, new Vector3(i, 0, Min.y));
            LineRenderers[index].SetPosition(1, new Vector3(i, 0, Max.y));
            index++;
        }

        // layout the x-axis
        for (var i = Min.y; i <= Max.y; i++) {
            LineRenderers[index].gameObject.SetActive(true);
            LineRenderers[index].SetPosition(0, new Vector3(Min.x, 0, i));
            LineRenderers[index].SetPosition(1, new Vector3(Max.x, 0, i));
            index++;
        }

        for (var i = index; i < LineRenderers.Length; i++) {
            LineRenderers[i].gameObject.SetActive(false);
        }
    }
}