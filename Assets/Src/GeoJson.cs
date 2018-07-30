using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// needed for Vector2
using UnityEngine;

// GeoJson and dependencies
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;


using PolygonEntry = System.Tuple<UnityEngine.Vector2[], string>;

class GeoJson
{
	

	public static List<PolygonEntry> GetPolygonsFromSerialised(string seralised_geometry)
	{
		// NOTE change var to dynamic
		var geo_features = JsonConvert.DeserializeObject<FeatureCollection>(seralised_geometry);
		var f1 = geo_features.Features[0];
		Debug.Log("number of features " + geo_features.Features.Count);


		// train boundary
		var bds = new FitBox(50);
		bds.Update(IteratorVector2(geo_features.Features));
		var scale = Math.Min(bds.Scale.x, bds.Scale.y);
		var translate = bds.Translate;

		var polygon_entries = new List<PolygonEntry>(geo_features.Features.Count);
		// convert geo data to gameobjects
		foreach (var poly_tuple in IteratorPoly(geo_features.Features))
		{
			var poly = poly_tuple.Item1;
			var poly_name = poly_tuple.Item2;

			var l = PolyIteratorVector2(poly).ToList();
			var vs_transformed = new Vector2[l.Count];
			int i = 0;
			foreach (var v in l)
			{
				vs_transformed[i] = (v + translate) * scale;
				++i;
			}

			//var ls1 = poly.Coordinates[0] as LineString;
			//var pt1 = ls1.Coordinates[0];
			//ls1.Coordinates[0] = new Position(pt1.Latitude, pt1.Longitude);

			polygon_entries.Add(new PolygonEntry(vs_transformed, poly_name));
		}

		return polygon_entries;
	}


	static IEnumerable<Vector2> PolyIteratorVector2(Polygon poly)
	{
		// first linestring == outer ring?
		var ring = poly.Coordinates[0];

		int i = 0;
		var last_i = ring.Coordinates.Count - 1;
		foreach (var coord in ring.Coordinates)
		{
			// eat last vertex since the polygon is 'closed' automatically 
			if (i == last_i)
				break;

			yield return new Vector2((float)coord.Longitude, (float)coord.Latitude);
			++i;
		}
	}


	static IEnumerable<Vector2> IteratorVector2(List<Feature> features)
	{
		foreach (var poly_tuple in IteratorPoly(features))
		{
			foreach (var vx in PolyIteratorVector2(poly_tuple.Item1))
			{
				yield return vx;
			}
		}
	}

	static IEnumerable<Tuple<Polygon, string>> IteratorPoly(List<Feature> features)
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
				string poly_name = "shape_" + i.ToString("D6") + "_" + f.Properties["LU_DESC"] + "_" + f.Properties["OID_"];

				foreach (var poly in mpoly.Coordinates)
				{
					bool are_holes = poly.Coordinates.Count > 1;
					if (!are_holes)
						continue;

					//Debug.Log("polygon rings: ***" + poly.Coordinates.Count.ToString());


					yield return new Tuple<Polygon, string>(poly, poly_name);
				}

			}
			else
			{
				throw new Exception("do not know how to handle geometry of type " + f.Geometry.Type.ToString()+", expected a multipolygon!");
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
		bds.Update(PolyIteratorVector2(poly));
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
