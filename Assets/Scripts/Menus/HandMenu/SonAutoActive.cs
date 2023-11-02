using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonAutoActive : MonoBehaviour
{
    [HideInInspector]
    public bool handDetected = false;
    [HideInInspector]
    public bool isChosenMode = false;

    [SerializeField]
    private GameObject visualObj;

    private void Start()
    {
        visualObj.SetActive(false);
    }

    private void MenuUpdate()
    {
        if(handDetected && isChosenMode) 
        {
            visualObj.SetActive(true);
        }else
        {
            visualObj.SetActive(false);
        }
    }

    public void SetHandDetected(bool value)
    {
        handDetected = value;
        MenuUpdate();
    }

    public void SetIsChosenMode(bool value)
    {
        isChosenMode = value;
        MenuUpdate();
    }
}
