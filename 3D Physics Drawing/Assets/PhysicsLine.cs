using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    //public EdgeCollider2D edgeCollider;
    public Rigidbody rigidBody;

    //[HideInInspector] public List<Vector2> points = new List<Vector2>();
    [HideInInspector] public List<Vector3> points = new List<Vector3>();
    [HideInInspector] public int pointsCount = 0;

    //The minimum distance between line's points.
    float pointsMinDistance = 0.1f;

    //Circle collider added to each line's point
    //float circleColliderRadius;
    float sphereColliderRadius;


    public void AddPoint(Vector3 newPoint)
    {
        //If distance between last point and new point is less than pointsMinDistance do nothing (return)
        if (pointsCount >= 1 && Vector3.Distance(newPoint, GetLastPoint()) < pointsMinDistance)
            return;

        points.Add(newPoint);
        pointsCount++;

        //Add Circle Collider to the Point
        //CircleCollider2D circleCollider = this.gameObject.AddComponent<CircleCollider2D>();
        SphereCollider sphereCollider = this.gameObject.AddComponent<SphereCollider>();

        //circleCollider.offset = newPoint;
        //circleCollider.radius = circleColliderRadius;
        //sphereCollider.offset = newPoint;

        sphereCollider.radius = sphereColliderRadius;
        sphereCollider.center = newPoint;

        //Line Renderer
        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPosition(pointsCount - 1, newPoint);

        //Edge Collider
        //Edge colliders accept only 2 points or more (we can't create an edge with one point :D )
        //if (pointsCount > 1)
        //	edgeCollider.points = points.ToArray();
    }

    public Vector3 GetLastPoint()
    {
        return (Vector3)lineRenderer.GetPosition(pointsCount - 1);
    }

    public void UsePhysics(bool usePhysics)
    {
        // isKinematic = true  means that this rigidbody is not affected by Unity's physics engine
        rigidBody.isKinematic = !usePhysics;
    }

    public void SetLineColor(Gradient colorGradient)
    {
        lineRenderer.colorGradient = colorGradient;
    }

    public void SetPointsMinDistance(float distance)
    {
        pointsMinDistance = distance;
    }

    public void SetLineWidth(float width)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        //circleColliderRadius = width / 2f;

        //edgeCollider.edgeRadius = circleColliderRadius;

        sphereColliderRadius = width / 2f;

        //edgeCollider.edgeRadius = sphereColliderRadius;
    }
}
