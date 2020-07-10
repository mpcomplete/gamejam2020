using System.Collections.Generic;
using UnityEngine;

public class LightBeam : MonoBehaviour
{
    [System.Serializable]
    public struct GridPosition
    {
        public int x;
        public int y;
        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static int MAX_GRID_POSITIONS = 17;

    [SerializeField] LineRenderer LineRenderer = null;
    [SerializeField] Color Color = Color.white;
    [SerializeField] List<GridPosition> GridPositions = new List<GridPosition>(MAX_GRID_POSITIONS);

    public static Vector3 GridToWorldPosition(ref GridPosition gp)
    {
        return new Vector3(gp.x, 1, gp.y);
    }

    public bool ObjectAt(int x, int y)
    {
        return false;
    }

    //  Cast will call itself recursively until it stops... isn't that cool?
    public int Cast(int src, int x, int y, int xMax, int yMax)
    {
        return 0;
    }

    public void OnDrawGizmos()
    {
        int count = GridPositions.Count; 

        if (count < 2)
            return;

        Gizmos.color = Color;
        // Render the start 
        {
            GridPosition gp0 = GridPositions[0];
            Vector3 p0World = GridToWorldPosition(ref gp0);

            Gizmos.DrawWireSphere(p0World, .25f);
        }

        // Render the beam
        for (int i = 1; i < count; i++)
        {
            GridPosition gp0 = GridPositions[i-1];
            GridPosition gp1 = GridPositions[i];
            Vector3 p0World = GridToWorldPosition(ref gp0);
            Vector3 p1World = GridToWorldPosition(ref gp1);

            Gizmos.DrawLine(p0World, p1World);
        }

        // Render the end
        {
            GridPosition gp = GridPositions[GridPositions.Count - 1];
            Vector3 pWorld = GridToWorldPosition(ref gp);

            Gizmos.DrawWireCube(pWorld, Vector3.one * .5f);
        }
    }

    [ContextMenu("Render")]
    public void Render()
    {
        int count = GridPositions.Count; 

        if (count < 2)
            return;

        // Render the beam
        LineRenderer.positionCount = count;
        for (int i = 0; i < count; i++)
        {
            GridPosition gp = GridPositions[i];
            Vector3 pWorld = GridToWorldPosition(ref gp);

            LineRenderer.SetPosition(i, pWorld);
        }
    }

    /*
    Light Beams are made up of light segments
    Light segments are defined by cell coordinates
    Marching a beam along a direction from a cell
    will update a list of Light Segments for that beam
    */
}