using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Serialization;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.Utilities;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;

public class PaintManagerRevolve : PaintManagerBasic
{
    [Header("Extra Prefabs")]
    [SerializeField, FormerlySerializedAs("Axis Prefab")]
    public GameObject axisPrefab;

    // private parameters
    private Vector3 lastPointPosition = Vector3.zero;
    private GameObject axisObject;
    private Vector3 planeNormal = Vector3.zero;

    private bool lineEnd = false;


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        // add an extra axis to the pool
        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            if (axisPrefab != null) pool.ResourceCache.Add(axisPrefab.name, axisPrefab);
        }
    }

    public override void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if(eventData.Handedness != Handedness.Right) return;
        if(mainMenuVisual.activeInHierarchy) return;

        lineEnd = false;

        photonView.RPC("PaintInitial", RpcTarget.All);
        photonView.RPC("AddKeyPoint", RpcTarget.All, handPosition);
        photonView.RPC("AxisInitial", RpcTarget.All, handPosition);
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

        #region Adjust the points
        // the points should be on one plane, and also, do not exceed the axis

        int borderIndexStart = 6;
        int borderIndexEnd = 10;

        Vector3 newPointPosition = Vector3.zero;
        // normally create the points in the first area
        if (currentLineRendererInfo.keyPointCount < borderIndexStart)
        {
            newPointPosition = handPosition;
        }
        // calculate the plane for the second area
        if (currentLineRendererInfo.keyPointCount >= borderIndexStart && currentLineRendererInfo.keyPointCount < borderIndexEnd)
        {
            CalculatePlane(handPosition);
            LineUpdate(planeNormal);
            newPointPosition = handPosition;
        }
        // if the points are in the second area, adjust the points to the plane
        if (currentLineRendererInfo.keyPointCount >= borderIndexEnd)
        {
            newPointPosition = NearestPosition(handPosition, planeNormal);
            newPointPosition = PosUpdateForChosenSide(newPointPosition, true);
        }
        
        #endregion

        // judge the distance
        if (Vector3.Distance(newPointPosition, lastPointPosition) < pointsNearDistance)
        {
            return;
        }

        photonView.RPC("AddKeyPoint", RpcTarget.All, newPointPosition);
        lastPointPosition = newPointPosition;
    }

    public override void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        EndCurrentLine();
    }

    public override void EndCurrentLine()
    {
        base.EndCurrentLine();
        if(currentLineRendererInfo.keyPointCount <= 10) return;

        // add an end point at the line
        Vector3 newPointPosition = PosUpdateForChosenSide(handPosition, false);
        photonView.RPC("AddKeyPoint", RpcTarget.All, newPointPosition);

        // add an end point where the first point is
        photonView.RPC("AddKeyPoint", RpcTarget.All, currentLineRendererInfo.keyPointObjectList[0].transform.position);
        
        lineEnd = true;

        switcherObject.GetComponent<PaintModelSwitcherRevolve>().SwitchMode();
    }

     private Vector3 PosUpdateForChosenSide(Vector3 inputPos, bool judgeSide)
    {
        // calculate the direction
        Vector3 direction = Vector3.left;

        // Positions
        Vector3 P1 = currentLineRendererInfo.keyPointObjectList[0].transform.position;
        Vector3 P2 = currentLineRendererInfo.keyPointObjectList[5].transform.position;
        Vector3 P3 = inputPos;

        //determine which side of the line P2 and P3 are on
        Vector3 P1P2 = P2 - P1;
        Vector3 P1P3 = P3 - P1;

        float sideP2 = Vector3.Dot(P1P2, direction);
        float sideP3 = Vector3.Dot(P1P3, direction);

        if(Mathf.Sign(sideP2) == Mathf.Sign(sideP3) && judgeSide)
        {
            // on the same side
            // UnityEngine.Debug.Log("On the same side");
            return inputPos;

            
        }else{
            // on the different side
            // return the nearest point on the line
            // UnityEngine.Debug.Log("On the different side");
            Vector3 projection = Vector3.Project(P1P3, Vector3.up);
            Vector3 nearestPos = P1 + projection;
            return nearestPos;
        }

        
    }

    private void CalculatePlane(Vector3 currentHandPos)
    {
        // get the first points
        Vector3 point1 = currentLineRendererInfo.keyPointObjectList[1].transform.position;
        Vector3 point2 = currentHandPos;

        
        // calculate the plane
        Vector3 vector1 = point2 - point1;
        Vector3 vector2 = Vector3.up;
        planeNormal = Vector3.Cross(vector1, vector2).normalized;
    }
    
    #region PUN functions

    [PunRPC]
    private void AxisInitial(Vector3 axisPosition)
    {
        axisObject = PhotonNetwork.Instantiate(axisPrefab.name, axisPosition, Quaternion.identity);
    }

    [PunRPC]
    public override void DestoryLineAndPoints()
    {
        base.DestoryLineAndPoints();

        if (axisObject != null)
        {
            PhotonNetwork.Destroy(axisObject);
        }
    }
    #endregion
}
