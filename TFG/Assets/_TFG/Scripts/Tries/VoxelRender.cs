using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelRender : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    public float scale = 1f;

    float adjScale;

    List<Vector3> temp;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        adjScale = scale * 0.5f;
        temp = new List<Vector3>();
        temp.Add(new Vector3(0,0,0));
        temp.Add(new Vector3(1,0,0));
        temp.Add(new Vector3(2,0,0));
        temp.Add(new Vector3(3,0,0));
        temp.Add(new Vector3(4,0,0));
        temp.Add(new Vector3(4,0,1));
        temp.Add(new Vector3(4,0,2));
        temp.Add(new Vector3(4,0,3));
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateVoxelMesh(new VoxelData(temp));
        UpdateMesh();
    }
    void GenerateVoxelMesh(VoxelData data)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        //for (int z = 0; z < data.Depth; z++)
        //{
        //    for (int x = 0; x < data.Width; x++)
        //    {
        //        Debug.Log("Z == " + z + "      " + "X == " + x + "      " + "data.Depth == " + data.Depth + "    data.Width == " + data.Width + "data.GetCell(" + x + "," + z +") = " +  data.GetCell(x, z));
        //        if (data.GetCell(x, z) == 0)
        //        {
        //            continue;
        //        }
        //        MakeCube(adjScale, new Vector3((float)x * scale, 0, (float)z * scale),data);
        //    }
        //}
        for (int i = 0; i < temp.Count; i++)
        {
            MakeCube(adjScale, temp[i] * scale, data);
        }
    }
    void MakeCube(float cubeScale, Vector3 cubePos, VoxelData data)
    {
        for (int i = 0; i < 6; i++)
        {
            if(data.GetNeighbor(cubePos, (Direction)i) == 0)
                MakeFace((Direction)i, cubeScale, cubePos);
        }
    }
    void MakeFace(Direction dir, float faceScale, Vector3 facePos)
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
