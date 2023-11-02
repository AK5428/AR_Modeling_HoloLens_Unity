using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuBarButton_Model : HandMenuBarButtonBasic
{
    public enum ModelButtonEnum
    {
        Extrusion,
        Revolve,
        Sweep
    }

    [Header("Model Button")]
    [SerializeField]
    private ModelButtonEnum displayButtonEnum = ModelButtonEnum.Extrusion;

    public ModelButtonEnum modelButtonEnum { get { return displayButtonEnum; } }
    
}
