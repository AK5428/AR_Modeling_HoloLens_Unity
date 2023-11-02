using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonMonoBehaviour : MonoBehaviour
{
    public PhotonView FindPhotonView(GameObject punObject)
    {
        return punObject.GetComponent<PhotonView>();
    }

    public GameObject FindPunObject(int viewID)
    {
        return PhotonView.Find(viewID).gameObject;
    }

    [PunRPC]
    public void DestoryPunModel(int viewId)
   {
       PhotonNetwork.Destroy(FindPunObject(viewId).gameObject);
   }
}
