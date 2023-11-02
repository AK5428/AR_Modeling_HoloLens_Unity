using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.OpenXR;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.Serialization;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;

/// <summary>
/// The class used as paint manager. The main functions are:
/// 1. Detect the user behavior: tap down, tap drag, tap end;
/// 2. For all the behavior, find the position of finger tip (currently using thumb tip);
/// 3. Initial or update the Linerenderer object, to draw the line;
/// </summary>
public class TutorialPaint : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityHandJointHandler, IMixedRealitySourceStateHandler
{
    /// <summary>
    /// Region for the objects in inspector
    /// </summary>
    #region In inspector
    [Header("Prefabs and GameObjects")]
    [SerializeField, FormerlySerializedAs("BrushTip")]
    private GameObject brushTipPrefab;

    [SerializeField, FormerlySerializedAs("PaintBrush")]
    private GameObject paintBrush;
    
    [SerializeField, FormerlySerializedAs("LineRendererPrefab")] 
    private GameObject lineRendererPrefab;

    
    
    
    [Header("Parameters")]
    [SerializeField, FormerlySerializedAs("The Nearness for two line points")]
    private float pointsNearDistance = 0.01f;
    // public float lineWidth = 0.01f;
    // paint parameters
    public float lineWidth = 0.005f;
    public Color lineColor;
    
    
    #endregion

    /// <summary>
    /// Region in private
    /// </summary>
    #region privateParameters
    // [HideInInspector] public bool enablePainting = true;
    // [HideInInspector] public bool inDrawMode = true;
    
    private bool lineEnd = false;
    private bool lineStart = false;


    private GameObject brushTip;
    private Vector3 handPosition = Vector3.zero;
    // [HideInInspector] public List<int> pun_ids = new List<int>();
    // Create a list of GameObjects
    private List<GameObject> lineRenderers = new List<GameObject>();

    private Vector3 lastDrawPointPosition = Vector3.zero;
    private int count = 0;
    // private PhotonView thisPhotonView;
    #endregion
    
    // call in draw setting reader
    public void LineSettingUpdate(float newLineWidth, Color newColor)
    {
        // the line parameters
        lineWidth = newLineWidth;
        lineColor = newColor;
        
        // update the brushtip
        brushTip.transform.localScale = new Vector3(lineWidth, lineWidth, lineWidth);
        brushTip.GetComponent<Renderer>().material.color = lineColor;
    }
    
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
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        lineEnd = false;
        lineStart = true;

        if(eventData.Handedness == Handedness.Right)
        {
            if(brushTip == null)
            {
                if (brushTip == null)
                {
                    brushTip = Instantiate(brushTipPrefab);
                    brushTip.transform.parent = paintBrush.transform;
                    brushTip.transform.position = Vector3.zero;
                    paintBrush.SetActive(true);
                    // brushTip.SetActive(false);
                }
            }
            brushTip.SetActive(true);
        
            // initial the painting objects
            // add the first point
            GameObject lineRendererGameObject = Instantiate(lineRendererPrefab, handPosition, Quaternion.identity);
            lineRendererGameObject.GetComponent<TutorialLineRenderer>().InitialLine();
            lineRendererGameObject.GetComponent<TutorialLineRenderer>().UpdateLine(handPosition, count);
            count++;
            lastDrawPointPosition = handPosition;

            lineRenderers.Add(lineRendererGameObject);
        }
        
    }
    
    // when tap and drag, draw the line.
    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        // initial and debug
        // Debug.Log("Pointer Dragged" + handPosition);

        // Drawline
        if(lineEnd || !lineStart) return;


        // calculate the newPos
        Vector3 newPos = handPosition;

        bool isFarEnough = Vector3.Distance(lastDrawPointPosition, newPos) > pointsNearDistance;

        if (isFarEnough)
        {
            if(lineRenderers.Last() == null) return;
            lineRenderers.Last().GetComponent<TutorialLineRenderer>().UpdateLine(newPos, count);
        
            count++;
            lastDrawPointPosition = newPos;
        }
        
    }
    
    
    // when tap end, end the drawing process
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        // initial and debug
        // Debug.Log("Pointer up");

        // Drawline
        if(lineEnd || !lineStart) return;

        EndCurrentLine();
    }
    
    private void EndCurrentLine()
    {
        lineEnd = true;
        lineStart = false;
        // if(lineRenderers.Count != 0) lineRenderers.Last().GetComponent<LineRenderManager>().enablePainting = false;
        brushTip.SetActive(false);

        // set the parameters to default
        count = 0;

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
                }
                    
                // brushTip.SetActive(false);
            }
        }
    }

    /// <inheritdoc />
    public void OnSourceLost(SourceStateEventData eventData)
    {
        // paintBrush.SetActive(false);
    }
    #endregion
}

