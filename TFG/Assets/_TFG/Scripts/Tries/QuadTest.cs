using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*public struct Quad
{
    Vector3 normal;
    float size;

    public Quad(Vector3 normal, float size)
    {
        this.pivot = pivot;
        this.normal = normal;
        this.size = size;
    }

    public void Append(Vector3 p, List<Vector3> vertices, List<int> tris)
    {
        var q = normal == Vector3.down ? new Quaternion(1f, 0f, 0f, 0f)
                                      : Quaternion.FromToRotation(normal, Vector3.up);

        var i = vertices.Count;

        vertices.Add(tr(-1f, -1f));
        vertices.Add(tr(-1f, +1f));
        vertices.Add(tr(+1f, +1f));
        vertices.Add(tr(+1f, -1f));

        tris.Add(i);
        tris.Add(i + 1);
        tris.Add(i + 2);

        tris.Add(i + 2);
        tris.Add(i + 3);
        tris.Add(i);

        Vector3 tr(float x, float y) => p + q * (size * new Vector3(x, 0f, y));
    }

}*/



public class QuadTest : MonoBehaviour
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
        vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(1,0,1)};
        triangles = new int[] { 0, 1, 2, 2, 1, 3 };
    }
    void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
