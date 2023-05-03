using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTeleporter : MonoBehaviour
{

    public GameObject positionMarker; // marker for display ground position

    public Transform bodyTransform; // target transferred by teleport

    public LayerMask excludeLayers; // excluding for performance

    public float angle = 45f; // Arc take off angle

    public float strength = 10f; // Increasing this value will increase overall arc length


    int maxVertexcount = 100; // limitation of vertices for performance. 

    private float vertexDelta = 0.08f; // Delta between each Vertex on arc. Decresing this value may cause performance problem.

    private LineRenderer arcRenderer;

    private Vector3 velocity; // Velocity of latest vertex

    private Vector3 groundPos; // detected ground position

    private Vector3 lastNormal; // detected surface normal

    private bool groundDetected = false;

    private List<Vector3> vertexList = new List<Vector3>(); // vertex on arc

    private bool displayActive = false; // don't update path when it's false.

	private Vector2 axisValues = Vector2.zero;


    // Teleport target transform to ground position
    public void Teleport()
    {
        if (groundDetected)
        {
			bodyTransform.position = groundPos;
			float degrees = Mathf.Atan2(axisValues.x, axisValues.y) * Mathf.Rad2Deg;
			Vector3 direction = DirectionPointing();
			//bodyTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
			bodyTransform.RotateAround(bodyTransform.position, Vector3.up, degrees);
		}
        else
        {
            Debug.Log("Ground wasn't detected");
        }
    }

    // Active Teleporter Arc Path
    public void ToggleDisplay(bool active)
    {
        arcRenderer.enabled = active;
        positionMarker.SetActive(active);
        displayActive = active;
	}

	// Set the teleporter rotation from axis values, e.g., from a joystick
	public void SetRotationFromAxisValues(Vector2 newAxisValues)
	{
		axisValues = newAxisValues;
	}





	private void Awake()
    {
        arcRenderer = GetComponent<LineRenderer>();
        arcRenderer.enabled = false;
        positionMarker.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (displayActive)
        {
            UpdatePath();
        }
    }


    private void UpdatePath()
    {
        groundDetected = false;

        vertexList.Clear(); // delete all previouse vertices


        velocity = Quaternion.AngleAxis(-angle, transform.right) * transform.forward * strength;

        RaycastHit hit;


        Vector3 pos = transform.position; // take off position

        vertexList.Add(pos);

        while (!groundDetected && vertexList.Count < maxVertexcount)
        {
            Vector3 newPos = pos + velocity * vertexDelta
                + 0.5f * Physics.gravity * vertexDelta * vertexDelta;

            velocity += Physics.gravity * vertexDelta;

            vertexList.Add(newPos); // add new calculated vertex

            // linecast between last vertex and current vertex
            if (Physics.Linecast(pos, newPos, out hit, ~excludeLayers))
            {
                groundDetected = true;
                groundPos = hit.point;
                lastNormal = hit.normal;
            }
            pos = newPos; // update current vertex as last vertex
        }


        positionMarker.SetActive(groundDetected);

        if (groundDetected)
        {
			positionMarker.transform.position = groundPos;
			float degrees = Mathf.Atan2(axisValues.x, axisValues.y) * Mathf.Rad2Deg;
			Vector3 direction = DirectionPointing();
			positionMarker.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
			positionMarker.transform.RotateAround(positionMarker.transform.position, Vector3.up, degrees);
		}

		// Update Line Renderer

		arcRenderer.positionCount = vertexList.Count;
        arcRenderer.SetPositions(vertexList.ToArray());
    }

	private Vector3 DirectionPointing()
	{
		// Zero out the y of both since they may be at different heights
		return (new Vector3(groundPos.x, 0f, groundPos.z) - new Vector3(transform.position.x, 0f, transform.position.z)).normalized;
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(new Vector3(transform.position.x, 0f, transform.position.z), groundPos);
	}

#endif
}