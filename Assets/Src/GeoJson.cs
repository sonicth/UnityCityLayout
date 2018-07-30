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


using PolygonData = System.Tuple<
						System.Collections.Generic.List<UnityEngine.Vector2[]>, 
						string>;
using PolygonInput = System.Tuple<
						GeoJSON.Net.Geometry.Polygon, 
						string>;

class GeoJson
{
	

	public static List<PolygonData> GetPolygonDatasetFromSerialised(string seralised_geometry)
	{
		// parse Json into list of read-only features
		var geo_features = JsonConvert.DeserializeObject<FeatureCollection>(seralised_geometry);

		// train boundary
		var fit_box = new FitBox(50);
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



	static private Vector2[] RingVectors(LineString ring)
	{
		var ring_vectors = new Vector2[ring.Coordinates.Count - 1];

		int i = 0;
		var last_i = ring.Coordinates.Count - 1;
		foreach (var coord in ring.Coordinates)
		{
			// eat last vertex since the polygon is 'closed' automatically 
			if (i == last_i)
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

			foreach (var ring in poly_input.Item1.Coordinates)
			{
				var ring_vectors = RingVectors(ring);
				poly_vectors.Add(ring_vectors);
			}
			
			yield return new PolygonData(poly_vectors, poly_name);
		}
	}

	static IEnumerable<PolygonInput> IteratorFeatureToPolygonInput(IEnumerable<Feature> features)
	{

		int i = 0;
		foreach (var f in features)
		{
			if (i > 200000)
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
					if (!are_holes)
						continue;

					// if there are multiple polygons in the set, add index prefix starting from the second one
					var poly_name = mpoly_name + (i > 0 ? "_" + pi.ToString("D2") : "");

					yield return new PolygonInput(poly, poly_name);
					++pi;
				}


			}
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
				//yield return ();
				Polygon p = f.Geometry as Polygon;
				foreach (var k in f.Properties.Keys)
				{
					Debug.Log("key: " + k + ", \tvalue: " + f.Properties[k].ToString());
				}

				//new Polygon(
				//new List<LineString> { new LineString(new List<IPosition> {
				//	new Position(10, 10),
				//	new Position(11, 12),
				//	new Position(10, 12),
				//	new Position(10, 10),
				//}) });
			}

		}

		string geo_txt_out = JsonConvert.SerializeObject(geo_features);
		File.WriteAllText(put_path, geo_txt_out);
	}



	//
	///  test stuff
	//
	void AddPolygon()
	{
		var offset = 0;
		var poly = new Polygon(new List<LineString>
				{
					new LineString(new List<IPosition>
					{
						new Position(52.959676831105995 + offset, -2.6797102391514338 + offset),
						new Position(52.9608756693609 + offset, -2.6769029474483279 + offset),
						new Position(52.908449372833715 + offset, -2.6079763270327119 + offset),
						new Position(52.891287242948195 + offset, -2.5815104708998668 + offset),
						new Position(52.875476700983896 + offset, -2.5851645010668989 + offset),
						new Position(52.882954723868622 + offset, -2.6050779098387191 + offset),
						new Position(52.875255907042678 + offset, -2.6373482332006359 + offset),
						new Position(52.878791122091066 + offset, -2.6932445076063951 + offset),
						new Position(52.89564268523565 + offset, -2.6931334629377890 + offset),
						new Position(52.930592009390175 + offset, -2.6548779332193022 + offset),
						new Position(52.959676831105995 + offset, -2.6797102391514338 + offset)
					})
				});

		var bds = new FitBox(10);

		//bds.Update(PolyIteratorVector2(poly));
		//TODO poly object --> vec2 data --> gameobject
	}

	public static void TestConversion()
	{
		string json = "{\"coordinates\":[-2.124156,51.899523],\"type\":\"Point\"}";

		//var conv = new GeoJSON.Net.Converters.GeoJsonConverter[] { new GeoJSON.Net.Converters.GeoJsonConverter() };
		//Debug.Log("can write " + conv[0].CanWrite.ToString() + ", can read " + conv[0].CanRead.ToString());

		//Point point = JsonConvert.DeserializeObject<Point>(json);
		Point point = new Point(new Position(0, 0));
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

		//NOTE this works!
		//var stuff = JObject.Parse(json);
		//stuff["type"] = "Fish";

		//var out_json = stuff.ToString();
		//Debug.Log(out_json);

		Polygon p;
		//p.Coordinates
		Position position = new Position(51.899523, -2.124156);

		//var features = new List<Feature> { new Feature(point) };
		//var pt = (features[0].Geometry as Point);
		//Point;
		//features[0].Geometry = p;
		var fcol = new FeatureCollection();
		var fitem = new Feature(ls);
		fcol.Features.Add(fitem);
		fcol.Features.Clear();

		//Point point = new Point(position);
		//string json = JsonConvert.SerializeObject(point);
	}
}
