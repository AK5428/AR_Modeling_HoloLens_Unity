using System.Data;
// using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rhino.Compute;
using Rhino;


public class TestCompute : MonoBehaviour
{
    // Start is called before the first frame update
    public string authToken;
    public Material mat;

    private Rhino.FileIO.File3dm model;
    private float prevHeight, prevPipeRad, prevAngle, prevSegments;
    private float height = 10f;
    private float pipeRad = 2f;
    private float angle = 30f;
    private int segments = 5;

    void Start()
    {
        ComputeServer.AuthToken = authToken;
        ComputeServer.WebAddress = "http://localhost:8081/";

        // set the mat as Cull front
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);
       
        // GenerationGo();
    }

    // Update is called once per frame
    void Update()
    {
        bool updateGeo = false;
        if(prevHeight != height)
        {
            prevHeight = height;
            updateGeo = true;
        }

        if(prevPipeRad != pipeRad)
        {
            prevPipeRad = pipeRad;
            updateGeo = true;
        }

        if(prevAngle != angle)
        {
            prevAngle = angle;
            updateGeo = true;
        }

        if(prevSegments != segments)
        {
            prevSegments = segments;
            updateGeo = true;
        }

        if(updateGeo)
        {
            DestroyOldMesh();
            GenerationGo();
        }
    }

    // the function used to destory the old mesh object
    private void DestroyOldMesh()
    {
        var oldMesh = GameObject.FindObjectsOfType<MeshFilter>();
        foreach(var obj in oldMesh)
        {
            Destroy(obj.mesh);
            Destroy(obj.gameObject);
        }

        Resources.UnloadUnusedAssets();
    }

    // a function used to create 3d objects
    private void GenerationGo()
    {
        model = new Rhino.FileIO.File3dm();

        int num = 10;
        float outerRad = 2f;
        var curves = new List<Rhino.Geometry.Curve>();

        for(int i = 0; i < num; i++)
        {
            var pt = new Rhino.Geometry.Point3d(0, 0, height / (num - 1) * i);
            var circle = new Rhino.Geometry.Circle(pt, pipeRad);
            // creat a polygon inside the circle
            var polygon = Rhino.Geometry.Polyline.CreateInscribedPolygon(circle, segments);
            var curve = polygon.ToNurbsCurve();
            curve.Rotate(i * Mathf.Deg2Rad * angle, new Rhino.Geometry.Vector3d(0, 0, 1), new Rhino.Geometry.Point3d(0, 0, 0));
            curve.Translate(new Rhino.Geometry.Vector3d(Mathf.Cos(Mathf.Deg2Rad * angle * i), Mathf.Sin(Mathf.Deg2Rad * angle * i), 0) * outerRad);

            // create from loft
            curves.Add(curve);

            // model.Objects.AddCurve(curve);
        }

        var brep = BrepCompute.CreateFromLoft(curves, Rhino.Geometry.Point3d.Unset, Rhino.Geometry.Point3d.Unset, Rhino.Geometry.LoftType.Normal, false);

        var meshList = new List<Rhino.Geometry.Mesh>();
        foreach(var obj in brep)
        {
            model.Objects.AddBrep(obj);

            // close the brep
            var berp2 = obj.CapPlanarHoles(0.001);
            // convert to mesh
            var mesh = MeshCompute.CreateFromBrep(berp2);
            meshList.AddRange(mesh);
        }

        // convert the rhino mesh into Unity mesh
        foreach(var mesh in meshList)
        {
            Mesh meshObj = new Mesh();

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            
            foreach(var meshVertex in mesh.Vertices)
            {
                var vertex = new Vector3((float)meshVertex.X, (float)meshVertex.Y, (float)meshVertex.Z);
                vertices.Add(vertex);
            }
            
            foreach(var meshFace in mesh.Faces)
            {
                if(meshFace.IsTriangle)
                {
                    // if start with A, then the mesh would be flipped
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.B);
                    triangles.Add(meshFace.A);
                }else if(meshFace.IsQuad)
                {
                    // the quad in the unity would be split into two triangles
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.B);
                    triangles.Add(meshFace.A);
                    triangles.Add(meshFace.D);
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.A);
                }
            }

            foreach(var meshNormal in mesh.Normals)
            {
                var normal = new Vector3((float)meshNormal.X, (float)meshNormal.Z, (float)meshNormal.Y);
                // flip the normal
                normal = -normal;
                normals.Add(normal);
            }

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.normals = normals.ToArray();

            GameObject obj = new GameObject();
            obj.AddComponent<MeshFilter>().mesh = meshObj;
            obj.AddComponent<MeshRenderer>().material = mat;
        }


        // write the model into an obj file
        // string path = Application.dataPath + "/test.3dm";
        // model.Write(path, 5);

        
    }

    private void OnGUI() {
        //lables
        GUI.Label(new Rect(10, 10, 90, 20), "Height");
        height = GUI.HorizontalSlider(new Rect(100, 15, 100, 20), height, 1.0f, 20.0f);

        GUI.Label(new Rect(10, 30, 90, 20), "Radius");
        pipeRad = GUI.HorizontalSlider(new Rect(100, 35, 100, 20), pipeRad, 0.5f, 5.0f);

        GUI.Label(new Rect(10, 50, 90, 20), "Angle");
        angle = GUI.HorizontalSlider(new Rect(100, 55, 100, 20), angle, 0.0f, 90.0f);

        GUI.Label(new Rect(10, 70, 90, 20), "Segments");
        segments = (int)GUI.HorizontalSlider(new Rect(100, 75, 100, 20), segments, 3, 8);
    }
}
