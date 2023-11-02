using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Photon.Pun;
using UnityEngine.Serialization;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;

public class PaintManagerSweep_Face : PaintManagerBasic
{
    [Header("Extra Parameters")]
    [SerializeField, FormerlySerializedAs("First Key Point Color")]
    private Color firstKeyPointColor = Color.red;
    [SerializeField, FormerlySerializedAs("Paint Sweep Switcher")]
    private PaintSweepSwitcher paintSweepSwitcher;

    private Vector3 lastPointPosition = Vector3.zero;
    private Vector3 planeNormal = Vector3.zero;

    private bool lineEnd = false;
    
    public override void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if(eventData.Handedness != Handedness.Right) return;
        if(mainMenuVisual.activeInHierarchy) return;
        
        lineEnd = false;
        
        photonView.RPC("PaintInitial", RpcTarget.All);
        photonView.RPC("AddFirstKeyPoint", RpcTarget.All, handPosition);
        lastPointPosition = handPosition;
    }

    public override void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        base.OnPointerDragged(eventData);
        if(eventData.Handedness != Handedness.Right) return;
        if(!lineStart) return;

        // judge, if the hand is near the last point, then do nothing
        if (Vector3.Distance(handPosition, lastPointPosition) < pointsNearDistance || lineEnd)
        {
            return;
        }

        if(mainMenuVisual.activeInHierarchy)
        {
            EndCurrentLine();
        }
        
        #region Adjust the points to one plane

        int borderIndex = 10;

        Vector3 newPointPosition = Vector3.zero;
        // normally create the points in the first area
        if(currentLineRendererInfo.keyPointCount < borderIndex)
        {
            newPointPosition = handPosition;
        }

        // if the points are in the border, calculate the plane and adjust the points to the plane
        if(currentLineRendererInfo.keyPointCount == borderIndex)
        {
            CalculatePlane(handPosition);
            LineUpdate(planeNormal);
            newPointPosition = NearestPosition(handPosition, planeNormal);
        }

        // if the points are in the second area, adjust the points to the plane
        if(currentLineRendererInfo.keyPointCount > borderIndex)
        {
            newPointPosition = NearestPosition(handPosition, planeNormal);
        }

        #endregion

        // judge the distance
        if (Vector3.Distance(newPointPosition, lastPointPosition) < pointsNearDistance)
        {
            return;
        }
        photonView.RPC("AddKeyPoint", RpcTarget.All, newPointPosition);
        lastPointPosition = newPointPosition;

        #region Auto close
        // if the final point is near the first point, then close the line
        if (currentLineRendererInfo.keyPointCount > borderIndex)
        {
            Vector3 firstPointPosition = currentLineRendererInfo.keyPointObjectList[0].transform.position;
            if (Vector3.Distance(handPosition, firstPointPosition) < pointsNearDistance * 3)
            {
                EndCurrentLine();
            }
        }
        #endregion
    }

    public override void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        // Debug.Log("OnPointerUp");

        if(!lineStart) return;
        
        EndCurrentLine();
    }

    public override void EndCurrentLine()
    {
        base.EndCurrentLine();
        if(currentLineRendererInfo.keyPointCount <= 10) return;

        // get the position of the first key point
        Vector3 firstKeyPointPosition = currentLineRendererInfo.keyPointObjectList[0].transform.position;
        // instantiate the key point object
        photonView.RPC("AddKeyPoint", RpcTarget.All, firstKeyPointPosition);
        // change the color of the key point object
        // currentLineRendererInfo.keyPointObjectList[currentLineRendererInfo.keyPointObjectList.Count - 1].GetComponentInChildren<MeshRenderer>().material.color = firstKeyPointColor;
        lineEnd = true;
        lineStart = false;

        // get a copy of first key point, then pass it to the rail (to the switcher first), switch the painter
        // instantiate the key point object
        GameObject keyPointObject = PhotonNetwork.Instantiate(keyPointPrefab.name, firstKeyPointPosition, Quaternion.identity);
        // change the color of the key point object
        // keyPointObject.GetComponentInChildren<MeshRenderer>().material.color = firstKeyPointColor;

        // pass the first key point to the switcher
        paintSweepSwitcher.SwitchPainter(FindPhotonView(keyPointObject).ViewID);
    }


    public int InitalFirstPointForRail()
    {
        Vector3 firstKeyPointPosition = currentLineRendererInfo.keyPointObjectList[0].transform.position;

        // get a copy of first key point, then pass it to the rail (to the switcher first), switch the painter
        // instantiate the key point object
        GameObject keyPointObject = PhotonNetwork.Instantiate(keyPointPrefab.name, firstKeyPointPosition, Quaternion.identity);
        // change the color of the key point object
        keyPointObject.GetComponentInChildren<MeshRenderer>().material.color = firstKeyPointColor;

        // pass the first key point to the switcher
        return FindPhotonView(keyPointObject).ViewID;
    }


    private void CalculatePlane(Vector3 currentHandPosition)
    {
        // get the first three points
        Vector3 point1 = currentLineRendererInfo.keyPointObjectList[2].transform.position;
        // get the point at the middle of the list
        Vector3 point2 = currentLineRendererInfo.keyPointObjectList[currentLineRendererInfo.keyPointCount / 2].transform.position;
        //get the point at the end of the list
        Vector3 point3 = currentHandPosition;

        // calculate the plane
        Vector3 vector1 = point2 - point1;
        Vector3 vector2 = point3 - point1;
        planeNormal = Vector3.Cross(vector1, vector2).normalized;

        float angle = Vector3.Angle(planeNormal, Vector3.up);
        if(angle > 45 && angle < 135)
        {
            // UnityEngine.Debug.Log("Angle is closer to vertical");
            planeNormal = Vector3.Cross(vector2, Vector3.up).normalized;
        }else{
            // UnityEngine.Debug.Log("Angle is closer to horizontal");
            planeNormal = -Vector3.up;
        }
    }

    [PunRPC]
    public void AddFirstKeyPoint(Vector3 keyPointPosition)
    {
        // instantiate the key point object
        GameObject keyPointObject = PhotonNetwork.Instantiate(keyPointPrefab.name, keyPointPosition, Quaternion.identity);
        currentLineRendererInfo.keyPointObjectList.Add(keyPointObject);

        // change the color of the key point object
        // keyPointObject.GetComponentInChildren<MeshRenderer>().material.color = firstKeyPointColor;

        // add the key point object to the key points all object, as a son object
        keyPointObject.transform.parent = currentLineRendererInfo.keyPointsAllObject.gameObject.transform;

        // add a new position to the line renderer
        FindPhotonView(currentLineRendererInfo.lineRenderer).RPC("RPC_UpdateLine", RpcTarget.All, keyPointPosition, currentLineRendererInfo.keyPointCount);
        currentLineRendererInfo.keyPointCount++;
    }
}
