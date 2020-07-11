using System.Collections.Generic;
using UnityEngine;

public class LightBeam : MonoBehaviour
{
    public static int MAX_GRID_POSITIONS = 17;

    [SerializeField] LineRenderer LineRenderer = null;
    [SerializeField] Color Color = Color.white;
    [SerializeField] List<Vector2Int> GridPositions = new List<Vector2Int>(MAX_GRID_POSITIONS);

    public static Vector3 GridToWorldPosition(ref Vector2Int gp) {
        return new Vector3(gp.x, 1, gp.y);
    }

    // Temporary "fake" reflection stuff
    public static Vector2Int ReflectedHeading(Vector2Int heading)
    {
        return new Vector2Int(heading.y, heading.x);
    }

    public int Cast(Board board, Vector2Int origin, Vector2Int direction) {
        GridPositions.Clear();
        GridPositions.Add(origin);
        return March(board, origin, direction);
    }

    //  March will call itself recursively until it stops... isn't that cool?
    public int March(Board board, Vector2Int src, Vector2Int heading)
    {
        Vector2Int nextCell = src + heading;

        if (board.OutOfBounds(nextCell)) {
            GridPositions.Add(src);
            return GridPositions.Count;
        }
        
        GameObject target = board.GetObjectAtCell(nextCell);

        if (target) {
            Mirror mirror = target.GetComponent<Mirror>();

            if (mirror) {
                GridPositions.Add(nextCell);
                return March(board, nextCell, ReflectedHeading(heading));
            }
        }

        return March(board, nextCell, heading);
    }

    public void OnDrawGizmos() {
        int count = GridPositions.Count; 

        if (count < 2) {
            return;
        }

        Gizmos.color = Color;
        // Render the start 
        {
            Vector2Int gp0 = GridPositions[0];
            Vector3 p0World = GridToWorldPosition(ref gp0);

            Gizmos.DrawWireSphere(p0World, .25f);
        }

        // Render the beam
        for (int i = 1; i < count; i++) {
            Vector2Int gp0 = GridPositions[i-1];
            Vector2Int gp1 = GridPositions[i];
            Vector3 p0World = GridToWorldPosition(ref gp0);
            Vector3 p1World = GridToWorldPosition(ref gp1);

            Gizmos.DrawLine(p0World, p1World);
        }

        // Render the end
        {
            Vector2Int gp = GridPositions[GridPositions.Count - 1];
            Vector3 pWorld = GridToWorldPosition(ref gp);

            Gizmos.DrawWireCube(pWorld, Vector3.one * .5f);
        }
    }

    [ContextMenu("Render")]
    public void Render()
    {
        int count = GridPositions.Count; 

        if (count < 2) {
            return;
        }

        // Render the beam
        LineRenderer.positionCount = count;
        for (int i = 0; i < count; i++) {
            Vector2Int gp = GridPositions[i];
            Vector3 pWorld = GridToWorldPosition(ref gp);

            LineRenderer.SetPosition(i, pWorld);
        }
    }
}