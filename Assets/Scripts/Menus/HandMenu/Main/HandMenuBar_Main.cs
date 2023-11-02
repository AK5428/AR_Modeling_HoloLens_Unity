using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Photon.Pun;

public class HandMenuBar_Main : HandMenuBarBasic
{
    public override void OnButtonPressed(HandMenuBarButtonBasic button)
    {
        base.OnButtonPressed(button);
        HandMenuBarButton_Main.MainButtonEnum buttonEnum = (button as HandMenuBarButton_Main).mainButtonEnum;

        if(buttonEnum == HandMenuBarButton_Main.MainButtonEnum.Revocation)
        {
            Revocation();
            return;
        }

        bool state = button.interactable.IsToggled;
        if(!state) return;

        MainControlCaller mainControlCaller =  new MainControlCaller();
        // switch for four state: Model, Choose, Animate, and Revocation
        switch(buttonEnum)
        {
            
            case HandMenuBarButton_Main.MainButtonEnum.Model:
                // switch to model state
                break;
            case HandMenuBarButton_Main.MainButtonEnum.Adjust:
                mainControlCaller.SetMode(MainController.ModeEnum.Adjust);
                // switch to model state
                break;
            case HandMenuBarButton_Main.MainButtonEnum.Choose:
                mainControlCaller.SetMode(MainController.ModeEnum.Choose);
                // switch to choose state
                break;
            case HandMenuBarButton_Main.MainButtonEnum.Revocation:
                // switch to revocation state
                // Revocation();
                break;
            case HandMenuBarButton_Main.MainButtonEnum.Clear:

                // mainControlCaller.ClearModel();
                // clear the model
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    private void Revocation()
    {
        int modelId = new ModelStorageCaller().RevocationModel();
        if (modelId == -1) return;

        // active the model
        GetComponent<PhotonView>().RPC("RPC_Revocation", RpcTarget.All, modelId);
    }

    [PunRPC]
    private void RPC_Revocation(int modelId)
    {
        GameObject modelObj = FindPunObject(modelId);
        if (modelObj == null)
        {
            Debug.LogError("Model not found");
            return;
        }

        // active the model
        modelObj.SetActive(true);

        // update the state
        StartCoroutine(SetModeAsCurrent());
    }

    IEnumerator SetModeAsCurrent(float time = 0.3f)
    {
        yield return new WaitForSeconds(time);
        MainControlCaller mainControlCaller = new MainControlCaller();
        mainControlCaller.SetMode();
    }
}
