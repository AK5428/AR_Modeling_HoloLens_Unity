using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MenuSizeController : MonoBehaviour
{
    [SerializeField]
    private SingleModelStateController singleModelStateController;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if(singleModelStateController.currentChooseState != SingleModelStateController.ChooseState.Group_son)
        {
            ScaleAppBar(transform.parent);
            return;
        }
        else
        {
            Transform rootParent = GetRootParent();
            Transform targetTrans = rootParent.gameObject.GetComponent<SingleModelStateController>().boundingMenu.transform;

            ScaleAppBar(targetTrans);

            Debug.Log("Scale app bar as group son");
        }
        

    }

    private Transform GetRootParent()
    {
        Transform rootParent = transform.parent;
        while(rootParent.parent != null)
        {
            rootParent = rootParent.parent;
        }
        Debug.Log(rootParent.GetComponent<PhotonView>().ViewID);
        return rootParent;
    }

    private void ScaleAppBar(Transform referTransform)
    {
        if(referTransform != null)
        {
            transform.localScale = new Vector3(
            initialScale.x / referTransform.localScale.x, 
            initialScale.y / referTransform.localScale.y, 
            initialScale.z / referTransform.localScale.z
            );
        }
    }
}
