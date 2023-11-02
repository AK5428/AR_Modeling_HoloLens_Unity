using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rhino.Compute;
using Photon.Pun;
public class RhinoModelSweep : RhinoModelBasic
{
    #region Hide in inspector
    [HideInInspector]
    public List<Vector3> faceVertices;

    [HideInInspector]
    public List<Vector3> railVertices;

    #endregion
    
    public override void StartModeling()
    {
        base.StartModeling();
        
        photonView.RPC("RPC_CreateModel", RpcTarget.All, Vector3ToFloatList(faceVertices), Vector3ToFloatList(railVertices), currentFatherObjId);

        EndModeling();
    }

    [PunRPC]
    public void RPC_CreateModel(float[] faceFloatPoints, float[] railFloatPoints, int fatherId)
    {
        LocalCreateModel(faceFloatPoints, railFloatPoints, fatherId);
    }

    private void LocalCreateModel(float[] faceFloatPoints, float[] railFloatPoints, int fatherId)
    {   
        // Convert float list to Vector3 list
        List<Vector3> railPoints = FloatListToVector3(railFloatPoints);
        List<Vector3> shapePoints = FloatListToVector3(faceFloatPoints);

        // initial the position of the first point
        Vector3 currentFirstPointPos = railPoints[0];

        // Create curves from points
        var railCurve = CreateBezierCurve(railPoints);
        var shapeCurve = CreateNormalCurve(shapePoints);

        // // Create a sweep
        var berps = BrepCompute.CreateFromSweep(railCurve, shapeCurve, false, 0.001);

        // list used to strore the mesh
        var meshList = new List<Rhino.Geometry.Mesh>();
        foreach (var berp in berps)
        {
            // close the brep
            var berp3 = berp.CapPlanarHoles(0.001);

            // convert to mesh
            var mesh = MeshCompute.CreateFromBrep(berp3);
            meshList.AddRange(mesh);
        }

        GetMeshDataAndCreate(meshList, currentFirstPointPos, fatherId);

    }

        private Rhino.Geometry.Curve CreateBezierCurve(List<Vector3> points)
    {
        List<Rhino.Geometry.Point3d> rhinoPoints = new List<Rhino.Geometry.Point3d>();
        foreach(Vector3 point in points)
        {
            rhinoPoints.Add(new Rhino.Geometry.Point3d(point.x, point.y, point.z));
        }
        return Rhino.Geometry.Curve.CreateControlPointCurve(rhinoPoints, 5);
    }

    private Rhino.Geometry.Curve CreateNormalCurve(List<Vector3> points)
    {
        List<Rhino.Geometry.Point3d> rhinoPoints = new List<Rhino.Geometry.Point3d>();
        foreach(Vector3 point in points)
        {
            rhinoPoints.Add(new Rhino.Geometry.Point3d(point.x, point.y, point.z));
        }
        Rhino.Geometry.Polyline polyline = new Rhino.Geometry.Polyline(rhinoPoints);
        bool isSmooth = polyline.Smooth(1);

        // turn polyline into a curve, and make sure the curve has no degree
        return polyline.ToNurbsCurve();
    }
}
