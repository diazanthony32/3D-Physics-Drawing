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

    [Header("List")]
    public List<Vector3> pointsArray = new List<Vector3>();

    //Vector3[] ropeVerticies;
    //int[] ropeTriangles;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();

    [Space(10)]

    public int ropeResolution;
    public float ropeDiameter;
    //public float ropeLength;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = "UnityPlumber Pipe";

        GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshCollider>().sharedMesh = mesh;

        GenerateMesh();
        UpdateMesh();
    }

    void GenerateMesh()
    {
        // for each segment, generate a cylinder
        for (int i = 0; i < pointsArray.Count - 1; i++)
        {
            Vector3 initialPoint = pointsArray[i];
            Vector3 endPoint = pointsArray[i + 1];
            Vector3 direction = (pointsArray[i + 1] - pointsArray[i]).normalized;

            // generate two circles with "pipeSegments" sides each and then
            // connect them to make the cylinder
            GenerateCircleAroundPoint(vertices, normals, initialPoint, direction);
            GenerateCircleAroundPoint(vertices, normals, endPoint, direction);

            MakeCylinderTriangles(triangles, i);

            GenerateColliders(initialPoint, endPoint);
        }

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
                (ropeDiameter/2 * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (ropeDiameter / 2 * Mathf.Sin(radiansPerSegment * i) * yAxis);
            vertices.Add(currentVertex);
            normals.Add((currentVertex - center).normalized);
        }
    }

    void MakeCylinderTriangles(List<int> triangles, int segmentIdx)
    {
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

    void GenerateColliders(Vector3 initial, Vector3 end)
    {
        GameObject g = new GameObject(); 
        g.transform.parent = this.transform;
        g.transform.position = ((end + initial)/2);

        Quaternion temp = Quaternion.LookRotation(end - initial);
        temp *= Quaternion.Euler(90, 0, 0);
        g.transform.rotation = temp;

        CapsuleCollider collider = g.AddComponent<CapsuleCollider>();
        collider.radius = ropeDiameter / 2;
        //collider.center = (end-initial).normalized;
        collider.height = Vector3.Distance(initial, end) + 0.25f ;

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
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        //GetComponent<MeshCollider>().sharedMesh = mesh;

    }

}
