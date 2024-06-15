using System;
using UnityEngine;

public class SplineTester : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform[] points;

    [ContextMenu("Test")]
    public void Test()
    {
        CatmullRomSpline spline = new CatmullRomSpline();

        lineRenderer.positionCount = 201;
        foreach (Transform point in points)
        {
            spline.AddControlPoint(point.position, Vector3.zero, 0f);
        }

        int index = 0;
        while (index <= 100)
        {
            float t = index / 100f + 1;
            Debug.Log(t);
            Vector3 pos =  spline.GetPoint(t);
            lineRenderer.SetPosition(index, pos);
            index++;
        }
        
        
        index = 0;
        while (index <= 100)
        {
            float t = index / 100f + 2;
            Debug.Log(t);
            Vector3 pos =  spline.GetPoint(t);
            lineRenderer.SetPosition(index + 100, pos);
            index++;
        }
    }
}