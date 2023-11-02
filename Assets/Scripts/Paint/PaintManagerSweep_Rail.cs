using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;
using Microsoft.MixedReality.Toolkit.Utilities;

public class PaintManagerSweep_Rail : PaintManagerBasic
{
    [Header("Extra Settings")]
    [SerializeField, FormerlySerializedAs("Paint Sweep Switcher")]
    private PaintSweepSwitcher paintSweepSwitcher;

    private Vector3 lastPointPosition = Vector3.zero;
    private int currentFirstKeyPointID = -1;

    public override void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        base.OnPointerDown(eventData);

    }
    
    public override void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        base.OnPointerDragged(eventData);
        if(eventData.Handedness != Handedness.Right) return;
        if(mainMenuVisual.activeInHierarchy) return;
        if(!lineStart) return;

        // judge, if the hand is near the last point, then do nothing
        if (Vector3.Distance(handPosition, lastPointPosition) < pointsNearDistance || !lineStart)
        {
            return;
        }
        
        photonView.RPC("AddKeyPoint", RpcTarget.All, handPosition);
        lastPointPosition = handPosition;
    }

    public override void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        // Debug.Log("OnPointerUp");
        if (!lineStart)
        {
            return;
        }
        
        EndLine();
    }

    private void VisualMenuTest()
    {
        if(mainMenuVisual.activeInHierarchy)
        {
            ReStartPaint();
            paintSweepSwitcher.SwitchPainter();
        }
    }

    private void EndLine()
    {
        // if the points are less than 5, then restart
        if (currentLineRendererInfo.keyPointCount <= 2)
        {
            string message = "The number of rail points is too small, please try again.";
            ReStartPaint(message);

            // initial the rail paint
            lineStart = false;
            paintSweepSwitcher.RestartRailPaint();
            return;
        }

        lineStart = false;

        // switch painter
        paintSweepSwitcher.SwitchPainter();

        // switch to the modeling state
        switcherObject.GetComponent<PaintModelSwitcherSweep>().SwitchMode();
    }

    public void PaintInitial_SweepRail(int firstKeyPointID)
    {
        currentFirstKeyPointID = firstKeyPointID;
        FindPhotonView(gameObject).RPC("RPC_PaintInitial_SweepRail", RpcTarget.All, firstKeyPointID);
        // photonView.RPC("RPC_PaintInitial_SweepRail", RpcTarget.All, firstKeyPointID);
    }

    [PunRPC]
    public void RPC_PaintInitial_SweepRail(int firstKeyPointID)
    {
        // get the gameObject
        GameObject firstKeyPoint = FindPunObject(firstKeyPointID);
        // get the position of the first key point
        Vector3 firstKeyPointPosition = firstKeyPoint.transform.position;

        #region initial

        // instantiate the line and points object
        GameObject lineAndPointsObject = PhotonNetwork.Instantiate(emptyPunPrefab.name, firstKeyPointPosition, Quaternion.identity);
        lineAndPointsObject.name = "Line and Points";
        currentLineRendererInfo.lineAndPoints = lineAndPointsObject;

        // instantiate the key Points all object
        GameObject keyPointsAllObject = PhotonNetwork.Instantiate(emptyPunPrefab.name, firstKeyPointPosition, Quaternion.identity);
        keyPointsAllObject.name = "Key Points All";
        currentLineRendererInfo.keyPointsAllObject = keyPointsAllObject;
        // make the key points all object as a son object of the line and points object
        keyPointsAllObject.transform.parent = lineAndPointsObject.transform;
        
        // instantiate the line renderer object
        GameObject lineRendererObject = PhotonNetwork.Instantiate(lineRendererPrefab.name, firstKeyPointPosition, Quaternion.identity);
        lineRendererObject.name = "Line Renderer";
        currentLineRendererInfo.lineRenderer = lineRendererObject;
        // make the line renderer object as a son object of the line and points object
        lineRendererObject.transform.parent = lineAndPointsObject.transform;
        // initialize the line renderer
        FindPhotonView(lineRendererObject).RPC("RPC_SetLine", RpcTarget.All);

        // instantiate the keyPointObjectList
        currentLineRendererInfo.keyPointObjectList = new List<GameObject>();

        // instantiate the keyPointCount
        currentLineRendererInfo.keyPointCount = 0;

        #endregion

        #region add the first key point
        // add to list
        currentLineRendererInfo.keyPointObjectList.Add(firstKeyPoint);

        // add the key point object to the key points all object, as a son object
        firstKeyPoint.transform.parent = currentLineRendererInfo.keyPointsAllObject.gameObject.transform;

        // add a new position to the line renderer
        FindPhotonView(currentLineRendererInfo.lineRenderer).RPC("RPC_UpdateLine", RpcTarget.All, firstKeyPointPosition, currentLineRendererInfo.keyPointCount);
        currentLineRendererInfo.keyPointCount++;
        #endregion

        lastPointPosition = firstKeyPointPosition;
    }

}
