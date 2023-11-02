using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rhino.Compute;
using Photon.Pun;

public class RhinoModelRevolve : RhinoModelBasic
{
    #region Hide in inspector
    [HideInInspector]
    public List<Vector3> faceVertices;

    #endregion

    public override void StartModeling()
    {
        base.StartModeling();
        
        photonView.RPC("RPC_CreateModel", RpcTarget.All, Vector3ToFloatList(faceVertices), currentFatherObjId);

        EndModeling();
    }

    [PunRPC]
    public void RPC_CreateModel(float[] floatPoints, int fatherId)
    {
        LocalCreateModel(floatPoints, fatherId);
    }

    private void LocalCreateModel(float[] floatPoints, int fatherId)
    {
        // turn the float list into vector3 list
        List<Vector3> points = FloatListToVector3(floatPoints);
        Vector3 currentFirstPointPos = points[0];
        
        // from the points, create a polyline
        List<Rhino.Geometry.Point3d> rhinoPoints = new List<Rhino.Geometry.Point3d>();
        foreach(Vector3 point in points)
        {
            rhinoPoints.Add(new Rhino.Geometry.Point3d(point.x, point.y, point.z));
        }

        // using this curve to create a bezier curve, smooth the curve
        Rhino.Geometry.Curve bezierCurve = Rhino.Geometry.Curve.CreateControlPointCurve(rhinoPoints, 3);

        // create a vertical rhino line for the revolve axis using the first point
        Rhino.Geometry.Line axisLine = new Rhino.Geometry.Line(rhinoPoints[0], new Rhino.Geometry.Point3d(rhinoPoints[0].X, rhinoPoints[0].Y + 1, rhinoPoints[0].Z));
        // var mesh = MeshCompute.CreateFromRevolvedCurve(curve, new Rhino.Geometry.Point3d(0, 0, 0), new Rhino.Geometry.Vector3d(0, 0, 1), 360, false);
        
        // express the degree using radians
        float radianParameter = Mathf.PI / 180;
        var revolve = Rhino.Geometry.RevSurface.Create(bezierCurve, axisLine, 0 * radianParameter, 360 * radianParameter);
        // close the revolve using berp
        var berp = BrepCompute.CapPlanarHoles(revolve.ToBrep(), 0.01);

        // var closeRevolve = MeshCompute.CreateFromBrep(revolve.ToBrep());
        var closeRevolve = MeshCompute.CreateFromBrep(berp);

        // create a mesh from the revolve
        var meshList = new List<Rhino.Geometry.Mesh>();
        
        meshList.AddRange(closeRevolve);

        GetMeshDataAndCreate(meshList, currentFirstPointPos, fatherId);
    }
}
