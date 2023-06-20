using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class TestExportFunction : MonoBehaviour
{
    public bool onlyobjectToInstantiates = false;
    public bool applyPosition = true;
    public bool applyRotation = true;
    public bool applyScale = true;
    public bool generateMaterials = true;
    public bool exportTextures = true;
    public bool splitObjects = true;
    public bool autoMarkTexReadable = false;
    public bool objNameAddIdNum = false;
    string exportPath = "C:\\Users\\Makineta\\Desktop\\PitoCoño";
    private string lastExportFolder;
    private string versionString = "v2.0";
    string MaterialToString(Material m)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("newmtl " + m.name);
        //add properties
        if (m.HasProperty("_Color"))
        {
            sb.AppendLine("Kd " + m.color.r.ToString() + " " + m.color.g.ToString() + " " + m.color.b.ToString());
            if (m.color.a < 1.0f)
            {
                //use both implementations of OBJ transparency
                sb.AppendLine("Tr " + (1f - m.color.a).ToString());
                sb.AppendLine("d " + m.color.a.ToString());
            }
        }

        if (m.HasProperty("_SpecColor"))
        {
            Color sc = m.GetColor("_SpecColor");
            sb.AppendLine("Ks " + sc.r.ToString() + " " + sc.g.ToString() + " " + sc.b.ToString());
        }

        if (exportTextures)
        {
            //diffuse
            string exResult = TryExportTexture("_MainTex", m);
            if (exResult != "false")
            {
                sb.AppendLine("map_Kd " + exResult);
            }

            //spec map
            exResult = TryExportTexture("_SpecMap", m);
            if (exResult != "false")
            {
                sb.AppendLine("map_Ks " + exResult);
            }

            //bump map
            exResult = TryExportTexture("_BumpMap", m);
            if (exResult != "false")
            {
                sb.AppendLine("map_Bump " + exResult);
            }
        }

        sb.AppendLine("illum 2");
        return sb.ToString();
    }

    string TryExportTexture(string propertyName, Material m)
    {
        if (m.HasProperty(propertyName))
        {
            Texture t = m.GetTexture(propertyName);
        }

        return "false";
    }

    Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    private string ConstructOBJString(int index)
    {
        string idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }

    // Use this for initialization
    void Start()
    {
        //init stuff
        Dictionary<string, bool> materialCache = new Dictionary<string, bool>();
        var exportFileInfo = new System.IO.FileInfo(exportPath);
        lastExportFolder = exportFileInfo.Directory.FullName;
        string baseFileName = System.IO.Path.GetFileNameWithoutExtension(exportPath);
        //get list of required export things
        MeshFilter[] sceneMeshes;
        if (onlyobjectToInstantiates)
        {
            List<MeshFilter> tempMFList = new List<MeshFilter>();
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Block"))
            {
                MeshFilter f = g.GetComponent<MeshFilter>();
                if (f != null)
                {
                    tempMFList.Add(f);
                }
            }

            sceneMeshes = tempMFList.ToArray();
        }
        else
        {
            sceneMeshes = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
        }

        if (Application.isPlaying)
        {
            foreach (MeshFilter mf in sceneMeshes)
            {
                MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
            }
        }

        //work on export
        StringBuilder sb = new StringBuilder();
        StringBuilder sbMaterials = new StringBuilder();
        sb.AppendLine("# Export of " + Application.loadedLevelName);
        sb.AppendLine("# from Aaro4130 OBJ Exporter " + versionString);
        if (generateMaterials)
        {
            sb.AppendLine("mtllib " + baseFileName + ".mtl");
        }

        float maxExportProgress = (float)(sceneMeshes.Length + 1);
        int lastIndex = 0;
        for (int i = 0; i < sceneMeshes.Length; i++)
        {
            string meshName = sceneMeshes[i].gameObject.name;
            float progress = (float)(i + 1) / maxExportProgress;
            MeshFilter mf = sceneMeshes[i];
            MeshRenderer mr = sceneMeshes[i].gameObject.GetComponent<MeshRenderer>();
            if (splitObjects)
            {
                string exportName = meshName;
                if (objNameAddIdNum)
                {
                    exportName += "_" + i;
                }

                sb.AppendLine("g " + exportName);
            }

            if (mr != null && generateMaterials)
            {
                Material[] mats = mr.sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Material m = mats[j];
                    if (!materialCache.ContainsKey(m.name))
                    {
                        materialCache[m.name] = true;
                        sbMaterials.Append(MaterialToString(m));
                        sbMaterials.AppendLine();
                    }
                }
            }

            //export the meshhh :3
            Mesh msh = mf.sharedMesh;
            int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);
            //export vector data (FUN :D)!
            foreach (Vector3 vx in msh.vertices)
            {
                Vector3 v = vx;
                if (applyScale)
                {
                    v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
                }

                if (applyRotation)
                {
                    v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
                }

                if (applyPosition)
                {
                    v += mf.gameObject.transform.position;
                }

                v.x *= -1;
                sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
            }

            foreach (Vector3 vx in msh.normals)
            {
                Vector3 v = vx;
                if (applyScale)
                {
                    v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);
                }

                if (applyRotation)
                {
                    v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
                }

                v.x *= -1;
                sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);
            }

            foreach (Vector2 v in msh.uv)
            {
                sb.AppendLine("vt " + v.x + " " + v.y);
            }

            for (int j = 0; j < msh.subMeshCount; j++)
            {
                if (mr != null && j < mr.sharedMaterials.Length)
                {
                    string matName = mr.sharedMaterials[j].name;
                    sb.AppendLine("usemtl " + matName);
                }
                else
                {
                    sb.AppendLine("usemtl " + meshName + "_sm" + j);
                }

                int[] tris = msh.GetTriangles(j);
                for (int t = 0; t < tris.Length; t += 3)
                {
                    int idx2 = tris[t] + 1 + lastIndex;
                    int idx1 = tris[t + 1] + 1 + lastIndex;
                    int idx0 = tris[t + 2] + 1 + lastIndex;
                    if (faceOrder < 0)
                    {
                        sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                    }
                    else
                    {
                        sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                    }
                }
            }

            lastIndex += msh.vertices.Length;
        }

        //write to disk
        //System.IO.File.WriteAllLines(exportPath, sb.ToString());
        using (System.IO.StreamWriter file = new System.IO.StreamWriter("hi"))
        {
            file.WriteLine(sb.ToString()); // "sb" is the StringBuilder
            if (generateMaterials)
            {
                file.WriteLine(exportFileInfo.Directory.FullName + "\\" + baseFileName + ".mtl", sbMaterials.ToString());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}