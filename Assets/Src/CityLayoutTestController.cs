using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CityLayoutTestController : MonoBehaviour {

	int updates = 0;
	private List<GameObject> shapes;

	public readonly float BOX_RADIUS = 50;
	private readonly bool enable_lighting = true;
	private CameraController cam_controller = null;

	private void Awake()
	{
		if (!enable_lighting)
		{
			// set ambient to black
			RenderSettings.ambientLight = Color.black;

			// disable scene lights
			var lights = FindObjectsOfType<Light>();
			foreach (var light in lights) { light.enabled = false; }
		}
	}

	// Use this for initialization
	void Start () {
		cam_controller = FindObjectOfType<CameraController>();
		cam_controller.scene_controller = this;

		// start reading gometry
		StartCoroutine("AddGeoJsonGeometry", this);
	}



	// Update is called once per frame
	void Update () {
		++updates;

		if (updates > 10000)
		{
#if UNITY_EDITOR
			// Application.Quit() does not work in the editor so
			// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}

	void AddSquareTest()
	{		
		// get data
		var r = BOX_RADIUS * 0.5f;
		var vxs = new Vector2[] { new Vector3(0, 0), new Vector3(0, r), new Vector3(r, r), new Vector3(r, 0) };

		// create mesh
		var tile = CityLayoutMesh.createMeshFromPolygonData(
			new Tuple<List<Vector2[]>, string>
				(new List<Vector2[]> { vxs }, 
				"<square>"),
			enable_lighting);

		// add locally
		shapes = new List<GameObject>(1);
		tile.transform.parent = transform;
		shapes.Add(tile);
	}



	IEnumerator AddGeoJsonGeometry()
	{
		// 1) get serialised textual geometry data (e.g. read from file)
		var seralised_geometry = CityLayoutModel.GetSerialisedGeometry();

		// 2) read serialised geometry (e.g. GeoJson) into list/collection of Vector2[]
		var polygon_dataset = GeoJson.GetPolygonDatasetFromSerialised(seralised_geometry, BOX_RADIUS);
		yield return null;

		// 3) create GameObjects with meshes from each polygon
		//TODO single gameobject/mesh here...??
		shapes = new List<GameObject>(polygon_dataset.Count);
		var yield_counter = 0;
		const int UNITS_PER_YIELD = 64;
		foreach (var polygon_data in polygon_dataset)
		{
			GameObject tile;
			tile = CityLayoutMesh.createMeshFromPolygonData(polygon_data, enable_lighting);
			tile.transform.parent = transform;
			shapes.Add(tile);

			++yield_counter;
			if (yield_counter >= UNITS_PER_YIELD)
			{
				yield_counter = 0;
				yield return null;
			}
		}


		yield return UpdateShapeState(cam_controller.PixToWorld);
	}

	public IEnumerator UpdateLines()
	{
		float pix_to_world_scale = cam_controller.PixToWorld;
		foreach(var shape_go in shapes)
		{
			if (shape_go == cam_controller.Selected)
				CityLayoutMesh.updateShapeLines(shape_go, CityLayoutMesh.PickingState.Selected, pix_to_world_scale);
			else if (shape_go == cam_controller.Picked)
				CityLayoutMesh.updateShapeLines(shape_go, CityLayoutMesh.PickingState.Picking, pix_to_world_scale);
			else
				CityLayoutMesh.updateShapeLines(shape_go, CityLayoutMesh.PickingState.Normal, pix_to_world_scale);
		}

		yield return null;
	}

	IEnumerator UpdateShapeState(float pix_to_world_scale)
	{
		foreach (var shape_go in shapes)
		{
			CityLayoutMesh.highlightShape(shape_go, CityLayoutMesh.PickingState.Normal, pix_to_world_scale);
			//CityLayoutMesh.updateShapeLines(tile_go, CityLayoutMesh.PickingState.Normal, pix_to_world_scale);
		}

		yield return null;
	}

}
