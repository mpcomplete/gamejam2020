﻿using UnityEngine;

public class SpaceField : MonoBehaviour
{
    [SerializeField] MeshFilter MeshFilter = null;
    [SerializeField] MeshRenderer MeshRenderer = null;
    [SerializeField] int GridCellSubdivision = 5;
    [SerializeField] Vector2Int Min = new Vector2Int(-1, -1);
    [SerializeField] Vector2Int Max = new Vector2Int(1, 1);

    public void Render(Vector3[] Positions, float[] Weights) {
        for (int i = 0; i < Positions.Length; i++) {
            MeshRenderer.material.SetVector($"Position_{i}", Positions[i]);
            MeshRenderer.material.SetFloat($"Weight_{i}", Weights[i]);
        }
    }

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh() {
        Mesh mesh = new Mesh();
        
        int length = Max.y - Min.y;
        int width = Max.x - Min.x;
        int resX = (length + 1) * GridCellSubdivision;
        int resZ = (width + 1) * GridCellSubdivision;
        
        #region Vertices		
        Vector3[] vertices = new Vector3[ resX * resZ ];
        for(int z = 0; z < resZ; z++)
        {
            // [ -length / 2, length / 2 ]
            float zPos = ((float)z / (resZ - 1) - .5f) * length;
            for(int x = 0; x < resX; x++)
            {
                // [ -width / 2, width / 2 ]
                float xPos = ((float)x / (resX - 1) - .5f) * width;
                vertices[ x + z * resX ] = new Vector3( xPos, 0f, zPos );
            }
        }
        #endregion
        
        #region Normales
        Vector3[] normales = new Vector3[ vertices.Length ];
        for( int n = 0; n < normales.Length; n++ )
            normales[n] = Vector3.up;
        #endregion
        
        #region UVs		
        Vector2[] uvs = new Vector2[ vertices.Length ];
        for(int v = 0; v < resZ; v++)
        {
            for(int u = 0; u < resX; u++)
            {
                uvs[ u + v * resX ] = new Vector2( (float)u / (resX - 1), (float)v / (resZ - 1) );
            }
        }
        #endregion
        
        #region Triangles
        int nbFaces = (resX - 1) * (resZ - 1);
        int[] triangles = new int[ nbFaces * 6 ];
        int t = 0;
        for(int face = 0; face < nbFaces; face++ )
        {
            // Retrieve lower left corner from face ind
            int i = face % (resX - 1) + (face / (resZ - 1) * resX);
        
            triangles[t++] = i + resX;
            triangles[t++] = i + 1;
            triangles[t++] = i;
        
            triangles[t++] = i + resX;	
            triangles[t++] = i + resX + 1;
            triangles[t++] = i + 1; 
        }
        #endregion
        
        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateBounds();
        mesh.Optimize();
        MeshFilter.mesh = mesh;
    }
}
