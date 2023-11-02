using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;

public class ModelStorage : MonoBehaviourPunCallbacks
{
    [HideInInspector]
    public string MODEL_IDS = "ModelIds";
    [HideInInspector]
    public string DELETE_MODEL_IDS = "DeleteModelIds";

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        
        BuildKey(MODEL_IDS);
        BuildKey(DELETE_MODEL_IDS);
    }

    public void BuildKey(string key)
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
        {
            ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable();
            initialProps[key] = new int[0];
            PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
        }
    }

    public void AddId(string key, int id)
    {
        StartCoroutine(AddIdCoroutine(key, id));
    }

    private IEnumerator AddIdCoroutine(string key, int id)
    {   
        AddIdOnce(key, id);
        
        // check if the id is added
        bool isAdded = false;

        while (!isAdded)
        {
            int[] ids = PhotonNetwork.CurrentRoom.CustomProperties[key] as int[];
            // turn the int[] to List<int>
        
            if(ids.Contains(id))
            {
                isAdded = true;
                yield break;
            }
            else
            {
                // add the id to the list
                AddIdOnce(key, id);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void AddIdOnce(string key, int id)
    {
        int[] ids = PhotonNetwork.CurrentRoom.CustomProperties[key] as int[];
        if (ids == null)
        {
            ids = new int[0];
        }

        // add the id to the list
        int[] newIds = ids.Concat(new int[] { id }).ToArray();

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { key, newIds } });
    }

    public void RemoveId(string key, int id)
    {
        StartCoroutine(RemoveIdCoroutine(key, id));
    }

    private IEnumerator RemoveIdCoroutine(string key, int id)
    {
        RemoveIdOnce(key, id);

        // check if the id is removed
        bool isRemoved = false;

        while (!isRemoved)
        {
            int[] ids = PhotonNetwork.CurrentRoom.CustomProperties[key] as int[];
            // turn the int[] to List<int>
            List<int> idList = ids.ToList();

            if (!idList.Contains(id))
            {
                isRemoved = true;
                yield break;
            }
            else
            {
                // remove the id from the list
                RemoveIdOnce(key, id);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void RemoveIdOnce(string key, int id)
    {
        int[] ids = PhotonNetwork.CurrentRoom.CustomProperties[key] as int[];
        if (ids != null)
        {
            // remove the id from the list
            int[] newIds = ids.Where(val => val != id).ToArray();
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { key, newIds } });
        }
    }

    public int[] GetIds(string key)
    {
        int[] ids = PhotonNetwork.CurrentRoom.CustomProperties[key] as int[];
        return ids;
    }

    public List<GameObject> GetObjList(string key)
    {
        List<GameObject> modelObjs = new List<GameObject>();
        int[] ids = PhotonNetwork.CurrentRoom.CustomProperties[key] as int[];
        foreach (int modelId in ids)
        {
            GameObject model = PhotonView.Find(modelId).gameObject;
            if (model != null)
            {
                modelObjs.Add(model);
            }
        }
        return modelObjs;
    }
}
