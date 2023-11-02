using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.Serialization;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;
using Photon.Pun;

public class PaintManagerBasic : PhotonMonoBehaviour, IMixedRealityPointerHandler, IMixedRealityHandJointHandler, IMixedRealitySourceStateHandler
{
    #region In inspector
    [Header("Prefabs and GameObjects")]
    [SerializeField, FormerlySerializedAs("PUN_Line and Points")]
    public GameObject emptyPunPrefab;

    [SerializeField, FormerlySerializedAs("BrushTip")]
    public GameObject brushTipPrefab;

    [SerializeField, FormerlySerializedAs("PaintBrush")]
    public GameObject paintBrush;
    
    [SerializeField, FormerlySerializedAs("LineRendererPrefab")] 
    public GameObject lineRendererPrefab;

    [SerializeField, FormerlySerializedAs("Key Point")] 
    public GameObject keyPointPrefab;

    [SerializeField, FormerlySerializedAs("Paint Model Switcher")]
    public GameObject switcherObject;

    [SerializeField, FormerlySerializedAs("Main Menu Visual")]
    public GameObject mainMenuVisual;
    
    
    [Header("Parameters")]
    [SerializeField, FormerlySerializedAs("The Nearness for two line points")]
    public float pointsNearDistance = 0.01f;
    public float lineWidth = 0.002f;
    
    // set line color to 0080D6 in hex
    public Color lineColor = ColorUtility.TryParseHtmlString("#0080D6", out Color color) ? color : Color.blue;
    
    #endregion

    #region Public variables hide in inspector
    [HideInInspector]
    public PhotonView photonView;
    [HideInInspector]
    public Vector3 handPosition = Vector3.zero;
    [HideInInspector]
    public bool lineStart = false;

    #endregion

    #region Private variables
    private GameObject brushTip;
    
    // private List<GameObject> KeyPoints = new List<GameObject>();

    public struct LineRendererInfo
    {
        public GameObject lineAndPoints;
        public GameObject lineRenderer;
        public GameObject keyPointsAllObject;
        public List<GameObject> keyPointObjectList;
        public int keyPointCount;
    }

    public LineRendererInfo currentLineRendererInfo;

    #endregion

    public virtual void Start()
    {
        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            // if (brushTipPrefab != null) pool.ResourceCache.Add(brushTipPrefab.name, brushTipPrefab);
            if (lineRendererPrefab != null && !pool.ResourceCache.ContainsKey(lineRendererPrefab.name))
            {
                pool.ResourceCache.Add(lineRendererPrefab.name, lineRendererPrefab);
            }
            if (keyPointPrefab != null && !pool.ResourceCache.ContainsKey(keyPointPrefab.name))
            {
                pool.ResourceCache.Add(keyPointPrefab.name, keyPointPrefab);
            }
            if (emptyPunPrefab != null && !pool.ResourceCache.ContainsKey(emptyPunPrefab.name))
            {
                pool.ResourceCache.Add(emptyPunPrefab.name, emptyPunPrefab);
            }
        }

