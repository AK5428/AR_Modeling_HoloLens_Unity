using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// On the line renderer objects.
/// Used to draw this single line attached to this object.
/// </summary>
public class TutorialLineRenderer : MonoBehaviour
{
    
    [HideInInspector]
    public bool enablePainting = true;
    
    // to determinant how near the draw points can be
    private float pointsNearDistance = 0.01f;
    private float lineWidth = 0.01f;
    private Color lineColor;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void GetPaintParameters()
    {
        GameObject[] controllers = GameObject.FindGameObjectsWithTag("PaintManager");
        // GameObject controller;
        foreach (GameObject gameObject in controllers)
        {
            if(gameObject.activeInHierarchy) 
            {
                GameObject controller = gameObject;
                if(controller.TryGetComponent(out TutorialPaint paintManager))
                {
                    lineWidth = paintManager.lineWidth;
                    lineColor = paintManager.lineColor;
                    break;
                }
            }
        }

        // GameObject controller = GameObject.FindWithTag("PaintManager");

        
    }

    #region Pun functions

    // Used to initial the line renderer.
    public void InitialLine()
    {
        if(!enablePainting) return;

        GetPaintParameters();
    
        // update the line parameters
        LineRenderer newLineRenderer = GetComponent<LineRenderer>();
        newLineRenderer.startWidth = lineWidth;
        newLineRenderer.endWidth = lineWidth;
        
        // update the color
        GetComponent<Renderer>().material.color = lineColor;
    }
    
    // Used to draw the line

    public void UpdateLine(Vector3 handPosition, int count)
    {
        // When use relative position to draw:
        // GameObject table = GameObject.FindGameObjectWithTag("table");
        // Vector3 handPosition = table.transform.position - relativePosition;
        if(!enablePainting) return;
       
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer.positionCount <= count)
        {
            lineRenderer.positionCount += 1;
        } 
        lineRenderer.SetPosition(count, handPosition);

    }
    #endregion
    
    
}
