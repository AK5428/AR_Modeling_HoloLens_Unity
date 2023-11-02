using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintModelSwitcherExtrusion : PaintModelSwitcherBasic
{
    public override void DataPassOnSwitch()
    {
        var vertices = new List<Vector3>();
        foreach (var vertex in paintManager.GetComponent<PaintManagerExtrusion>().currentLineRendererInfo.keyPointObjectList)
        {
            vertices.Add(vertex.transform.position);
        }

        modelManager.GetComponent<RhinoModelExtrusion>().faceVertices = vertices;
        modelManager.GetComponent<RhinoModelExtrusion>().planeNormal = paintManager.GetComponent<PaintManagerExtrusion>().planeNormal;

        base.DataPassOnSwitch();
    }
}