        photonView = GetComponent<PhotonView>();

    }

    public virtual void EndCurrentLine()
    {
        // if the points are less than 5, then restart
        if (currentLineRendererInfo.keyPointCount <= 10)
        {
            lineStart = false;
            if (currentLineRendererInfo.keyPointCount < 7)
            {
                ReStartPaint();
            } else{
                string message = "The number of points is too small, please try again.";
                ReStartPaint(message);
            }
            return;
        }
    }

    public virtual void ReStartPaint()
    {
        if(currentLineRendererInfo.lineAndPoints != null)
        {
            photonView.RPC("DestoryLineAndPoints", RpcTarget.All);
            currentLineRendererInfo = new LineRendererInfo();
        }
    }

    public virtual void ReStartPaint(string dialogMessage)
    {
        if(currentLineRendererInfo.lineAndPoints != null)
        {
            photonView.RPC("DestoryLineAndPoints", RpcTarget.All);
            currentLineRendererInfo = new LineRendererInfo();
        }

        DialogCaller dialogCaller = new DialogCaller();
        dialogCaller.CallConfirmationDialog(dialogMessage);
    }

    public void LineUpdate(Vector3 planeNormal)
    {
        for (int i = 0; i < currentLineRendererInfo.keyPointCount; i++)
        {
            Vector3 keyPointPosition = currentLineRendererInfo.keyPointObjectList[i].transform.position;
            Vector3 nearestPosition = NearestPosition(keyPointPosition, planeNormal);
            photonView.RPC("UpdateLineAndPoint", RpcTarget.All, nearestPosition, i);
        }
    }

    public Vector3 NearestPosition(Vector3 inputPos, Vector3 planeNormal)
    {
        Vector3 nearestPos = new Vector3();
        Vector3 firstPoint = currentLineRendererInfo.keyPointObjectList[0].transform.position;
        
        float distance = Vector3.Dot(planeNormal, inputPos - firstPoint);
        nearestPos = inputPos - distance * planeNormal;
        return nearestPos;
    }

    public void BrushTipInitial()
    {
        // Debug.Log("Hand detected");
        if (paintBrush != null)
        {
            paintBrush.SetActive(true);
            if (brushTip == null)
            {
                brushTip = Instantiate(brushTipPrefab);
                brushTip.transform.parent = paintBrush.transform;
                brushTip.transform.position = Vector3.zero;
                paintBrush.SetActive(true);
                brushTip.SetActive(false);

                // set the parameters for brushtip
                // update the brushtip
                brushTip.transform.localScale = new Vector3(lineWidth, lineWidth, lineWidth);
                brushTip.GetComponent<Renderer>().material.color = lineColor;
            }
        }
    }

    [PunRPC]
    public virtual void DestoryLineAndPoints()
    {
        if(currentLineRendererInfo.lineAndPoints == null) return;

        // destory the key point object
        foreach (var keyPointObject in currentLineRendererInfo.keyPointObjectList)
        {
            PhotonNetwork.Destroy(keyPointObject);
        }

        // destory the line renderer object
        PhotonNetwork.Destroy(currentLineRendererInfo.lineRenderer);

        // destory the key points all object
        PhotonNetwork.Destroy(currentLineRendererInfo.keyPointsAllObject);

        // destory the line and points object
        PhotonNetwork.Destroy(currentLineRendererInfo.lineAndPoints);
    }

    #region PUN functions

    [PunRPC]
    public void PaintInitial()
    {
        currentLineRendererInfo = new LineRendererInfo();

        // instantiate the line and points object
        GameObject lineAndPointsObject = PhotonNetwork.Instantiate(emptyPunPrefab.name, Vector3.zero, Quaternion.identity);
        lineAndPointsObject.name = "Line and Points";
        currentLineRendererInfo.lineAndPoints = lineAndPointsObject;

        // instantiate the key Points all object
        GameObject keyPointsAllObject = PhotonNetwork.Instantiate(emptyPunPrefab.name, Vector3.zero, Quaternion.identity);
        keyPointsAllObject.name = "Key Points All";
        currentLineRendererInfo.keyPointsAllObject = keyPointsAllObject;
        // make the key points all object as a son object of the line and points object
        keyPointsAllObject.transform.parent = lineAndPointsObject.transform;
        
        // instantiate the line renderer object
        GameObject lineRendererObject = PhotonNetwork.Instantiate(lineRendererPrefab.name, Vector3.zero, Quaternion.identity);
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
    }

    [PunRPC]
    public void AddKeyPoint(Vector3 keyPointPosition)
    {
        // instantiate the key point object
        GameObject keyPointObject = PhotonNetwork.Instantiate(keyPointPrefab.name, keyPointPosition, Quaternion.identity);
        currentLineRendererInfo.keyPointObjectList.Add(keyPointObject);

        // add the key point object to the key points all object, as a son object
        keyPointObject.transform.parent = currentLineRendererInfo.keyPointsAllObject.gameObject.transform;

        // add a new position to the line renderer
        FindPhotonView(currentLineRendererInfo.lineRenderer).RPC("RPC_UpdateLine", RpcTarget.All, keyPointPosition, currentLineRendererInfo.keyPointCount);
        currentLineRendererInfo.keyPointCount++;
    }

    [PunRPC]
    public void UpdateLineAndPoint(Vector3 pointPosition, int pointIndex)
    {
        // update the line renderer
        FindPhotonView(currentLineRendererInfo.lineRenderer).RPC("RPC_UpdateLine", RpcTarget.All, pointPosition, pointIndex);

        // update the key point object
        FindPhotonView(currentLineRendererInfo.keyPointObjectList[pointIndex]).RPC("RPC_ChangePosition", RpcTarget.All, pointPosition);
    }

    [PunRPC]
    public void CloseLine()
    {
        Vector3 firstPointPosition = currentLineRendererInfo.keyPointObjectList[0].transform.position;
        // close the line renderer
        FindPhotonView(currentLineRendererInfo.lineRenderer).RPC("RPC_UpdateLine", RpcTarget.All,firstPointPosition, currentLineRendererInfo.keyPointCount);
    }

    #endregion
  
    
    #region IMixedRealityPointerHandler implementation
    
    private void OnEnable()
    {
        // settings to detect user behavior. Three handler are used.
        PointerUtils.SetGazePointerBehavior(PointerBehavior.AlwaysOff);
        // PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOff);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
    }
    
    // corresponding with the OnEnable function.
    private void OnDisable()
    {
        // PointerUtils.SetGazePointerBehavior(PointerBehavior.Default);
        // PointerUtils.SetHandRayPointerBehavior(PointerBehavior.Default);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHandJointHandler>(this);
    }
    
    /// <summary>
    /// Photon function for Paint Manager. Used to store all the line renderer objects.
    /// These objects can only be found by id.
    /// </summary>
    /// <param name="viewId"></param>
    #region controll functions

   
    // void UpdateList(int viewId)
    // {
    //     object_ids.Add(viewId);
    // }

    #endregion
    
    /// <summary>
    /// Main handler to detect user behavior. Different functions are set for different behavior.
    /// </summary>
    /// <param name="eventData"></param>
    #region IMixedRealityPointerHandler
    
    // when tap down, create the line renderer object, then call the initial function on the object itself.
    // save the object id to list.
    public virtual void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if(eventData.Handedness != Handedness.Right) return;
        if(brushTip == null) BrushTipInitial();

        brushTip.SetActive(true);
        lineStart = true;
    }

    
    // when tap and drag, draw the line.
    public virtual void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if(eventData.Handedness != Handedness.Right) return;
        if(!lineStart) return;
        
    }


    
    // when tap end, end the drawing process
    public virtual void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if(eventData.Handedness != Handedness.Right) return;
        // if(!lineStart) return;

        if(brushTip != null) brushTip.SetActive(false);
    }
    

    // unused
    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}
    #endregion
    
    /// <summary>
    /// The handler used to get the finger tip position
    /// </summary>
    /// <param name="eventData"></param>
    #region IMixedRealityHandJointHandler

    public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
    {
        if (eventData.Handedness == Handedness.Right)
        {
            MixedRealityPose thumbJointPose = eventData.InputData[TrackedHandJoint.ThumbTip];
            if (thumbJointPose != null)
            {
                handPosition = thumbJointPose.Position;
            }
        }
    }
    #endregion
    
    /// <summary>
    /// The region for source detect
    /// Initial the paint objects while detect hands.
    /// </summary>
    /// <param name="eventData"></param>
    #region IMixedRealitySourceStateHandler
    public void OnSourceDetected(SourceStateEventData eventData)
    {
        var hand = eventData.Controller;
        if (hand != null && hand.ControllerHandedness == Handedness.Right)
        {
            BrushTipInitial();
        }
    }

    /// <inheritdoc />
    public void OnSourceLost(SourceStateEventData eventData)
    {
        // paintBrush.SetActive(false);
        if(eventData.Controller.ControllerHandedness == Handedness.Right)
        EndCurrentLine();
    }
    #endregion

    #endregion
}
