using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Photon.Pun;

public class PaintSweepSwitcher : PhotonMonoBehaviour
{
    [Header("Prefabs and GameObjects")]
    [SerializeField, FormerlySerializedAs("Sweep Paint Rail")]
    public GameObject sweepPaintRail;

    [SerializeField, FormerlySerializedAs("Sweep Paint Face")]
    public GameObject sweepPaintFace;

    // private int currentFirstKeyPointID = -1;

    void Start()
    {
        sweepPaintFace.SetActive(true);
        sweepPaintRail.SetActive(false);
    }

    public void SwitchPainter()
    {
        if (sweepPaintRail.activeSelf)
        {
            sweepPaintRail.SetActive(false);
            sweepPaintFace.SetActive(true);

            // sweepPaintFace.GetComponent<PaintManagerSweep_Face>().ReStartPaint();
        }
        else
        {
            sweepPaintRail.SetActive(true);
            sweepPaintFace.SetActive(false);
        }
    }

    public void SwitchPainter(int firstKeyPointID)
    {
        if (sweepPaintRail.activeSelf)
        {
            sweepPaintRail.SetActive(false);
            sweepPaintFace.SetActive(true);
        }
        else
        {
            sweepPaintRail.SetActive(true);
            sweepPaintFace.SetActive(false);

            // initial the rail paint
            // currentFirstKeyPointID = firstKeyPointID;
            sweepPaintRail.GetComponent<PaintManagerSweep_Rail>().PaintInitial_SweepRail(firstKeyPointID);
        }
    }

    public void RestartRailPaint()
    {
        int firstKeyPointID = sweepPaintFace.GetComponent<PaintManagerSweep_Face>().InitalFirstPointForRail();
        sweepPaintRail.GetComponent<PaintManagerSweep_Rail>().PaintInitial_SweepRail(firstKeyPointID);
    }
}
