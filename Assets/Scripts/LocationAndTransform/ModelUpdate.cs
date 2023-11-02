using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ModelUpdate : PhotonMonoBehaviour
{
    private Vector3 lastPosition;
    
    private PhotonView photonView;
    private int ViewID;

    private void Start()
    {
        lastPosition = transform.position;

        photonView = GetComponent<PhotonView>();
        ViewID = photonView.ViewID;
    }

    private void FixedUpdate()
    {
        if(!photonView.IsMine) return;
        if (transform.position != lastPosition)
        {
            try
            {
                Vector3 positionOffset = transform.position - lastPosition;
                photonView.RPC("UpdatePosition", RpcTarget.All, positionOffset, ViewID);
            }
            catch (MissingReferenceException e)
            {
                Debug.Log("MissingReferenceException: " + e.Message);
            }
            
        }
        
    }

    [PunRPC]
    private void UpdatePosition(Vector3 positionOffset, int ViewID)
    {
        // find the object with the same ViewID
        GameObject boundingObj = FindPunObject(ViewID);
        boundingObj.transform.position += positionOffset;
    }

}
