using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class MainControlCaller
{
    public void SetMode()
    {
        FindMainController().SetMode();
    }

    public void SetMode(MainController.ModeEnum mode)
    {
        FindMainController().SetMode(mode);
    }

    public void SetMode(MainController.ModeEnum mode, MainController.ModelStateEnum modelState)
    {
        FindMainController().SetMode(mode, modelState);
    }
    
    public void SetMode(MainController.ModeEnum mode, MainController.ChooseStateEnum chooseState)
    {
        FindMainController().SetMode(mode, chooseState);
    }

    public void SetMode(MainController.ModeEnum mode, MainController.AnimateStateEnum animateState)
    {
        FindMainController().SetMode(mode, animateState);
    }

    private MainController FindMainController()
    {
        return GameObject.FindWithTag("MainController").GetComponent<MainController>();
    }

    public Vector3 GetAdjustFeature()
    {
        return FindMainController().adjustPosFeature;
    }

    public void ClearModel()
    {
        FindMainController().ClearModel();
    }
}
