using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// Main controller, used pun to control the state of the app
/// </summary>
public class MainController : MonoBehaviourPunCallbacks
{
    #region In inspector
    [Header("Controllers")]
    [SerializeField]
    private ModelStateController modelStateController = null;
    public Vector3 adjustPosFeature;

    #endregion

    #region Hide in inspector
    [HideInInspector]
    public List<GameObject> allModelList = new List<GameObject>();

    #endregion

    #region Private fields
    private ModeEnum currentMode = ModeEnum.Default;
    private ModelStateEnum currentModelState = ModelStateEnum.Extrusion;
    private ChooseStateEnum currentChooseState = ChooseStateEnum.Select;
    private AnimateStateEnum currentAnimateState = AnimateStateEnum.Record;

    private PhotonView photonView;

    #endregion

    #region Singleton and Register
    /// <summary>
    /// all the enum used in the app
    /// </summary>
    public enum ModeEnum
    {
        Default,
        Model,
        Choose,
        Adjust,
        Animate
    }

    public enum ModelStateEnum
    {
        Extrusion,
        Revolve,
        Sweep
    }

    public enum ChooseStateEnum
    {
        Select,
        Group
    }

    public enum AnimateStateEnum
    {
        Record,
        Play
    }

    /// <summary>
    /// the register function used to register all enum into pun
    /// </summary>
    /// <param name="mode"></param>
    
    static byte[] SerializeModeEnum(object customType)
    {
        if(customType is ModeEnum)
        {
            var enumValue = (int)(ModeEnum)customType;
            byte[] enumBytes = BitConverter.GetBytes(enumValue);
            return new byte[] { (byte)'A'}.Concat(enumBytes).ToArray();
        }
        else if (customType is ModelStateEnum)
        {
            var enumValue = (int)(ModelStateEnum)customType;
            byte[] enumBytes = BitConverter.GetBytes(enumValue);
            return new byte[] { (byte)'B' }.Concat(enumBytes).ToArray();
        }
        else if (customType is ChooseStateEnum)
        {
            var enumValue = (int)(ChooseStateEnum)customType;
            byte[] enumBytes = BitConverter.GetBytes(enumValue);
            return new byte[] { (byte)'C' }.Concat(enumBytes).ToArray();
        }
        else if (customType is AnimateStateEnum)
        {
            var enumValue = (int)(AnimateStateEnum)customType;
            byte[] enumBytes = BitConverter.GetBytes(enumValue);
            return new byte[] { (byte)'D' }.Concat(enumBytes).ToArray();
        }
        else
        {
            Debug.LogError("The type is not registered");
            return null;
        }
    }

    static object DeserializeEnums(byte[] serializedCustomType)
    {
        byte enumType = serializedCustomType[0];
        int enumValue = BitConverter.ToInt32(serializedCustomType, 1);

        switch (enumType)
        {
            case (byte)'A':
                return (ModeEnum)enumValue;
            case (byte)'B':
                return (ModelStateEnum)enumValue;
            case (byte)'C':
                return (ChooseStateEnum)enumValue;
            case (byte)'D':
                return (AnimateStateEnum)enumValue;
            default:
                Debug.LogError("The type is not registered");
                return null;
        }
    }
    

