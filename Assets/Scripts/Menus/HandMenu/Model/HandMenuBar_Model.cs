using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuBar_Model : HandMenuBarBasic
{
    public override void OnButtonPressed(HandMenuBarButtonBasic button)
    {
        base.OnButtonPressed(button);

        bool state = button.interactable.IsToggled;
        if(!state) return;

        HandMenuBarButton_Model.ModelButtonEnum modelButtonEnum = (button as HandMenuBarButton_Model).modelButtonEnum;

        switch(modelButtonEnum)
        {
            case HandMenuBarButton_Model.ModelButtonEnum.Extrusion:
                new MainControlCaller().SetMode(MainController.ModeEnum.Model, MainController.ModelStateEnum.Extrusion);
                break;
            case HandMenuBarButton_Model.ModelButtonEnum.Revolve:
                new MainControlCaller().SetMode(MainController.ModeEnum.Model, MainController.ModelStateEnum.Revolve);
                break;
            case HandMenuBarButton_Model.ModelButtonEnum.Sweep:
                new MainControlCaller().SetMode(MainController.ModeEnum.Model, MainController.ModelStateEnum.Sweep);
                break;
            // case HandMenuBarButton_Model.ModelButtonEnum.Adjust:
            //     new MainControlCaller().SetMode(MainController.ModeEnum.Model, MainController.ModelStateEnum.Adjust);
            //     break;
        }
    }

    public override void MenuUnToggled()
    {
        base.MenuUnToggled();

        new MainControlCaller().SetMode(MainController.ModeEnum.Default);
    }
}
