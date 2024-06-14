using System.Collections.Generic;
using UnityEngine;

public class ControlVertex {
    public Vector3 position;
    public Vector3 tangent;
    public float tension;
}

public class CatmullRomSpline {
    public List<ControlVertex> controlPoints = new List<ControlVertex>();

    public void AddControlPoint(Vector3 position, Vector3 tangent, float tension) {
        ControlVertex cv = new ControlVertex();
        cv.position = position;
        cv.tangent = tangent.normalized;
        cv.tension = tension;
        controlPoints.Add(cv);
    }

    public Vector3 GetPoint(float t) {
        int numPoints = controlPoints.Count;
        int segmentIndex = Mathf.FloorToInt(t);

        // Ensure segmentIndex is within valid range
        segmentIndex = Mathf.Clamp(segmentIndex, 1, numPoints - 3);

        // Get the 4 control points
        ControlVertex P0 = controlPoints[segmentIndex - 1];
        ControlVertex P1 = controlPoints[segmentIndex];
        ControlVertex P2 = controlPoints[segmentIndex + 1];
        ControlVertex P3 = controlPoints[segmentIndex + 2];

        // Local parameter within the segment
        float localT = t - segmentIndex;

        // Calculate interpolation coefficients
        float t2 = localT * localT;
        float t3 = t2 * localT;

        float a0 = -0.5f * t3 + t2 - 0.5f * localT;
        float a1 =  1.5f * t3 - 2.5f * t2 + 1.0f;
        float a2 = -1.5f * t3 + 2.0f * t2 + 0.5f * localT;
        float a3 =  0.5f * t3 - 0.5f * t2;

        // Calculate the interpolated position
        Vector3 position = a0 * P0.position + a1 * P1.position + a2 * P2.position + a3 * P3.position;
        return position;
    }
}