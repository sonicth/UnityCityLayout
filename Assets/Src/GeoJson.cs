using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// needed for Vector2, Debug.*
using UnityEngine;

// GeoJson and dependencies
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

//TODO share with others
using PolygonData = System.Tuple<
					System.Collections.Generic.List<UnityEngine.Vector2[]>,
					string>;

using PolygonInput = System.Tuple<
						GeoJSON.Net.Geometry.Polygon, 
						string>;

public class GeoJson
{

	enum PolygonsWithHoles { All, WithHolesOnly, WithoutHolesOnly };

	private const int MAX_FEATURES_TO_READ = 400000;
	private const PolygonsWithHoles loadWithHoles = PolygonsWithHoles.All;
	private const bool discardHoles = false;

	public static List<PolygonData> GetPolygonDatasetFromSerialised(string seralised_geometry, float box_radius)
	{
		// parse Json into list of read-only features
		var geo_features = JsonConvert.DeserializeObject<FeatureCollection>(seralised_geometry);

		// train boundary
		var fit_box = new FitBox(box_radius);
		foreach (var vx in IteratorFeatureOuterVxs(geo_features.Features))
		{
			fit_box.UpdateWithVertex(vx);
		}

		// convert features to geometry data
		var polygon_dataset = IteratorFeatureToPolygonData(geo_features.Features).ToList();
		
		// transform geometry data to scene coordinates
		foreach (var poly_data in polygon_dataset)
		{
			foreach (var ring_data in poly_data.Item1)
			{
				var vxi = 0;
				foreach (var vx in ring_data)
				{
					ring_data[vxi] = fit_box.TransformAffine(vx);
					++vxi;
				}
			}
		}

		return polygon_dataset;
	}

	readonly static bool STRIP_LAST_RING_VERTEX = false;

	static private Vector2[] RingVectors(LineString ring)
	{
		var ring_vectors = new Vector2[ring.Coordinates.Count];

		int i = 0;
		var last_i = ring.Coordinates.Count - 1;
		foreach (var coord in ring.Coordinates)
		{
			// eat last vertex since the polygon is 'closed' automatically 
			if (i == last_i && STRIP_LAST_RING_VERTEX)
				break;

			ring_vectors[i] = new Vector2((float)coord.Longitude, (float)coord.Latitude);
			++i;
		}

		return ring_vectors;
	}


	static IEnumerable<Vector2> IteratorFeatureOuterVxs(IEnumerable<Feature> features)
	{
		foreach (var poly_data in IteratorFeatureToPolygonData(features))
		{
			// take only the first ring!
			var first_ring_vectors = poly_data.Item1[0];

			foreach (var vx in first_ring_vectors)
			{
				yield return vx;
			}
		}
	}

	static IEnumerable<PolygonData> IteratorFeatureToPolygonData(IEnumerable<Feature> features)
	{
		foreach (var poly_input in IteratorFeatureToPolygonInput(features))
		{
			// first linestring == outer ring?
			var num_rings = poly_input.Item1.Coordinates.Count;
			var poly_vectors = new List<Vector2[]>(num_rings);
			var poly_name = poly_input.Item2;

			int ri = 0;
			foreach (var ring in poly_input.Item1.Coordinates)
			{
				if (ri == 0 || !discardHoles)
				{
					var ring_vectors = RingVectors(ring);
					poly_vectors.Add(ring_vectors);
				}
				++ri;
			}
			
			yield return new PolygonData(poly_vectors, poly_name);
		}
	}

	static IEnumerable<PolygonInput> IteratorFeatureToPolygonInput(IEnumerable<Feature> features)
	{

		int i = 0;
		foreach (var f in features)
		{
			if (i >= MAX_FEATURES_TO_READ)
				break;

			if (f.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.MultiPolygon)
			{
				// shape represented with multipolygon
				var mpoly = (f.Geometry as MultiPolygon);

				// game object name
				string mpoly_name = "shape_" + i.ToString("D6") + "_" + f.Properties["LU_DESC"] + "_" + f.Properties["OID_"];

				int pi = 0;
				foreach (var poly in mpoly.Coordinates)
				{
					bool are_holes = poly.Coordinates.Count > 1;

#pragma warning disable 0162
					if ((are_holes && loadWithHoles == PolygonsWithHoles.WithoutHolesOnly)
						|| (!are_holes && loadWithHoles == PolygonsWithHoles.WithHolesOnly))
						continue;
#pragma warning restore 0162

					// if there are multiple polygons in the set, add index prefix starting from the second one
					var poly_name = mpoly_name + (pi > 0 ? "_" + pi.ToString("D2") : "");

					yield return new PolygonInput(poly, poly_name);
					++pi;
				}


			}
			// NOTE a single polygon feature for debugging purposes; normally expect only multipolygons in the input.
			else if (f.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.Polygon)
			{
				var poly = (f.Geometry as Polygon);				
				string poly_name = f.Properties.ContainsKey("name") 
					? ("polygon: " + f.Properties["name"]) 
					: "<polygon>";

				yield return new PolygonInput(poly, poly_name);
			}
			else
			{
				Debug.LogWarningFormat("Ignoring shape of type <{0}>", f.Geometry.Type.ToString());
			}

			++i;
		}

	}

	//
	// debug stuff
	//
	private void writeGJ(string put_path, FeatureCollection geo_features)
	{

		foreach (var f in geo_features.Features)
		{
			if (f.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.Polygon)
			{
				foreach (var k in f.Properties.Keys)
				{
					Debug.Log("key: " + k + ", \tvalue: " + f.Properties[k].ToString());
				}
			}

		}

		string geo_txt_out = JsonConvert.SerializeObject(geo_features);
		File.WriteAllText(put_path, geo_txt_out);
	}



	//
	///  test stuff
	//

	public static void TestConversion()
	{
		var offset = 0;
		LineString ls = new LineString(
			new List<IPosition>
					{
						new Position(52.959676831105995 + offset, -2.6797102391514338 + offset),
						new Position(52.9608756693609 + offset, -2.6769029474483279 + offset),
						new Position(52.908449372833715 + offset, -2.6079763270327119 + offset),
						new Position(52.959676831105995 + offset, -2.6797102391514338 + offset),
					}
			);
		var fcol = new FeatureCollection();
		var fitem = new Feature(ls);
		fcol.Features.Add(fitem);
		fcol.Features.Clear();

	}
}
