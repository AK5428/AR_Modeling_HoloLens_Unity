using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelStateController : MonoBehaviour
{
    #region In inspector
    [Header("Model Managers")]
    [SerializeField]
    private GameObject extrusionManagerObj = null;
    [SerializeField]
    private GameObject revolveManagerObj = null;
    [SerializeField]
    private GameObject sweepManagerObj = null;

    private MainController.ModelStateEnum modelState = MainController.ModelStateEnum.Extrusion;
    private List<GameObject> managerObjList = new List<GameObject>();

    #endregion

    void Start()
    {
        managerObjList.Add(extrusionManagerObj);
        managerObjList.Add(revolveManagerObj);
        managerObjList.Add(sweepManagerObj);

        SetAllManagerFalse();
    }

    public void SetAllManagerFalse()
    {
        foreach (GameObject managerObj in managerObjList)
        {
            managerObj.SetActive(false);
        }
    }

    public void SetState(MainController.ModelStateEnum state)
    {
        modelState = state;
        switch (state)
        {
            case MainController.ModelStateEnum.Extrusion:
                SetManager(extrusionManagerObj);
                break;
            case MainController.ModelStateEnum.Revolve:
                SetManager(revolveManagerObj);
                break;
            case MainController.ModelStateEnum.Sweep:
                SetManager(sweepManagerObj);
                break;
            default:
                break;
        }
    }

    public void SetManager(GameObject currentManagerObj)
    {
        foreach (GameObject managerObj in managerObjList)
        {
            if (managerObj == currentManagerObj)
            {
                managerObj.SetActive(true);
            }
            else
            {
                managerObj.SetActive(false);
            }
        }
    }
}
