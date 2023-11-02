using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogCaller
{
    private DialogController FindDialog()
    {
        GameObject dialog = GameObject.FindWithTag("Dialog");
        DialogController dialogController = dialog.GetComponent<DialogController>();

        return dialogController;
    }

    public void CallConfirmationDialog(string text)
    {
        DialogController dialogController = FindDialog();
        dialogController.OpenConfirmationDialog(text);
    }

    public void CallChoiceDialog(string text)
    {
        DialogController dialogController = FindDialog();
        dialogController.OpenChoiceDialog(text);
    }
}
