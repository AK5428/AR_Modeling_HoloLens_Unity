using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class PaintModelSwitcherBasic : PhotonMonoBehaviour
{
    [Header("Manager GameObjects")]
    public GameObject paintManager;
    public GameObject modelManager;

    [Tooltip("The Modeling's current state")]
    [SerializeField]
    private Mode state = Mode.Paint;

    public Mode State
    {
        get { return state; }
        set { state = value; }
    }

    public enum Mode
    {
        Paint,
        Model
    }

    // Start is called before the first frame update
    void Start()
    {
        paintManager.SetActive(true);
        modelManager.SetActive(false);
    }

    public void SwitchMode()
    {
        if (state == Mode.Paint)
        {
            // pass the data
            DataPassOnSwitch();

            // set the switch
            paintManager.SetActive(false);
            modelManager.SetActive(true);
            state = Mode.Model;

            // start the modeling
            modelManager.GetComponent<RhinoModelBasic>().TryStartModeling();
        }
        else
        {
            paintManager.SetActive(true);
            modelManager.SetActive(false);
            state = Mode.Paint;
        }
    }

    public virtual void DataPassOnSwitch()
    {
        // destroy the line and points after the data is pass
        FindPhotonView(paintManager).RPC("DestoryLineAndPoints", RpcTarget.All);
    }
    
}
