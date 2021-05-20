using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CustomRopeMesh : MonoBehaviour
{
    /*
     * 
     Issues encountered:
        * Cant use Rigidbody on a mesh collider
        * convex collider removes accuracy
        * cant rotate capsule collider to fit geometery unless its on a empty gameobject
        * 
     
     */
    Mesh mesh;

    //[Header("List")]
    public List<Vector3> pointsArray = new List<Vector3>();
    public int pointsCount = 0;

    //Vector3[] ropeVerticies;
    //int[] ropeTriangles;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();

    [Space(10)]
    [Range(3, 32)]
    public int ropeResolution;
    public float ropeRadius;
    public float elbowRadius = 0.5f;
    //public float ropeLength;

    [Space(10)]
    [Range(3, 32)]
    public int elbowSegments = 6;

    //The minimum distance between line's points.
    public float pointsMinDistance = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = "UnityPlumber Pipe";

        GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshCollider>().sharedMesh = mesh;

        //GenerateMesh();
        //UpdateMesh();
    }

    public void AddPoint(Vector3 newPoint)
    {
        //If distance between last point and new point is less than pointsMinDistance do nothing (return)
        if (pointsCount >= 1 && Vector3.Distance(newPoint, GetLastPoint()) < pointsMinDistance)
            return;

        pointsArray.Add(newPoint);
        pointsCount++;

        if (pointsCount > 1) 
        {
            GenerateMesh();
            UpdateMesh();
        }
    }

    public Vector3 GetLastPoint()
    {
        return (Vector3)pointsArray[pointsArray.Count - 1];
    }
    public void UsePhysics(bool usePhysics)
    {
        // isKinematic = true  means that this rigidbody is not affected by Unity's physics engine
        this.GetComponent<Rigidbody>().isKinematic = !usePhysics;
    }

    // OPTIMIZE LATER: FIGURE OUT A WAY TO PREVENT THE DELETION AND RECREATION OF THE MESH FOR EACH POINT ADDED
    public void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        // for each segment, generate a cylinder
        for (int i = 0; i < pointsArray.Count - 1; i++)
        {
            Vector3 initialPoint = pointsArray[i];
            Vector3 endPoint = pointsArray[i + 1];
            Vector3 direction = (pointsArray[i + 1] - pointsArray[i]).normalized;

            //if (i > 0) //&& generateElbows)
            //{
            //    // leave space for the elbow that will connect to the previous
            //    // segment, except on the very first segment
            //    initialPoint = initialPoint + direction * elbowRadius;
            //}

            //if (i < pointsArray.Count - 2) //&& generateElbows)
            //{
            //    // leave space for the elbow that will connect to the next
            //    // segment, except on the last segment
            //    endPoint = endPoint - direction * elbowRadius;
            //}

            // generate two circles with "pipeSegments" sides each and then
            // connect them to make the cylinder
            GenerateCircleAroundPoint(vertices, normals, initialPoint, direction);
            GenerateCircleAroundPoint(vertices, normals, endPoint, direction);

            MakeCylinderTriangles(triangles, i);

            GenerateColliders(initialPoint, endPoint);
        }

        // for each segment generate the elbow that connects it to the next one
        //if (generateElbows)
        //{
        //for (int i = 0; i < pointsArray.Count - 2; i++)
        //{
        //    Vector3 point1 = pointsArray[i]; // starting point
        //    Vector3 point2 = pointsArray[i + 1]; // the point around which the elbow will be built
        //    Vector3 point3 = pointsArray[i + 2]; // next point
        //    GenerateElbow(i, vertices, normals, triangles, point1, point2, point3);
        //}
        //}

        GenerateEndCaps(vertices, triangles, normals);

    }

    void GenerateCircleAroundPoint(List<Vector3> vertices, List<Vector3> normals, Vector3 center, Vector3 direction) {
        // 'direction' is the normal to the plane that contains the circle

        // define a couple of utility variables to build circles
        float twoPi = Mathf.PI * 2;
        float radiansPerSegment = twoPi / ropeResolution;

        // generate two axes that define the plane with normal 'direction'
        // we use a plane to determine which direction we are moving in order
        // to ensure we are always using a left-hand coordinate system
        // otherwise, the triangles will be built in the wrong order and
        // all normals will end up inverted!
        Plane p = new Plane(Vector3.forward, Vector3.zero);
        Vector3 xAxis = Vector3.up;
        Vector3 yAxis = Vector3.right;
        if (p.GetSide(direction))
        {
            yAxis = Vector3.left;
        }

        // build left-hand coordinate system, with orthogonal and normalized axes
        Vector3.OrthoNormalize(ref direction, ref xAxis, ref yAxis);

        for (int i = 0; i < ropeResolution; i++)
        {
            Vector3 currentVertex =
                center +
                (ropeRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (ropeRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);
            vertices.Add(currentVertex);
            normals.Add((currentVertex - center).normalized);
        }
    }

    void MakeCylinderTriangles(List<int> triangles, int segmentIdx)
    {
        //Debug.Log(segmentIdx);
        
        // connect the two circles corresponding to segment segmentIdx of the pipe
        int offset = segmentIdx * ropeResolution * 2;
        for (int i = 0; i < ropeResolution; i++)
        {
            triangles.Add(offset + (i + 1) % ropeResolution);
            triangles.Add(offset + i + ropeResolution);
            triangles.Add(offset + i);

            triangles.Add(offset + (i + 1) % ropeResolution);
            triangles.Add(offset + (i + 1) % ropeResolution + ropeResolution);
            triangles.Add(offset + i + ropeResolution);
        }
    }

    void GenerateElbow(int index, List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 point1, Vector3 point2, Vector3 point3)
    {
        // generates the elbow around the area of point2, connecting the cylinders
        // corresponding to the segments point1-point2 and point2-point3
        Vector3 offset1 = (point2 - point1).normalized * ropeRadius;
        Vector3 offset2 = (point3 - point2).normalized * ropeRadius;
        Vector3 startPoint = point2 - offset1;
        Vector3 endPoint = point2 + offset2;

        // auxiliary vectors to calculate lines parallel to the edge of each
        // cylinder, so the point where they meet can be the center of the elbow
        Vector3 perpendicularToBoth = Vector3.Cross(offset1, offset2);
        Vector3 startDir = Vector3.Cross(perpendicularToBoth, offset1).normalized;
        Vector3 endDir = Vector3.Cross(perpendicularToBoth, offset2).normalized;

        // calculate torus arc center as the place where two lines projecting
        // from the edges of each cylinder intersect
        Vector3 torusCenter1;
        Vector3 torusCenter2;
        ClosestPointsOnTwoLines(out torusCenter1, out torusCenter2, startPoint, startDir, endPoint, endDir);
        Vector3 torusCenter = 0.5f * (torusCenter1 + torusCenter2);

        // calculate actual torus radius based on the calculated center of the 
        // torus and the point where the arc starts
        float actualTorusRadius = (torusCenter - startPoint).magnitude;

        float angle = Vector3.Angle(startPoint - torusCenter, endPoint - torusCenter);
        float radiansPerSegment = (angle * Mathf.Deg2Rad) / elbowSegments;
        Vector3 lastPoint = point2 - startPoint;

        for (int i = 0; i <= elbowSegments; i++)
        {
            // create a coordinate system to build the circular arc
            // for the torus segments center positions
            Vector3 xAxis = (startPoint - torusCenter).normalized;
            Vector3 yAxis = (endPoint - torusCenter).normalized;
            Vector3.OrthoNormalize(ref xAxis, ref yAxis);

            Vector3 circleCenter = torusCenter +
                (actualTorusRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (actualTorusRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);

            Vector3 direction = circleCenter - lastPoint;
            lastPoint = circleCenter;

            if (i == elbowSegments)
            {
                // last segment should always have the same orientation
                // as the next segment of the pipe
                direction = endPoint - point2;
            }
            else if (i == 0)
            {
                // first segment should always have the same orientation
                // as the how the previous segmented ended
                direction = point2 - startPoint;
            }

            GenerateCircleAroundPoint(vertices, normals, circleCenter, direction);

            if (i > 0)
            {
                MakeElbowTriangles(vertices, triangles, i, index);
            }
        }
    }

    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }

    void MakeElbowTriangles(List<Vector3> vertices, List<int> triangles, int segmentIdx, int elbowIdx)
    {
        // connect the two circles corresponding to segment segmentIdx of an
        // elbow with index elbowIdx
        int offset = (pointsArray.Count - 1) * ropeResolution * 2; // all vertices of cylinders
        offset += elbowIdx * (elbowSegments + 1) * ropeResolution; // all vertices of previous elbows
        offset += segmentIdx * ropeResolution; // the current segment of the current elbow

        // algorithm to avoid elbows strangling under dramatic
        // direction changes... we basically map vertices to the
        // one closest in the previous segment
        Dictionary<int, int> mapping = new Dictionary<int, int>();
        ////if (avoidStrangling)
        ////{
        //    List<Vector3> thisRingVertices = new List<Vector3>();
        //    List<Vector3> lastRingVertices = new List<Vector3>();

        //    for (int i = 0; i < ropeResolution; i++)
        //    {
        //        lastRingVertices.Add(vertices[offset + i - ropeResolution]);
        //    }

        //    for (int i = 0; i < ropeResolution; i++)
        //    {
        //        // find the closest one for each vertex of the previous segment
        //        Vector3 minDistVertex = Vector3.zero;
        //        float minDist = Mathf.Infinity;
        //        for (int j = 0; j < ropeResolution; j++)
        //        {
        //            Vector3 currentVertex = vertices[offset + j];
        //            float distance = Vector3.Distance(lastRingVertices[i], currentVertex);
        //            if (distance < minDist)
        //            {
        //                minDist = distance;
        //                minDistVertex = currentVertex;
        //            }
        //        }
        //        thisRingVertices.Add(minDistVertex);
        //        mapping.Add(i, vertices.IndexOf(minDistVertex));
        //    }
        ////}
        //else
        //{
        // keep current vertex order (do nothing)
        for (int i = 0; i < ropeResolution; i++)
        {
            mapping.Add(i, offset + i);
        }
        //}

        // build triangles for the elbow segment
        for (int i = 0; i < ropeResolution; i++)
        {
            triangles.Add(mapping[i]);
            triangles.Add(offset + i - ropeResolution);
            triangles.Add(mapping[(i + 1) % ropeResolution]);

            triangles.Add(offset + i - ropeResolution);
            triangles.Add(offset + (i + 1) % ropeResolution - ropeResolution);
            triangles.Add(mapping[(i + 1) % ropeResolution]);
        }
    }

    void GenerateColliders(Vector3 initial, Vector3 end)
    {
        GameObject g = new GameObject(); 
        g.transform.parent = this.transform;
        g.transform.position = ((end + initial)/2);

        Quaternion temp = Quaternion.LookRotation(end - initial);
        temp *= Quaternion.Euler(90, 0, 0);
        g.transform.rotation = temp;

        CapsuleCollider collider = g.AddComponent<CapsuleCollider>();
        collider.radius = ropeRadius;
        //collider.center = (end-initial).normalized;
        collider.height = Vector3.Distance(initial, end);

    }

    void GenerateEndCaps(List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        // create the circular cap on each end of the pipe
        int firstCircleOffset = 0;
        int secondCircleOffset = (pointsArray.Count - 1) * ropeResolution * 2 - ropeResolution;

        vertices.Add(pointsArray[0]); // center of first segment cap
        int firstCircleCenter = vertices.Count - 1;
        normals.Add(pointsArray[0] - pointsArray[1]);

        vertices.Add(pointsArray[pointsArray.Count - 1]); // center of end segment cap
        int secondCircleCenter = vertices.Count - 1;
        normals.Add(pointsArray[pointsArray.Count - 1] - pointsArray[pointsArray.Count - 2]);

        for (int i = 0; i < ropeResolution; i++)
        {
            triangles.Add(firstCircleCenter);
            triangles.Add(firstCircleOffset + (i + 1) % ropeResolution);
            triangles.Add(firstCircleOffset + i);

            triangles.Add(secondCircleOffset + i);
            triangles.Add(secondCircleOffset + (i + 1) % ropeResolution);
            triangles.Add(secondCircleCenter);
        }
    }

    // Update is called once per frame
    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        //GetComponent<MeshCollider>().sharedMesh = mesh;

    }

}
