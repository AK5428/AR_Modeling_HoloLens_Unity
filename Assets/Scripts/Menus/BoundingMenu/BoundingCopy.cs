using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Photon.Pun;

public class BoundingCopy : PhotonMonoBehaviour
{
    #region In inspector
    [Header("Prefabs and GameObjects")]
    [SerializeField, FormerlySerializedAs("Empty Pun Prefab")]
    private GameObject emptyPunPrefab;

    #endregion

    #region Private variables
    private SingleModelStateController singleModelStateController;
    private PhotonView photonView;

    #endregion

    private void Start()
    {
        singleModelStateController = GetComponent<SingleModelStateController>();
        photonView = GetComponent<PhotonView>();
    }



    public void Copy()
    {
        // judge is the object is Group_parent
        // if is a single model
        if(singleModelStateController.currentChooseState != SingleModelStateController.ChooseState.Group_parent)
        {
            Copy_Single(singleModelStateController); 
        }
        // if is a group model
        else
        {
            // Debug.Log("Copy group model");
            Copy_Group();
        }
    }

    #region Copy for single model

    private GameObject Copy_Single(SingleModelStateController inputController)
    {
        // create an empty object
        GameObject emptyObj = PhotonNetwork.Instantiate(emptyPunPrefab.name, Vector3.zero, Quaternion.identity);
        // get the original RhinoModel object
        GameObject originalModelObj = null;
        foreach (Transform child in inputController.boundingMenu.transform)
        {
            if (child.tag == "RhinoModel" )
            {
                originalModelObj = child.gameObject;
                break;
            }
        }
        if(originalModelObj == null)
        {
            Debug.LogError("RhinoModel object not found");
            return null;
        }

        // using pun to copy the original model object
        // find the id of the original model object and the empty object
        int originalModelObjId = originalModelObj.GetComponent<PhotonView>().ViewID;
        int emptyObjId = emptyObj.GetComponent<PhotonView>().ViewID;
        // call the RPC function
        photonView.RPC("CopyMesh", RpcTarget.All, emptyObjId, originalModelObjId);

        // Create the bounding for the copy object
        var boundingManagerCaller = new BoundingManagerCaller();
        boundingManagerCaller.CreateBounding(emptyObjId);

        // Update the bounding
        // find the id of the original bounding object
        int originalBoundingObjId = inputController.gameObject.GetComponent<PhotonView>().ViewID;
        // call the RPC function
        photonView.RPC("BoundingObjUpdate", RpcTarget.All, emptyObjId, originalBoundingObjId);

        // find the copy bounding object
        GameObject copyBoundingObj = emptyObj.transform.parent.parent.gameObject;
        return copyBoundingObj;
    }

    [PunRPC]
    private void BoundingObjUpdate(int emptyObjId, int originalBoundingObjId)
    {
        // Adjust the transform of this object to the original one
        // find the original bounding object
        GameObject originalBoundingObj = FindPunObject(originalBoundingObjId);
        GameObject boundingMenuObj = originalBoundingObj.GetComponent<SingleModelStateController>().boundingMenu;
        // finding the copy bounding object
        GameObject boundingMenuObjCopy = FindPunObject(emptyObjId).transform.parent.gameObject;
        // GameObject boundingMenuObjCopy = copyBoundingObj.GetComponent<SingleModelStateController>().boundingMenu;
        // set the transform of the copy bounding object as the original one
        boundingMenuObjCopy.transform.position = boundingMenuObj.transform.position;
        boundingMenuObjCopy.transform.rotation = boundingMenuObj.transform.rotation;
        boundingMenuObjCopy.transform.localScale = boundingMenuObj.transform.localScale;

        // set the mode as adjust after a little while
        StartCoroutine(SetModeAsCurrent());
    }

    [PunRPC]
    private void CopyMesh(int emptyObjId, int originalModelObjId)
    {
        // find the empty object
        GameObject emptyObj = FindPunObject(emptyObjId);
        emptyObj.name = "RhinoModel";
        emptyObj.tag = "RhinoModel";

        // find the original model object
        GameObject originalModelObj = FindPunObject(originalModelObjId);

        // find all the rhino mesh object from the original model object
        foreach (Transform child in originalModelObj.transform)
        {
            // copy the rhino mesh object
            GameObject rhinoMeshObj = Instantiate(child.gameObject);
            // rhinoMeshObj.transform.position = child.transform.position;
            rhinoMeshObj.transform.parent = emptyObj.transform;
            rhinoMeshObj.name = "RhinoMesh";
            rhinoMeshObj.GetComponent<MeshRenderer>().enabled = false;
            rhinoMeshObj.GetComponent<MeshFilter>().mesh = child.GetComponent<MeshFilter>().mesh;
            rhinoMeshObj.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    IEnumerator SetModeAsCurrent(float time = 0.3f)
    {
        yield return new WaitForSeconds(time);
        MainControlCaller mainControlCaller = new MainControlCaller();
        mainControlCaller.SetMode();
    }

    #endregion

    #region Copy for group model

    private void Copy_Group()
    {
        // Copy all the children of the group model at first
        // find all the children models
        List<GameObject> childrenModelList = new List<GameObject>();
        foreach (Transform child in singleModelStateController.groupSonObjRoot.transform)
        {
            if (child.tag == "BoundingModel")
            {
                childrenModelList.Add(child.gameObject);
                // Debug.Log("Find a child model");
            }
        }

        // copy the group model
        GameObject fatherObj = Copy_Single(singleModelStateController);

        // copy all the children models, and find their bounding object
        List<GameObject> childrenObjs = new List<GameObject>();
        foreach (GameObject childModel in childrenModelList)
        {
            SingleModelStateController childModelStateController = childModel.GetComponent<SingleModelStateController>();
            GameObject childObj = Copy_Single(childModelStateController);
            childrenObjs.Add(childObj);
        }


        StartCoroutine(GroupCopyModels(childrenObjs, fatherObj));

    }

    private IEnumerator GroupCopyModels(List<GameObject> childrenObjs, GameObject fatherObj)
    {
        // Group all the models
        yield return new WaitForSeconds(0.3f);

        int fatherId = fatherObj.GetComponent<PhotonView>().ViewID;

        // set the father object to group parent state
        GetComponent<PhotonView>().RPC("SetStateForGroupParent", RpcTarget.All, fatherId);

        // for the son objects
        foreach (GameObject childObj in childrenObjs)
        {
            int childId = childObj.GetComponent<PhotonView>().ViewID;
            if (childId != fatherId)
            {
                // first, set all chosen objects (except the father object) to default state
                GetComponent<PhotonView>().RPC("SetStateForGroupSon", RpcTarget.All, childId);

                // third, set the son objects to the group parent
                GetComponent<PhotonView>().RPC("SetSonObjsToGroupParent", RpcTarget.All, childId, fatherId);
            }
        }

        // set the mode as adjust after a little while
        StartCoroutine(SetModeAsCurrent());
    }

    #endregion

}
