using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KeyPointManager : MonoBehaviour
{
    [PunRPC]
    public void RPC_ChangePosition(Vector3 position)
    {
        transform.position = position;
    }

    private void Start()
    {
        GameObject keyPointFather = GameObject.FindWithTag("AllKeyPoints");
        transform.parent = keyPointFather.transform;
    }
}
