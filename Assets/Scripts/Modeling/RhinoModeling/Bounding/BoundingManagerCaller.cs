using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingManagerCaller
{
    private BoundingManager FindBoundingManager()
    {
        GameObject boundingManagerObj = GameObject.FindWithTag("ModelManager");
        if (boundingManagerObj == null)
        {
            Debug.LogError("BoundingManager not found");
        }
        return boundingManagerObj.GetComponent<BoundingManager>();
    }

    public void CreateBounding(int modelId)
    {
        FindBoundingManager().CreateBounding(modelId);
    }
}
