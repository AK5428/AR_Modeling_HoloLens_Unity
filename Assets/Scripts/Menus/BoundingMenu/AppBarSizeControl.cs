using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppBarSizeControl : MonoBehaviour
{
    private Vector3 initialScale = new Vector3(1, 1, 1);
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        // use to set the scale of this object
        // make sure this object's size stay static no matter how the parent object is changing
        if(transform.parent != null)
        {
            transform.localScale = new Vector3(
            initialScale.x / transform.parent.localScale.x, 
            initialScale.y / transform.parent.localScale.y, 
            initialScale.z / transform.parent.localScale.z
            );
        }

    }
}
