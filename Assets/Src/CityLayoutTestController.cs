﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CityLayoutTestController : MonoBehaviour {

	int updates = 0;
	private List<GameObject> tiles;

	public readonly bool DRAW_LINES = false;
	public readonly float BOX_RADIUS = 50;

	// Use this for initialization
	void Start () {

		AddGeoJsonGeometry(tiles);
		//void AddSquare();
		//GeoJson.TestConversion();
	}



	// Update is called once per frame
	void Update () {
		++updates;

		if (updates > 1000)
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

	void AddSquare()
	{		
		// get data
		var r = BOX_RADIUS * 0.5f;
		var vxs = new Vector2[] { new Vector3(0, 0), new Vector3(0, r), new Vector3(r, r), new Vector3(r, 0) };

		// create mesh
		var tile = CityLayoutMesh.createMeshFromPolygonData(
			new Tuple<List<Vector2[]>, string>(new List<Vector2[]> { vxs }, 
			"<square>"));

		// add locally
		tiles = new List<GameObject>(1);
		tile.transform.parent = transform;
		tiles.Add(tile);
	}



	void AddGeoJsonGeometry(List<GameObject> tiles)
	{
		// 1) get serialised textual geometry data (e.g. read from file)
		var seralised_geometry = CityLayoutModel.GetSerialisedGeometry();

		// 2) read serialised geometry (e.g. GeoJson) into list/collection of Vector2[]
		var polygon_dataset = GeoJson.GetPolygonDatasetFromSerialised(seralised_geometry, BOX_RADIUS);

		// 3) create GameObjects with meshes from each polygon
		//TODO single gameobject/mesh here...??
		tiles = new List<GameObject>(polygon_dataset.Count);
		foreach (var polygon_data in polygon_dataset)
		{
			GameObject tile;
			if (DRAW_LINES)
			{
				tile = CityLayoutMesh.createLinesFromPolygonData(polygon_data);
			}
			else
			{
				tile = CityLayoutMesh.createMeshFromPolygonData(polygon_data);
			}
			tile.transform.parent = transform;
			tiles.Add(tile);
		}

		var _dummy = 0xf154;

	}


}
