using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PaintModelSwitcherSweep : PaintModelSwitcherBasic
{
    public override void DataPassOnSwitch()
    {
        // pass the data
        var faceVertices = new List<Vector3>();
        foreach (var vertex in paintManager.GetComponentInChildren<PaintManagerSweep_Face>(true).currentLineRendererInfo.keyPointObjectList)
        {
            faceVertices.Add(vertex.transform.position);
        }
        modelManager.GetComponent<RhinoModelSweep>().faceVertices = faceVertices;

        var railVertices = new List<Vector3>();
        foreach (var vertex in paintManager.GetComponentInChildren<PaintManagerSweep_Rail>(true).currentLineRendererInfo.keyPointObjectList)
        {
            railVertices.Add(vertex.transform.position);
        }
        modelManager.GetComponent<RhinoModelSweep>().railVertices = railVertices;

        // try to destory the points and line objects
        // Destory the point and line objects, using photon view
        foreach (var photonView in paintManager.GetComponentsInChildren<PhotonView>(true))
        {
            // determine if the function is on the photon view
            if (photonView.gameObject.GetComponent<PaintManagerBasic>() != null)
            {
                photonView.RPC("DestoryLineAndPoints", RpcTarget.All);
            }
            // photonView.RPC("DestoryLineAndPoints", RpcTarget.All);
        }
        

    }
}
