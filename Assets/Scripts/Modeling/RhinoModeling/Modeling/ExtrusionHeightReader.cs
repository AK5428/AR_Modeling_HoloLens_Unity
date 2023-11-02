using System.Collections.Generic;
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
public class ExtrusionHeightReader : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityHandJointHandler, IMixedRealitySourceStateHandler
{
    private Vector3 handPosition = Vector3.zero;
    private RhinoModelExtrusion modelManager;

    private void Start()
    {
        modelManager = GetComponent<RhinoModelExtrusion>();
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
        if(eventData.Handedness != Handedness.Right) return;

        modelManager.EndModeling();
        
    }

    
    // when tap and drag, draw the line.
    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
         
        
    }


    
    // when tap end, end the drawing process
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
       
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
                Vector3 firstPointPos = modelManager.faceVertices[0];
                Vector3 planeNormal = modelManager.planeNormal;
                float height = -Vector3.Dot(planeNormal, handPosition - firstPointPos);
                modelManager.currentExtrusionHeight = height;
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
            
        }
    }

    /// <inheritdoc />
    public void OnSourceLost(SourceStateEventData eventData)
    {
        // paintBrush.SetActive(false);
    }
    #endregion
}

