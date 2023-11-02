// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Assign Dialog.prefab")]
    private GameObject dialogPrefab;

    /// <summary>
    /// Small Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefab
    {
        get => dialogPrefab;
        set => dialogPrefab = value;
    }

    private List<GameObject> dialogList = new List<GameObject>();

    private void ClearDialogList()
    {
        foreach (GameObject dialog in dialogList)
        {
            Destroy(dialog);
        }
        dialogList.Clear();
    }


    /// <summary>
    /// Opens confirmation dialog example
    /// </summary>
    public void OpenConfirmationDialog(string text)
    {
        ClearDialogList();
        Dialog newDialog = Dialog.Open(DialogPrefab, DialogButtonType.OK, "Confirmation Dialog", text, true);
        if (newDialog != null)
        {
            // newDialog.OnClosed += OnClosedDialogEvent;
            dialogList.Add(newDialog.gameObject);
        }
    }

    /// <summary>
    /// Opens choice dialog example
    /// </summary>
    public void OpenChoiceDialog(string text)
    {
        Dialog myDialog = Dialog.Open(DialogPrefab, DialogButtonType.Yes | DialogButtonType.No, "Choice Dialog", text, true);
        if (myDialog != null)
        {
            myDialog.OnClosed += OnClosedDialogEvent;
            dialogList.Add(myDialog.gameObject);
        }
    }

    private void OnClosedDialogEvent(DialogResult obj)
    {
        if (obj.Result == DialogButtonType.Yes)
        {
            Debug.Log(obj.Result);
        }
    }
}
