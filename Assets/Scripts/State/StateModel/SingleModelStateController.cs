using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Photon.Pun;
using MRTK.Tutorials.MultiUserCapabilities;

public class SingleModelStateController : PhotonMonoBehaviour
{
    #region In Inspector
    [Header("Internal Objects")]
    [SerializeField, FormerlySerializedAs("Bounding Menu Object")] 
    public GameObject boundingMenu;

    [SerializeField, FormerlySerializedAs("App bar")] 
    private GameObject appBarObj;

    [SerializeField, FormerlySerializedAs("The root of the group son objects")]
    public GameObject groupSonObjRoot;

    [SerializeField, FormerlySerializedAs("Cube shown while chosen")]
    public GameObject chosenCube;

    [SerializeField, FormerlySerializedAs("Chosen button object")]
    public GameObject chosenBtnObj;

    #endregion

    #region HideInInspector
    [HideInInspector]
    public GameObject thisRhinoModel;



    #endregion

    #region Private Variables
    private AppBar appBar;
    private Vector3 lastFatherScale;
    private ChooseState lastChooseState = ChooseState.Prepare;
    private float constantModelScale_Multi;
    // private Vector3 lastBoundingMenuPos;
    // private Vector3 adjustPosFeature;

    #endregion

    
    #region Enum Define
    public enum ModelTheme
    {
        Default,
        Model,
        Choose,
        Adjust,
        Animate
    }

    public enum ModelState
    {
        Default
    }

    public enum AdjustState
    {
        Unlock,
        Lock
    }

    public enum ChooseState
    {
        Prepare,
        Chosen,
        Group_parent,
        Group_son
    }

    public enum AnimateState
    {
        Record,
        Play
    }

    #endregion

    #region State Define
    // Current model theme
    [Header("Current Model State")]
    [SerializeField]
    private ModelTheme modelTheme = ModelTheme.Model;

    public ModelTheme currentModelTheme
    {
        get { return modelTheme; }
        set { modelTheme = value; }
    }
    
    // Current model state
    [SerializeField]
    private ModelState modelState = ModelState.Default;

    public ModelState currentModelState
    {
        get { return modelState; }
        set { modelState = value; }
    }

    // Current adjust state
    [SerializeField]
    private AdjustState adjustState = AdjustState.Unlock;

    public AdjustState currentAdjustState
    {
        get { return adjustState; }
        set { adjustState = value; }
    }

    // Current choose state
    [SerializeField]
    private ChooseState chooseState = ChooseState.Prepare;

    public ChooseState currentChooseState
    {
        get { return chooseState; }
        set { chooseState = value; }
    }

    // Current animate state
    [SerializeField]
    private AnimateState animateState = AnimateState.Record;

    public AnimateState currentAnimateState
    {
        get { return animateState; }
        set { animateState = value; }
    }

    #endregion

    

    private void Start()
    {
        // get the app bar
        appBar = appBarObj.GetComponent<AppBar>();
        // lastBoundingMenuPos = boundingMenu.transform.position;
        UpdateModelState();

        // adjustPosFeature = new MainControlCaller().GetAdjustFeature();
        boundingMenu.GetComponent<GenericNetSyncObject>().adjustPosFeature = new MainControlCaller().GetAdjustFeature();
    }

    private void Update()
    {
        // update the scale

        if( lastChooseState != ChooseState.Group_son && currentChooseState == ChooseState.Group_son)
        {
            lastFatherScale = GetFatherScale();
            constantModelScale_Multi = transform.localScale.x * lastFatherScale.x;
            lastChooseState = currentChooseState;
            return;
        }
        if(currentChooseState == ChooseState.Group_son)
        {
            Vector3 currentFatherScale = GetFatherScale();

            if(currentFatherScale == lastFatherScale) return;
            
            // get the scale ratio
            float ratioForfather = constantModelScale_Multi / currentFatherScale.x;

            // get the scale for son
            float ratioForSon = ratioForfather / transform.localScale.x;

            // scale this object to keep the size static
            transform.localScale = Vector3.one * ratioForfather;

            // scale the bouding menu to keep the size static
            boundingMenu.transform.localScale /= ratioForSon;

            lastFatherScale = currentFatherScale;
        }

        // // update the position
        Vector3 currentBoundingMenuPos = boundingMenu.transform.localPosition;
        if(currentBoundingMenuPos == Vector3.zero) return;
        Vector3 offset = currentBoundingMenuPos;
        transform.position += offset;
        boundingMenu.transform.localPosition = Vector3.zero;
        
        
    }

