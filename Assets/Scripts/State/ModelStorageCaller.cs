using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelStorageCaller
{
    private ModelStorage FindModelStorage()
    {
        GameObject modelStorageObj = GameObject.FindWithTag("MainController");
        if (modelStorageObj == null)
        {
            modelStorageObj = new GameObject("ModelStorage");
            modelStorageObj.AddComponent<ModelStorage>();
        }
        return modelStorageObj.GetComponent<ModelStorage>();
    }

 
    public void AddModel(int modelId)
    {
        ModelStorage modelStorage = FindModelStorage();
        modelStorage.AddId(modelStorage.MODEL_IDS ,modelId);
        // Debug.Log("Add model id: " + modelId);
    }


    public void RemoveModel(int modelId)
    {
        ModelStorage modelStorage = FindModelStorage();
        modelStorage.AddId(modelStorage.DELETE_MODEL_IDS, modelId);
        modelStorage.RemoveId(modelStorage.MODEL_IDS, modelId);
        // Debug.Log("Delete model id: " + modelId);
    }

    public int RevocationModel()
    {
        ModelStorage modelStorage = FindModelStorage();
        int[] deleteModelIds = modelStorage.GetIds(modelStorage.DELETE_MODEL_IDS);
        // int[] modelIds = modelStorage.GetIds(modelStorage.MODEL_IDS);
        int modelId = -1;
        if (deleteModelIds.Length > 0)
        {
            modelId = deleteModelIds[deleteModelIds.Length - 1];
            modelStorage.RemoveId(modelStorage.DELETE_MODEL_IDS, modelId);
            modelStorage.AddId(modelStorage.MODEL_IDS, modelId);
        }
        Debug.Log("Revocation model id: " + modelId);
        return modelId;
    }
    

    public List<GameObject> GetModelObjList()
    {
        ModelStorage modelStorage = FindModelStorage();
        List<GameObject> modelObjs = modelStorage.GetObjList(modelStorage.MODEL_IDS);
        return modelObjs;
    }
}
