using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BoundingBoxUpdate : PhotonMonoBehaviour
{
    private Vector3 lastRotation;
    private Vector3 lastScale;
    private PhotonView photonView;
    private int ViewID;

    private void Start()
    {
        lastRotation = transform.rotation.eulerAngles;
        lastScale = transform.localScale;

        photonView = GetComponent<PhotonView>();
        ViewID = photonView.ViewID;
    }

    private void FixedUpdate()
    {
        if(!photonView.IsMine) return;
        if (transform.rotation.eulerAngles != lastRotation)
        {
            lastRotation = transform.rotation.eulerAngles;
            photonView.RPC("UpdateRotation", RpcTarget.All, lastRotation, ViewID);
        }
        else if (transform.localScale != lastScale)
        {
            lastScale = transform.localScale;
            photonView.RPC("UpdateScale", RpcTarget.All, lastScale, ViewID);
        }
    }

    [PunRPC]
    private void UpdateRotation(Vector3 rotation, int ViewID)
    {
        // find the object with the same ViewID
        GameObject boundingObj = FindPunObject(ViewID);
        boundingObj.transform.rotation = Quaternion.Euler(rotation);
    }

    [PunRPC]
    private void UpdateScale(Vector3 scale, int ViewID)
    {
        // find the object with the same ViewID
        GameObject boundingObj = FindPunObject(ViewID);
        boundingObj.transform.localScale = scale;
    }
}
