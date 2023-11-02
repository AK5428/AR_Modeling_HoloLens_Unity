using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTest : MonoBehaviour
{
    // public DialogController dialogController;
    // Start is called before the first frame update
    DialogCaller dialogCaller = new DialogCaller();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if the space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // open the dialog
            // dialogController.OpenConfirmationDialog("This is a test");
            // DialogCaller dialogCaller = new DialogCaller();
            dialogCaller.CallConfirmationDialog("This is a test");
        }
    }
}
