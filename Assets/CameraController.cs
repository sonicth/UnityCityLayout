// copied from https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
// copyright of the author

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

	private static readonly float PanSpeed = 100f;
	private static readonly float ZoomSpeedTouch = 0.1f;
	private static readonly float ZoomSpeedMouse = 50.0f;

	private static readonly float[] BoundsX = new float[] { -50f, 50f };
	private static readonly float[] BoundsZ = new float[] { -50f, 50f };
	private static readonly float[] ZoomBounds = new float[] { 10f, 285f };

	private Camera cam;

	private Vector3 lastPanPosition;
	private int panFingerId; // Touch mode only

	private bool wasZoomingLastFrame; // Touch mode only
	private Vector2[] lastZoomPositions; // Touch mode only

	void Awake()
	{
		cam = GetComponent<Camera>();
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
		if (Input.GetMouseButtonDown(0))
		{
			lastPanPosition = Input.mousePosition;
		}
		else if (Input.GetMouseButton(0))
		{
			PanCamera(Input.mousePosition);
		}

		// Check for scrolling to zoom the camera
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		ZoomCamera(scroll, ZoomSpeedMouse);
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
		pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
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

		cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
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