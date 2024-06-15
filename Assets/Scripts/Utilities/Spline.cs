using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Vector3 position;
    public Vector3 velocity;
}

public class HermitSpline
{
    private List<Vertex> vertices;

    public HermitSpline()
    {
        vertices = new List<Vertex>();
    }

    public void AddVertex(Vector3 position, Vector3 velocity)
    {
        vertices.Add(new Vertex()
        {
            position = position,
            velocity = velocity
        });
    }

    public void DoNext()
    {
        vertices.RemoveAt(0);
    }

    public Vector3 Interpolation(float t)
    {
        Vector3 A = vertices[0].position;
        Vector3 D = vertices[1].position;
        Vector3 U = vertices[0].velocity;
        Vector3 V = vertices[1].velocity;

        float t2 = t * t;
        float t3 = t2 * t;

        float h00 = 2 * t3 - 3 * t2 + 1;
        float h10 = t3 - 2 * t2 + t;
        float h01 = -2 * t3 + 3 * t2;
        float h11 = t3 - t2;

        return h00 * A + h10 * U + h01 * D + h11 * V;
    }
}

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