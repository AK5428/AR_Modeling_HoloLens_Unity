using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IPaint2Model
{
    List<Vector3> faceVertices { get; set; }
    List<Vector3> railVertices { get; set; }
    Vector3 firstPoint { get; set; }
    PaintManagerBasic paintManager { get; set; }

    
    void CreateModel(List<Vector3> faceVertices);
    void CreateModel(List<Vector3> faceVertices, Vector3 planeNormal);
    void CreateModel(List<Vector3> faceVertices, List<Vector3> railVertices);

    void EndModeling();

}