    private Vector3 GetFatherScale()
    {
        Vector3 currentFatherScale = new Vector3(1, 1, 1);
        if(transform.parent.parent != null)
        {
            currentFatherScale = transform.parent.parent.localScale;
        }

        return currentFatherScale;
    }

    private void GetThisRhinoModel()
    {
        foreach (Transform child in boundingMenu.transform)
        {
            if (child.tag == "RhinoModel" )
            {
                thisRhinoModel = child.gameObject;
                break;
            }
        }
    }

    public void UpdateModelState()
    {
        appBarObj.SetActive(true);
        appBar = appBarObj.GetComponent<AppBar>();
        
        if(currentChooseState == ChooseState.Group_son) 
        {
            appBar.State = AppBar.AppBarStateEnum.None;
            SetCollider(false);
            SetManipulation(false);
            return;
        }
        switch (currentModelTheme)
        {
            case ModelTheme.Default:
                appBar.State = AppBar.AppBarStateEnum.None;
                SetCollider(false);
                SetManipulation(false);
                break;

            case ModelTheme.Model:
                SetCollider(false);
                appBar.State = AppBar.AppBarStateEnum.None;
                SetManipulation(false);
                break;

            case ModelTheme.Adjust:
                switch (currentAdjustState)
                {
                    case AdjustState.Unlock:
                        SetCollider(true);
                        SetManipulation(true);
                        appBar.State = AppBar.AppBarStateEnum.Default;
                        break;
                    case AdjustState.Lock:
                        SetCollider(false);
                        SetManipulation(false);
                        appBar.State = AppBar.AppBarStateEnum.Hidden;
                        break;
                }
                break;

            case ModelTheme.Choose:
                SetManipulation(false);
                SetCollider(false);
                switch (currentChooseState)
                {
                    case ChooseState.Prepare:
                        appBar.State = AppBar.AppBarStateEnum.Choose;
                        chosenBtnObj.GetComponent<Interactable>().IsToggled = false;
                        break;
                    case ChooseState.Chosen:
                        appBar.State = AppBar.AppBarStateEnum.Choose;
                        break;
                    case ChooseState.Group_parent:
                        appBar.State = AppBar.AppBarStateEnum.Group_parent;
                        boundingMenu.GetComponent<BoxCollider>().enabled = true;
                        break;
                    case ChooseState.Group_son:
                        appBar.State = AppBar.AppBarStateEnum.None;
                        break;
                }
                break;
            case ModelTheme.Animate:
                
                switch (currentAnimateState)
                {
                    case AnimateState.Record:
                        break;
                    case AnimateState.Play:
                        break;
                }
                break;
        }
    }

    private void SetCollider(bool isOn)
    {
        if (isOn)
        {
            boundingMenu.GetComponent<BoxCollider>().enabled = true;
            chosenCube.GetComponent<BoxCollider>().enabled = true;
        }
        else
        {
            boundingMenu.GetComponent<BoxCollider>().enabled = false;
            chosenCube.GetComponent<BoxCollider>().enabled = false;
        }
    }


    private void SetManipulation(bool isOn)
    {
        if (isOn)
        {
            boundingMenu.GetComponent<ObjectManipulator>().enabled = true;
        }
        else
        {
            boundingMenu.GetComponent<ObjectManipulator>().enabled = false;
        }
    }

    #region Update Chosen State
    // main caller
    public void UpdateChosenState()
    {
        int ControllerId = GetComponent<PhotonView>().ViewID;

        bool isToggled = chosenBtnObj.GetComponent<Interactable>().IsToggled;

        GetComponent<PhotonView>().RPC("RPC_UpdateChosenState", RpcTarget.All, ControllerId, isToggled);
    }

