using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

public class BoundingManager: PhotonMonoBehaviour
{
    #region In inspector
    [Header("Prefabs and GameObjects")]
    [SerializeField, FormerlySerializedAs("Bounding Prefab")]
    public GameObject boundingPrefab;

    [Header("Bounding Settings")]
    [SerializeField, FormerlySerializedAs("Bounding Scale")]
    public float boundingScale = 1.1f;
    #endregion

    #region Public variables hide in inspector
    [HideInInspector]
    public PhotonView photonView;

    #endregion

    public void Start()
    {
        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            if (boundingPrefab != null && !pool.ResourceCache.ContainsKey(boundingPrefab.name))
            {
                pool.ResourceCache.Add(boundingPrefab.name, boundingPrefab);
            }
        }
        photonView = GetComponent<PhotonView>();
    }

    public void CreateBounding(int modelId)
    {
        Vector3 boundingPos = FindPunObject(modelId).transform.position;
        GameObject boundingObj = PhotonNetwork.Instantiate(boundingPrefab.name, boundingPos, Quaternion.identity);
        int boundingObjId = boundingObj.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_CreateBounding", RpcTarget.All, modelId, boundingObjId);
    }

    [PunRPC]
    public void RPC_CreateBounding(int modelId, int boundingObjId)
    {
        GameObject modelObj = FindPunObject(modelId);
        GameObject boundingObj = FindPunObject(boundingObjId);
        boundingObj.name = "BoundingModel";
        
        if (modelObj == null)
        {
            Debug.LogError("Model not found");
            return;
        }

        // first, set collider
        SetCollider(modelObj);

        // then, add border
        AddBorder(modelObj, boundingObj);

        // save the boundingObj using the key in photon room
        ModelStorageCaller modelStorageCaller = new ModelStorageCaller();
        modelStorageCaller.AddModel(boundingObjId);
        

    }

    private void SetCollider(GameObject modelObj)
    {

        // initial the bounds
        Bounds combinedBounds = new Bounds();

        bool hasInitialBounds = false;

        foreach (MeshFilter meshFilter in modelObj.GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            if (!hasInitialBounds)
            {
                combinedBounds = meshFilter.sharedMesh.bounds;
                hasInitialBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(meshFilter.sharedMesh.bounds);
            }
        }

        // create bounding
        BoxCollider boxCollider = modelObj.AddComponent<BoxCollider>();
        boxCollider.center = modelObj.transform.InverseTransformPoint(combinedBounds.center);
        boxCollider.size = combinedBounds.size;
    }

    private void AddBorder(GameObject modelObj, GameObject singleModelObj)
    {
        // Initial and get the SingleModelStateController from the prefab
        SingleModelStateController singleModelStateController = singleModelObj.GetComponent<SingleModelStateController>();
        singleModelStateController.thisRhinoModel = modelObj;

        GameObject boundingObj = singleModelStateController.boundingMenu;


        // Get the size of the border's collider and the cube's collider
        BoxCollider borderCollider = boundingObj.GetComponent<BoxCollider>();
        BoxCollider cubeCollider = modelObj.GetComponent<BoxCollider>();
        Vector3 sizeBorderCollider = borderCollider.size;
        Vector3 sizeCubeCollider = cubeCollider.size;
        // Scale the border to fit the cube's collider
        float modelSize = Mathf.Max(
            sizeCubeCollider.x / sizeBorderCollider.x,
            sizeCubeCollider.y / sizeBorderCollider.y,
            sizeCubeCollider.z / sizeBorderCollider.z
        );

        float boundingSize = modelSize * boundingScale;
        boundingObj.transform.localScale = boundingSize / sizeBorderCollider.x * Vector3.one;

        // Find the center of the border's collider and the cube's collider
        Vector3 centerBorderCollider = borderCollider.center;
        Vector3 centerCubeCollider = cubeCollider.center;
        // Move the cube to overlap the center of the border
        Vector3 offset = centerCubeCollider - centerBorderCollider;
        
        boundingObj.transform.position += offset;

        // Make the cube a child of the border
        modelObj.transform.SetParent(boundingObj.transform);

        // disable the collider of the cube
        cubeCollider.enabled = false;

        // if the model is not mine, then update the postion of the bounding with the adjust feature
        if (!boundingObj.GetComponent<PhotonView>().IsMine)
        {
            Debug.Log("Update the position of the bounding");
            boundingObj.transform.parent.position += new MainControlCaller().GetAdjustFeature();
        }
    }
}
