using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    List<GameObject> tiles;
    const int GRID_SIZE = 8;
	const float scale = 20;
	Nagivaiton navigation;

	// Use this for initialization
	void Start () {
		// new navigation update
		navigation = new Nagivaiton();

		// rotate camera
		var tr = gameObject.transform;
		tr.position = new Vector3(0, 60, 0);
		tr.rotation = Quaternion.Euler(90, 0, 0);

		// add object(s)
		//AddCentreObject();
		//AddGridObjects();
	}
	
	// Update is called once per frame
	void Update () {
		navigation.Update(transform);
	}



	void AddCentreObject()
	{
		tiles = new List<GameObject>(1);

		var tile = createQuad(scale * 0.5f);
		tile.transform.position = Vector3.zero;
		tiles.Add(tile);
	}


	GameObject createQuad(float r)
	{
		var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad.transform.localScale = new Vector3(r, r, 1);
		quad.transform.rotation = Quaternion.Euler(90, 0, 0);
		return quad;
	}

	void AddGridObjects()
	{
		tiles = new List<GameObject>(GRID_SIZE * GRID_SIZE);
		for (int i = 0; i < GRID_SIZE; ++i)
			for (int j = 0; j < GRID_SIZE; ++j)
			{
				var tile = createQuad(scale * 0.5f);

				// translate to a grid position
				var pos = (new Vector3(i, 0, j) - new Vector3(GRID_SIZE, 0, GRID_SIZE) * 0.5f) * scale;
				tile.transform.position = pos;

				tiles.Add(tile);
			}

	}
}

public class Nagivaiton
{
	public float speed = 50;
	private float limit_r = 20;

	public void Update(Transform tr_in)
	{
		// FASTER test if any key is pressed
		var any_key_pressed = true;
		if (any_key_pressed)
		{
			var inc = speed * Time.deltaTime;
			var d_move = Vector3.zero;

			if (Input.GetKey(KeyCode.RightArrow))
			{
				d_move.x += inc;
			}
			else if (Input.GetKey(KeyCode.LeftArrow))
			{
				d_move.x -= inc;
			}
			else if (Input.GetKey(KeyCode.UpArrow))
			{
				d_move.z += inc;
			}
			else if (Input.GetKey(KeyCode.DownArrow))
			{
				d_move.z -= inc;
			}
			var tr = tr_in.localPosition;

			tr += d_move;

			// move only when within a given square
			tr.x = Mathf.Clamp(tr.x, -limit_r, +limit_r);
			tr.z = Mathf.Clamp(tr.z, -limit_r, +limit_r);

			// update transform
			tr_in.localPosition = tr;
		}
	}
}