using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuBarButton_Main : HandMenuBarButtonBasic
{
    [Header("Main Menu Bar Button Properties")]
    [SerializeField]
    private GameObject sonMenu;

    public enum MainButtonEnum
    {
        Model,
        Choose,
        Clear,
        Adjust,
        Revocation
    }

    [Header("Model Button")]
    [SerializeField]
    private MainButtonEnum displayButtonEnum = MainButtonEnum.Model;

    public MainButtonEnum mainButtonEnum { get { return displayButtonEnum; } }

    public override void InitializeButtonContent(HandMenuBarBasic parentToolBar)
    {
        base.InitializeButtonContent(parentToolBar);

        if (sonMenu != null)
        {
            sonMenu.GetComponent<SonAutoActive>().SetIsChosenMode(false);
        }
    }

    public override void SetToggleState(bool state)
    {
        base.SetToggleState(state);

        // Debug.Log("Button toggled: " + name + " to " + state);

        if (sonMenu != null)
        {
            if(state == false)
            {
                sonMenu.GetComponent<HandMenuBarBasic>().MenuUnToggled();
            }
            sonMenu.GetComponent<SonAutoActive>().SetIsChosenMode(state);
            // sonMenu.SetActive(state);
        }
    }
}
