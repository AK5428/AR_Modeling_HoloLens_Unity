using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class HandMenuBarButtonBasic : MonoBehaviour
{
    public HandMenuBarBasic.ButtonTypeEnum ButtonType { get { return buttonType; } }

    // [Header("Button Content")]
    // [SerializeField]
    // private MeshRenderer iconMeshRenderer = null;
    // [SerializeField]
    // private Texture icon;

    [SerializeField]
    private HandMenuBarBasic.ButtonTypeEnum buttonType = HandMenuBarBasic.ButtonTypeEnum.Button;

    private HandMenuBarBasic parentToolBar;

    [HideInInspector]
    public Interactable interactable = null;


    public virtual void InitializeButtonContent(HandMenuBarBasic parentToolBar)
    {
        this.parentToolBar = parentToolBar;
        interactable = GetComponent<Interactable>();

        switch (ButtonType)
        {
            case HandMenuBarBasic.ButtonTypeEnum.Button:
                break;
            default:
                // set the toggle state
                interactable.IsToggled = false;
                break;
        }
    }

    public virtual void SetToggleState(bool state)
    {
        if (ButtonType == HandMenuBarBasic.ButtonTypeEnum.Toggle)
        {
            interactable.IsToggled = state;

            // Debug.Log("Button toggled: " + name + " to " + state);
        }
    }
}
