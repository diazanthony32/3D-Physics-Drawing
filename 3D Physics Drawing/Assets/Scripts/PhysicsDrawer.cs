using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsDrawer : MonoBehaviour
{
	public GameObject customLinePrefab;

	//public LayerMask cantDrawOverLayer;
	//int cantDrawOverLayerIndex;

	[Space(10f)]
	//public Gradient lineColor;
	//public float linePointsMinDistance;
	//public float lineWidth;

	CustomRopeMesh currentLine;

	public Camera cam;


	void Start()
	{
		//cam = Camera.main;
		//cantDrawOverLayerIndex = LayerMask.NameToLayer("CantDrawOver");
	}

	void Update()
	{
		if (Input.GetButtonDown("Interact"))
		{
			Debug.Log("Starting New Line!");
			BeginDraw();
		}

		if (currentLine != null)
		{
			Draw();
		}

		if (Input.GetButtonUp("Interact"))
		{
			Debug.Log("Line Done");
			EndDraw();
		}
	}

	// Begin Draw ----------------------------------------------
	void BeginDraw()
	{
		currentLine = Instantiate(customLinePrefab, this.transform).GetComponent<CustomRopeMesh>();

		//Set line properties
		//currentLine.UsePhysics(false);
		//currentLine.SetLineColor(lineColor);
		//currentLine.SetPointsMinDistance(linePointsMinDistance);
		//currentLine.SetLineWidth(lineWidth);

	}
	// Draw ----------------------------------------------------
	void Draw()
	{
		Vector3 mousePosition = cam.transform.position + (cam.transform.TransformDirection(Vector3.forward) * 2.0f);

		//Check if mousePos hits any collider with layer "CantDrawOver", if true cut the line by calling EndDraw( )
		//RaycastHit hit;
		//if (Physics.SphereCast(mousePosition, lineWidth / 3f, transform.forward, out hit, 2.0f))
		//EndDraw();
		//else
		if (currentLine.pointsCount < 151)
		{
			currentLine.AddPoint(mousePosition);
		}
		else
		{
			Debug.Log("Max Line Length Reached");
		}
    }
	// End Draw ------------------------------------------------
	void EndDraw()
	{
		if (currentLine != null)
		{
            if (currentLine.pointsCount < 2)
            {
                //If line has one point
                Destroy(currentLine.gameObject);
            }
            else
            {
                //currentLine.GenerateMesh();
                //currentLine.UpdateMesh();
                //Add the line to "CantDrawOver" layer
                //currentLine.gameObject.layer = cantDrawOverLayerIndex;

                //Activate Physics on the line
                currentLine.UsePhysics(true);

                currentLine = null;
            }
        }
	}

	void OnDrawGizmos()
	{
		// Draw a yellow sphere in front of the object
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(cam.transform.position + (cam.transform.TransformDirection(Vector3.forward) * 2.0f), 0.25f);

		// Draws a 1.75 unit long red line in front of the object
		//Vector3 direction = transform.TransformDirection(Vector3.forward) * 1.75f;
		//Gizmos.DrawRay(transform.position, direction);
	}
}
