using System;
using System.Linq;
using MathPlus;
using UnityEngine;

public class SplineTester : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform[] points;

    [ContextMenu("Test")]
    public void Test()
    {
        HermitSpline spline = new HermitSpline();

        for(int i = 0; i < points.Length; i++)
        {
            spline.AddVertex(points[i].position, points[i].up);
        }

        lineRenderer.positionCount = points.Length * 100;

        for (int i = 0; i < points.Length - 1; i++)
        {
            for(int j = 1; j <= 100; j++)
            {
                var pos = spline.Interpolation(100 / j);
                lineRenderer.SetPosition((j - 1) * (i + 1), pos);
            }
            spline.DoNext();
        }
    }
}