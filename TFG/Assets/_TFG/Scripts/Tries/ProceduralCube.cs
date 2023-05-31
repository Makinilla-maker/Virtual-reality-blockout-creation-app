using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCube : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    public float scale = 1f;

    public int posX, posY, posZ;


    float adjScale;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        adjScale = scale * 0.5f;
    }
    // Start is called before the first frame update
    void Start()
    {
        MakeCube(adjScale, new Vector3((float)posX * scale, (float)posY * scale, (float)posZ * scale));
        UpdateMesh();
    }
    void MakeCube(float cubeScale, Vector3 cubePos)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i= 0; i < 6; i++)
        {
            MakeFace(i, cubeScale, cubePos);
        }
    }
    void MakeFace(int dir, float faceScale, Vector3 facePos)
    {
        Vector3[] faceVertices = CubeMeshData.FaceVertices(dir, faceScale, facePos);

        int vCount = vertices.Count;

        vertices.AddRange(faceVertices);

        triangles.Add(vCount);
        triangles.Add(vCount + 1);
        triangles.Add(vCount + 2);
        triangles.Add(vCount);
        triangles.Add(vCount + 2);
        triangles.Add(vCount + 3);
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