    // local function 
    public void Local_UpdateChosenState(bool isToggled)
    {
        if (isToggled)
        {
            chosenCube.SetActive(true);
            currentChooseState = ChooseState.Chosen;
        }
        else
        {
            chosenCube.SetActive(false);
            currentChooseState = ChooseState.Prepare;
        }
    }

    [PunRPC]
    private void RPC_UpdateChosenState(int ControllerId, bool isToggled)
    {
        Debug.Log("RPC_UpdateChosenState");
        GameObject controller = PhotonView.Find(ControllerId).gameObject;
        SingleModelStateController singleModelStateController = controller.GetComponent<SingleModelStateController>();
        singleModelStateController.Local_UpdateChosenState(isToggled);
    }

    #endregion



    // update lock state
    public void UpdateLockState(bool isLock)
    {
        int viewId = GetComponent<PhotonView>().ViewID;
        GetComponent<PhotonView>().RPC("RPC_UpdateLockState", RpcTarget.All, viewId, isLock);
    }

    [PunRPC]
    private void RPC_UpdateLockState(int viewId, bool isLock)
    {
        GameObject modelObj = PhotonView.Find(viewId).gameObject;
        SingleModelStateController singleModelStateController = modelObj.GetComponent<SingleModelStateController>();
        singleModelStateController.UpdateLockState_Local(isLock);
    }

    public void UpdateLockState_Local(bool isLock)
    {
        if(isLock)
        {
            SetState(AdjustState.Lock);
        }
        else
        {
            SetState(AdjustState.Unlock);
        }
    }

    #region Functions to set all state
    /// <summary>
    /// Set the state of the model
    /// </summary>
    /// <param name="theme"> The priority state of the model, first class</param>
    /// <param name="state"> The secondary state of the model for the currnet theme</param>
    public void SetState(ModelTheme theme)
    {
        currentModelTheme = theme;
        UpdateModelState();
    }
    public void SetState(ModelTheme theme, ModelState state)
    {
        currentModelTheme = theme;
        currentModelState = state;
        UpdateModelState();
    }

    public void SetState(ModelTheme theme, AdjustState state)
    {
        currentModelTheme = theme;
        currentAdjustState = state;
        UpdateModelState();
    }

    public void SetState(AdjustState state)
    {
        // currentModelTheme = theme;
        currentAdjustState = state;
        UpdateModelState();
    }

    public void SetState(ModelTheme theme, ChooseState state)
    {
        currentModelTheme = theme;
        currentChooseState = state;
        UpdateModelState();
    }

    public void SetState(ModelTheme theme, AnimateState state)
    {
        currentModelTheme = theme;
        currentAnimateState = state;
        UpdateModelState();
    }

    #endregion
    
    #region Functions to group and ungroup

    /// <summary>
    /// Group functions used to group all chosen objects
    /// </summary>
    public void Group()
    {
        // get all chosen objects   
        List<GameObject> chosenObjs = GetAllChosenObj();
        if(chosenObjs.Count == 0) return;

        // get the group parent
        int fatherControllerId = GetComponent<PhotonView>().ViewID;

        // set the father object to group parent state
        GetComponent<PhotonView>().RPC("SetStateForGroupParent", RpcTarget.All, fatherControllerId);

        // for the son objects
        foreach (GameObject chosenObj in chosenObjs)
        {
            int chosenControllerId = chosenObj.GetComponent<PhotonView>().ViewID;
            if (chosenControllerId != fatherControllerId)
            {
                // first, set all chosen objects (except the father object) to default state
                GetComponent<PhotonView>().RPC("SetStateForGroupSon", RpcTarget.All, chosenControllerId);

                // third, set the son objects to the group parent
                GetComponent<PhotonView>().RPC("SetSonObjsToGroupParent", RpcTarget.All, chosenControllerId, fatherControllerId);
            }
        }
        
    }