    #endregion

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // if(PhotonNetwork.IsMasterClient)
        // register the enum
        PhotonPeer.RegisterType(typeof(ModeEnum), (byte)'A', SerializeModeEnum, DeserializeEnums);
        PhotonPeer.RegisterType(typeof(ModelStateEnum), (byte)'B', SerializeModeEnum, DeserializeEnums);
        PhotonPeer.RegisterType(typeof(ChooseStateEnum), (byte)'C', SerializeModeEnum, DeserializeEnums);
        PhotonPeer.RegisterType(typeof(AnimateStateEnum), (byte)'D', SerializeModeEnum, DeserializeEnums);

    }



    #region Function SetMode
    public void SetMode()
    {
        // currentMode = mode;
        Main_UpdateModeAndState();
    }
    public void SetMode(ModeEnum mode)
    {
        currentMode = mode;
        Main_UpdateModeAndState();
    }

    public void SetMode(ModeEnum mode, ModelStateEnum modelState)
    {
        currentMode = mode;
        currentModelState = modelState;
        Main_UpdateModeAndState();
    }

    public void SetMode(ModeEnum mode, ChooseStateEnum chooseState)
    {
        currentMode = mode;
        currentChooseState = chooseState;
        Main_UpdateModeAndState();
    }

    public void SetMode(ModeEnum mode, AnimateStateEnum animateState)
    {
        currentMode = mode;
        currentAnimateState = animateState;
        Main_UpdateModeAndState();
    }
    

    #endregion

    #region Function SetMode
    /// <summary>
    /// The main controller to control mode and state, when the mode is not model, sync to all players
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="modelState"></param>
    /// <param name="chooseState"></param>
    /// <param name="animateState"></param>
    private void Main_UpdateModeAndState()
    {
        // set the model state to default while not in model mode
        if(currentMode == ModeEnum.Model)
        {
            UpdateModelState(true);
        }
        else
        {
            UpdateModelState(false);
        }

        // set the app state 
        switch (currentMode)
        {
            case ModeEnum.Default:
                SetState(SingleModelStateController.ModelTheme.Default);
                break;
            case ModeEnum.Model:
                SetState(SingleModelStateController.ModelTheme.Model, SingleModelStateController.ModelState.Default);
                break;
            case ModeEnum.Adjust:
                SetState(SingleModelStateController.ModelTheme.Adjust);
                break;
            case ModeEnum.Choose:
                SetState(SingleModelStateController.ModelTheme.Choose);
                break;
            case ModeEnum.Animate:
                break;
            default:

                break;
        }
    }


    #endregion
    
    private void UpdateModelState(bool isModelMode)
    {
        if(isModelMode)
        {
            // modelStateController.gameObject.SetActive(true);
            modelStateController.SetState(currentModelState);
        }
        else
        {
            modelStateController.SetAllManagerFalse();
            // modelStateController.gameObject.SetActive(false);
        }
        
    }

    #region Set state for all single model
    // function used to get all the model objects, and then return all the model objects
    private List<GameObject> GetAllModelObjects()
    {
        ModelStorageCaller modelStorageCaller = new ModelStorageCaller();
        return modelStorageCaller.GetModelObjList();
    }

    private void SetState(SingleModelStateController.ModelTheme theme)
    {
        List<GameObject> modelList = GetAllModelObjects();
        foreach (GameObject model in modelList)
        {
            SingleModelStateController singleModelStateController = model.GetComponent<SingleModelStateController>();
            singleModelStateController.SetState(theme);
        }
    }

    private void SetState(SingleModelStateController.ModelTheme theme, SingleModelStateController.ModelState state)
    {
        List<GameObject> modelList = GetAllModelObjects();
        foreach (GameObject model in modelList)
        {
            SingleModelStateController singleModelStateController = model.GetComponent<SingleModelStateController>();
            singleModelStateController.SetState(theme, state);
        }
    }

    private void SetState(SingleModelStateController.ModelTheme theme, SingleModelStateController.ChooseState state)
    {
        List<GameObject> modelList = GetAllModelObjects();
        foreach (GameObject model in modelList)
        {
            SingleModelStateController singleModelStateController = model.GetComponent<SingleModelStateController>();
            singleModelStateController.SetState(theme, state);
        }
    }

    private void SetState(SingleModelStateController.ModelTheme theme, SingleModelStateController.AnimateState state)
    {
        List<GameObject> modelList = GetAllModelObjects();
        foreach (GameObject model in modelList)
        {
            SingleModelStateController singleModelStateController = model.GetComponent<SingleModelStateController>();
            singleModelStateController.SetState(theme, state);
        }
    }

    #endregion

    #region Function ClearModel

    public void ClearModel()
    {
        photonView.RPC("RPC_ClearModel", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ClearModel()
    {
        Debug.Log("Clear model");
        List<GameObject> modelList = GetAllModelObjects();

        Debug.Log("Model list count: " + modelList.Count);

        foreach (GameObject model in modelList)
        {
            int modelId = model.GetComponent<PhotonView>().ViewID;
            model.SetActive(false);
            // remove the model from the model list
            new ModelStorageCaller().RemoveModel(modelId);

            Debug.Log("Model id: " + modelId);
        }
    }

    #endregion

    #region Function SetLock
    public void SetAllLock(GameObject button)
    {
        bool isLock = button.GetComponent<Interactable>().IsToggled;

        List<GameObject> modelList = GetAllModelObjects();
        foreach (GameObject model in modelList)
        {
            int modelId = model.GetComponent<PhotonView>().ViewID;
            photonView.RPC("RPC_SetLock", RpcTarget.All, modelId, isLock);
        }
    }

    [PunRPC]
    private void RPC_SetLock(int modelId, bool isLock)
    {
        GameObject model = PhotonView.Find(modelId).gameObject;
        model.GetComponent<SingleModelStateController>().UpdateLockState(isLock);
    }
    #endregion

}
