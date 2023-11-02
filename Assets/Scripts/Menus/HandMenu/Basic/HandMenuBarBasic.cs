using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HandMenuBarBasic : PhotonMonoBehaviour
{
    #region Enum Definitions
    public enum ButtonTypeEnum
    {
        Toggle,
        Button
    }

    #endregion

    #region Private Serialized Fields with Public Properties
    [Tooltip("The parent transform for the button collection")]
    [SerializeField]
    private Transform buttonParent = null;

    /// <summary>
    /// The parent transform for the button collection
    /// </summary>
    public Transform ButtonParent
    {
        get => buttonParent;
        set => buttonParent = value;
    }

    #endregion

    private List<HandMenuBarButtonBasic> buttons = new List<HandMenuBarButtonBasic>();
    private bool wasActiveLastFrame = false;


    public void OnEnable()
    {
        // Debug.Log("HandMenuBarBasic enabled");

        buttons.Clear();
        
        foreach (Transform child in ButtonParent)
        {
            HandMenuBarButtonBasic button = child.GetComponent<HandMenuBarButtonBasic>();
            if (button != null)
            {
                button.InitializeButtonContent(this);
            }

            buttons.Add(button);
        }

    }

    public virtual void MenuUnToggled()
    {
        foreach (HandMenuBarButtonBasic button in buttons)
        {
            button.SetToggleState(false);
        }
    }


    public virtual void OnButtonPressed(HandMenuBarButtonBasic button)
    {
        // Debug.Log("Button pressed: " + button.name);
        switch (button.ButtonType)
        {
            case ButtonTypeEnum.Button:
                break;
            case ButtonTypeEnum.Toggle:
                // toggle the button state
                bool state = button.interactable.IsToggled;

                // if state is true, means the toggle is turned on
                // then update the hand menu bar, set the other buttons to false
                if(state) UpdateHandMenuBar(button);
                // if state is false, means the toggle is turned off
                // then set the Main mode to default
                else 
                {
                    MainControlCaller mainControlCaller = new MainControlCaller();
                    mainControlCaller.SetMode(MainController.ModeEnum.Default);
                }
                button.SetToggleState(state);
                break;
        }
    }

    private void UpdateHandMenuBar(HandMenuBarButtonBasic pressedButton)
    {
        foreach (HandMenuBarButtonBasic button in buttons)
        {
            if (button != pressedButton)
            {
                button.SetToggleState(false);
            }
        }
    }
}