    [PunRPC]
    private void SetStateForGroupSon(int modelId)
    {
        GameObject model = PhotonView.Find(modelId).gameObject;
        SingleModelStateController singleModelStateController = model.GetComponent<SingleModelStateController>();
        singleModelStateController.SetState(ModelTheme.Choose, ChooseState.Group_son);

        // de chosen
        singleModelStateController.chosenCube.SetActive(false);
    }

    [PunRPC]
    private void SetStateForGroupParent(int modelId)
    {
        GameObject model = PhotonView.Find(modelId).gameObject;
        SingleModelStateController singleModelStateController = model.GetComponent<SingleModelStateController>();
        singleModelStateController.SetState(ModelTheme.Choose, ChooseState.Group_parent);

        // de chosen
        singleModelStateController.chosenCube.SetActive(false);
    }

    [PunRPC]
    private void SetSonObjsToGroupParent(int sonId, int fatherId)
    {
        // find the son object and father object
        GameObject son = PhotonView.Find(sonId).gameObject;
        
        GameObject father = PhotonView.Find(fatherId).gameObject;
        SingleModelStateController fatherController = father.GetComponent<SingleModelStateController>();

        // set the son object as son of groupSonObjRoot
        son.transform.parent = fatherController.groupSonObjRoot.transform;
    }

    /// <summary>
    /// Ungroup functions used to ungroup all son objects from the group parent
    /// </summary>
    /// <returns></returns>

    public void Ungroup()
    {
        // get the group parent
        int fatherControllerId = GetComponent<PhotonView>().ViewID;

        GetComponent<PhotonView>().RPC("RPC_Ungroup", RpcTarget.All, fatherControllerId);
    }

    [PunRPC]
    private void RPC_Ungroup(int fatherObjId)
    {
        // find the father object
        GameObject fatherObj = PhotonView.Find(fatherObjId).gameObject;
        SingleModelStateController fatherController = fatherObj.GetComponent<SingleModelStateController>();
        // set state of the father object to default
        fatherController.SetState(ModelTheme.Choose, ChooseState.Prepare);

        // get all son objects
        List<GameObject> sonObjs = new List<GameObject>();
        foreach (Transform son in fatherController.groupSonObjRoot.transform)
        {
            sonObjs.Add(son.gameObject);
        }

        // set all son objects to default state, detach it from the group parent
        foreach (GameObject sonObj in sonObjs)
        {
            sonObj.transform.parent = null;
            
            SingleModelStateController sonController = sonObj.GetComponent<SingleModelStateController>();
            sonController.SetState(ModelTheme.Choose, ChooseState.Prepare);
        }
    }


    private List<GameObject> GetAllChosenObj()
    {
        // get all model objects
        List<GameObject> modelObjs = new List<GameObject>();
        ModelStorageCaller modelStorageCaller = new ModelStorageCaller();
        modelObjs = modelStorageCaller.GetModelObjList();

        // get all chosen objects
        List<GameObject> chosenObjs = new List<GameObject>();
        foreach (GameObject modelObj in modelObjs)
        {
            SingleModelStateController singleModelStateController = modelObj.GetComponent<SingleModelStateController>();
            if (singleModelStateController.currentChooseState == ChooseState.Chosen)
            {
                chosenObjs.Add(modelObj);
            }
        }
        
        return chosenObjs;
    }

    #endregion

    #region Remove and revocation
    public void Remove()
    {
        // get this model object's id
        int modelId = GetComponent<PhotonView>().ViewID;

        // set to not active
        GetComponent<PhotonView>().RPC("RPC_Remove", RpcTarget.All, modelId);

        // remove from the model storage
        ModelStorageCaller modelStorageCaller = new ModelStorageCaller();
        modelStorageCaller.RemoveModel(modelId);
    }

    [PunRPC]
    private void RPC_Remove(int modelId)
    {
        // find the model object
        GameObject modelObj = PhotonView.Find(modelId).gameObject;
        SingleModelStateController singleModelStateController = modelObj.GetComponent<SingleModelStateController>();
        singleModelStateController.SetState(ModelTheme.Default);
        
        // set to not active
        modelObj.SetActive(false);
    }

    #endregion


}
