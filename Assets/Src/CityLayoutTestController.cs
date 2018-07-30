using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CityLayoutTestController : MonoBehaviour {

	int updates = 0;
	private List<GameObject> tiles;
	private readonly float scale = 10;

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
		var r = scale * 0.5f;
		var vxs = new Vector2[] { new Vector3(0, 0), new Vector3(0, r), new Vector3(r, r), new Vector3(r, 0) };

		// create mesh
		var tile = CityLayoutMesh.createMeshFromRingVertices(scale, vxs, "<square>");

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
		var polygon_entries = GeoJson.GetPolygonsFromSerialised(seralised_geometry);

		// 3) create GemeObjects with meshes from each polygon
		tiles = new List<GameObject>(polygon_entries.Count);
		foreach (var entry in polygon_entries)
		{
			var vec2_array = entry.Item1;
			var name = entry.Item2;

			var tile = CityLayoutMesh.createMeshFromRingVertices(scale, vec2_array, name);
			tile.name = name;
			tile.transform.parent = transform;
			tiles.Add(tile);
		}
	}
	
}
