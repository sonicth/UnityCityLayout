// copied from https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
// copyright of the author

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

	private static readonly float PanSpeed = 100f;
	private static readonly float ZoomSpeedTouch = 2.0f;
	private static readonly float ZoomSpeedMouse = 50.0f;

	public static readonly float bound_x = 50f;
	private static readonly float[] BoundsZ = new float[] { -50f, 50f };
	private static readonly float[] ZoomBounds = new float[] { 1f, 50f };

	private Camera cam;

	private Vector3 lastPanPosition;
	private int panFingerId; // Touch mode only

	private bool wasZoomingLastFrame; // Touch mode only
	private Vector2[] lastZoomPositions; // Touch mode only


	readonly int PAN_BUTTON = 1;
	readonly int SELECT_BUTTON = 0;

	// picking
	private GameObject picked = null;
	private GameObject selected = null;

	void Awake()
	{
		var cams = GetComponentsInChildren<Camera>();
		cam = cams[0];
	}

	void Update()
	{
		if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
		{
			HandleTouch();
		}
		else
		{
			HandleMouse();
		}
	}

	void HandleTouch()
	{
		switch (Input.touchCount)
		{

			case 1: // Panning
				wasZoomingLastFrame = false;

				// If the touch began, capture its position and its finger ID.
				// Otherwise, if the finger ID of the touch doesn't match, skip it.
				Touch touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began)
				{
					lastPanPosition = touch.position;
					panFingerId = touch.fingerId;
				}
				else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
				{
					PanCamera(touch.position);
				}
				break;

			case 2: // Zooming
				Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
				if (!wasZoomingLastFrame)
				{
					lastZoomPositions = newPositions;
					wasZoomingLastFrame = true;
				}
				else
				{
					// Zoom based on the distance between the new positions compared to the 
					// distance between the previous positions.
					float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
					float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
					float offset = newDistance - oldDistance;

					ZoomCamera(offset, ZoomSpeedTouch);

					lastZoomPositions = newPositions;
				}
				break;

			default:
				wasZoomingLastFrame = false;
				break;
		}
	}


	void HandleMouse()
	{
		
		// On mouse down, capture it's position.
		// Otherwise, if the mouse is still down, pan the camera.
		if (Input.GetMouseButtonDown(PAN_BUTTON))
		{
			lastPanPosition = Input.mousePosition;
		}
		else if (Input.GetMouseButton(PAN_BUTTON))
		{
			PanCamera(Input.mousePosition);
		}
		else if (Input.GetMouseButtonDown(SELECT_BUTTON))
		{
			if (picked)
			{
				//TODO
				// select object
				Debug.LogFormat("TODO select picked object <{0}>", picked.name);

				if (selected)
					CityLayoutMesh.highlightMesh(selected, CityLayoutMesh.PickingState.Normal);

				selected = picked;
				CityLayoutMesh.highlightMesh(selected, CityLayoutMesh.PickingState.Selected);
			}
			else
			{
				if (selected)
				{
					Debug.LogFormat("de-select object <{0}>", selected.name);
					CityLayoutMesh.highlightMesh(selected, CityLayoutMesh.PickingState.Normal);
					selected = null;
					
				}
				else
					Debug.Log("noting selected!");
			}
		}

		// Check for scrolling to zoom the camera
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		ZoomCamera(scroll, ZoomSpeedMouse);

		// pick object all the time!
		PickObjects(Input.mousePosition);
	}

	void PickObjects(Vector3 position)
	{
		// prepare ray to cast based on the screen and mouse input
		Ray ray = Camera.main.ScreenPointToRay(position);

		RaycastHit hit;
		GameObject to_unpick = null;
		if (Physics.Raycast(ray, out hit))
		{
			// collided at hit.point

			// a new different object hit!
			if (picked != hit.collider.gameObject)
			{
				// unpick previoius object; can be null
				to_unpick = picked;

				// set new picked object!
				picked = hit.collider.gameObject;

				// highligh (select) picked object
				if (picked != selected)
					CityLayoutMesh.highlightMesh(picked, CityLayoutMesh.PickingState.Picking);
			}
			//else { Debug.Log("**same object hit!"); }
		}
		else
		{
			// de-highlight object
			if (picked)
			{
				to_unpick = picked;
				picked = null;
			}
			//else { Debug.Log("**nothing hit!"); }
		}

		if (to_unpick)
		{
			if (to_unpick != selected)
				CityLayoutMesh.highlightMesh(to_unpick, CityLayoutMesh.PickingState.Normal);

			to_unpick = null;
		}
	}

	void PanCamera(Vector3 newPanPosition)
	{
		// Determine how much to move the camera
		Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
		Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

		// Perform the movement
		transform.Translate(move, Space.World);

		// Ensure the camera remains within bounds.
		Vector3 pos = transform.position;
		pos.x = Mathf.Clamp(transform.position.x, -bound_x, bound_x);
		pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);

		transform.position = pos;

		// Cache the position
		lastPanPosition = newPanPosition;
	}

	void ZoomCamera(float offset, float speed)
	{
		if (offset == 0)
		{
			return;
		}

		var size_change = speed * (cam.orthographicSize / ZoomBounds[1]);
		cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (offset * size_change), ZoomBounds[0], ZoomBounds[1]);
		//cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
	}
}


//public class CameraNavigation
//{
//	public float speed = 50;
//	private float limit_r = 20;

//	public void Update(Transform tr_in)
//	{
//		// FASTER test if any key is pressed
//		var any_key_pressed = true;
//		if (any_key_pressed)
//		{
//			var inc = speed * Time.deltaTime;
//			var d_move = Vector3.zero;

//			if (Input.GetKey(KeyCode.RightArrow))
//			{
//				d_move.x += inc;
//			}
//			else if (Input.GetKey(KeyCode.LeftArrow))
//			{
//				d_move.x -= inc;
//			}
//			else if (Input.GetKey(KeyCode.UpArrow))
//			{
//				d_move.z += inc;
//			}
//			else if (Input.GetKey(KeyCode.DownArrow))
//			{
//				d_move.z -= inc;
//			}
//			var tr = tr_in.localPosition;

//			tr += d_move;

//			// move only when within a given square
//			tr.x = Mathf.Clamp(tr.x, -limit_r, +limit_r);
//			tr.z = Mathf.Clamp(tr.z, -limit_r, +limit_r);

//			// update transform
//			tr_in.localPosition = tr;
//		}
//	}
//}