using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SixVertexMesh : MonoBehaviour
{
    public Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }
    // Start is called before the first frame update
    void Start()
    {
        MakeMeshData();
        CreateMesh();
    }

    void MakeMeshData()
    {
        vertices = new Vector3[] {  new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
                                    new Vector3(1, 0, 0), new Vector3(0, 0, 1),  new Vector3(1, 0, 1) };
        triangles = new int[] { 0, 1, 2, 3, 4, 5 };
    }
    void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
