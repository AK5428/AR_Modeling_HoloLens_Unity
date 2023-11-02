using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LineRenderManagerBasic : PhotonMonoBehaviour
{
    [HideInInspector]
    private float lineWidth = 0.01f;
    [HideInInspector]
    private Color lineColor;

      
    public void GetParameters()
    {
        GameObject[] controllers = GameObject.FindGameObjectsWithTag("PaintManager");
        // GameObject controller;
        foreach (GameObject gameObject in controllers)
        {
            if(gameObject.activeInHierarchy) 
            {
                GameObject paintManager = gameObject;
                PaintManagerBasic paintManagerBasic = paintManager.GetComponent<PaintManagerBasic>();
                lineWidth = paintManagerBasic.lineWidth;
                lineColor = paintManagerBasic.lineColor;
            }
        }
    }

    #region PUN RPCs
    [PunRPC]
    public void RPC_SetLine()
    {
        GetParameters();

        // Update the line renderer
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.002f;

        lineColor = ColorUtility.TryParseHtmlString("#0080D6", out Color color) ? color : Color.blue;

        // Update the line color
        lineRenderer.material.color = lineColor;
    }

    [PunRPC]
    public void RPC_UpdateLine(Vector3 position, int keyPointCount)
    {
        // Update the line renderer
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer.positionCount <= keyPointCount) lineRenderer.positionCount++;
        lineRenderer.SetPosition(keyPointCount, position);
    }

    #endregion
}
