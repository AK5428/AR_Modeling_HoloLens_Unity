using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rhino.Compute;
using Photon.Pun;

public class RhinoModelExtrusion : RhinoModelBasic
{
    // [Header("Extra Parameters")]
    // public GameObject switchManager;

    #region Hide in inspector
    [HideInInspector]
    public List<Vector3> faceVertices;
    [HideInInspector]
    public Vector3 planeNormal;
    [HideInInspector]
    public bool modelEnd = false;
    [HideInInspector]
    public float currentExtrusionHeight = 0.1f;
    #endregion
    
    #region Private variables hide in inspector
    private float lastExtrusionHeight = 0.1f;
    private List<GameObject> localModelObjects = new List<GameObject>();

    // parameters for local
    private GameObject localFatherObject;

    #endregion

    public override void EndModeling()
    {
        modelEnd = true;
        Destroy(localModelObjects.Last());
        photonView.RPC("RPC_CreateModel", RpcTarget.All, Vector3ToFloatList(faceVertices), currentExtrusionHeight, currentFatherObjId);
        base.EndModeling();
    }

    public override void Update()
    {
        if(!modelEnd && currentExtrusionHeight != lastExtrusionHeight)
        {
            lastExtrusionHeight = currentExtrusionHeight;
            Destroy(localModelObjects.Last());
            LocalCreateModel(Vector3ToFloatList(faceVertices), currentExtrusionHeight, false, currentFatherObjId);
        }
    }

    public override void StartModeling()
    {
        base.StartModeling();
        modelEnd = false;
        LocalCreateModel(Vector3ToFloatList(faceVertices), currentExtrusionHeight, false, currentFatherObjId);
        // photonView.RPC("CreateModel", RpcTarget.All, Vector3ToFloatList(faceVertices), currentExtrusionHeight);
    }

    [PunRPC]
    public void RPC_CreateModel(float[] floatPoints, float extrusionHeight, int fatherId)
    {
        LocalCreateModel(floatPoints, extrusionHeight, true, fatherId);
    }

    private void LocalCreateModel(float[] floatPoints, float extrusionHeight, bool isPun, int fatherId)
    {
        // turn the float list into vector3 list
        List<Vector3> points = FloatListToVector3(floatPoints);

        // initial the position of the first point
        Vector3 currentFirstPointPos = points[0];

        /*** Rhino3dm no server calculate ***/
        // from the points, create a polyline
        List<Rhino.Geometry.Point3d> rhinoPoints = new List<Rhino.Geometry.Point3d>();
        foreach(Vector3 point in points)
        {
            rhinoPoints.Add(new Rhino.Geometry.Point3d(point.x, point.y, point.z));
        }

        // create a polygon from the points
        Rhino.Geometry.Polyline polyline = new Rhino.Geometry.Polyline(rhinoPoints);
        polyline.Smooth(1);

        // convert the polyline into a curve
        Rhino.Geometry.Curve bezierCurve = Rhino.Geometry.Curve.CreateControlPointCurve(rhinoPoints, 3);

        // make sure the curve is on a plane
        // calculate the plane based on the first three points
        var fitPlane = new Rhino.Geometry.Plane(rhinoPoints[0], rhinoPoints[1], rhinoPoints[2]);
        bezierCurve.Transform(Rhino.Geometry.Transform.PlanarProjection(fitPlane));

        /*** compute.rhino3d.io server calculate ***/
        Rhino.Geometry.Extrusion extrusion = Rhino.Geometry.Extrusion.Create(bezierCurve, extrusionHeight, true);
        // create a mesh from the extrusion
        var meshList = new List<Rhino.Geometry.Mesh>();
        // var mesh = Rhino.Geometry.Mesh.CreateFromCurveExtrusion(curve, 1, true, Rhino.Geometry.MeshingParameters.Default);
        var meshCount = MeshCompute.CreateFromBrep(extrusion.ToBrep(), Rhino.Geometry.MeshingParameters.Default);
        meshList.AddRange(meshCount);

        if(isPun) GetMeshDataAndCreate(meshList, currentFirstPointPos, fatherId);
        else LocalGetMeshAndCreate(meshList, currentFirstPointPos);
    }

    private void LocalGetMeshAndCreate(List<Rhino.Geometry.Mesh> meshList, Vector3 objectPosition)
    {
        localFatherObject = new GameObject();
        localFatherObject.transform.position = objectPosition;
        localModelObjects.Add(localFatherObject);

        // convert the rhino mesh into Unity mesh
        foreach(var mesh in meshList)
        {
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
                // normal = -normal;
                normals.Add(normal);
            }
        
            // create the mesh object
            LocalCreateRhinoMesh(vertices, triangles, normals);
        }

    }

    private void LocalCreateRhinoMesh(List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        // get the position of the first vertex
        // initialize the mesh object as the root of this single mesh
        GameObject singleMeshObject = new GameObject();
        singleMeshObject.name = "RhinoMesh";
        singleMeshObject.transform.parent = localFatherObject.transform;

        // create the mesh object
        Mesh meshObj = new Mesh();
        meshObj.vertices = vertices.ToArray();
        meshObj.triangles = triangles.ToArray();
        meshObj.normals = normals.ToArray();

        // add the mesh object to the single mesh object
        singleMeshObject.AddComponent<MeshFilter>().mesh = meshObj;
        singleMeshObject.AddComponent<MeshRenderer>().material = mat;
    }
  
}
