using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintModelSwitcherRevolve : PaintModelSwitcherBasic
{
    public override void DataPassOnSwitch()
    {
        var vertices = new List<Vector3>();
        foreach (var vertex in paintManager.GetComponent<PaintManagerRevolve>().currentLineRendererInfo.keyPointObjectList)
        {
            vertices.Add(vertex.transform.position);
        }

        modelManager.GetComponent<RhinoModelRevolve>().faceVertices = vertices;
        
        base.DataPassOnSwitch();
    }
}
