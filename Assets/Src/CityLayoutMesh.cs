using System.Collections.Generic;
using UnityEngine;

class CityLayoutMesh
{

	private static Mesh GetTriangleMeshBasic(Vector2[] vxs)
	{
		// convert to 3D space
		var vxs3d = System.Array.ConvertAll<Vector2, Vector3>(vxs, v => v);
	
		// triangulate polygon
		var triangulator = new Triangulator(vxs);
		var indices = triangulator.Triangulate();

		// create mesh
		var m = new Mesh();
		
		// assign to mesh
		m.vertices = vxs3d;
		m.triangles = indices;

		// recompute geometry
		// QUESTION why do we need bounds?
		m.RecalculateNormals();
		m.RecalculateBounds();

		return m;
	}

	private static Mesh GetTriangleMeshP2T(List<Vector2[]> poly_data)
	{
		var vxs = poly_data[0];

		// 1. convert to P2T polygon format
		var points = new List<Poly2Tri.PolygonPoint>(vxs.Length);

		foreach (Vector2 vx in vxs)
		{
			//NOTE float -> double conversion
			points.Add(new Poly2Tri.PolygonPoint((double) vx.x, (double) vx.y));
		}
		var p2t_poly = new Poly2Tri.Polygon(points);

		// 2. triangulate!
		Poly2Tri.P2T.Triangulate(p2t_poly);

		// 3. read triangle data
		//NOTE vertices are duplicated, e.g. each vertex to each index!
		var tri_vxs = new Vector3[p2t_poly.Triangles.Count * 3];
		var tri_ids = new int[p2t_poly.Triangles.Count * 3];
		int i = 0;
		foreach (var tri in p2t_poly.Triangles)
		{
			tri_vxs[i + 0] = new Vector3(tri.Points[0].Xf, tri.Points[0].Yf, 0);
			tri_vxs[i + 1] = new Vector3(tri.Points[2].Xf, tri.Points[2].Yf, 0);
			tri_vxs[i + 2] = new Vector3(tri.Points[1].Xf, tri.Points[1].Yf, 0);

			tri_ids[i + 0] = i + 0;
			tri_ids[i + 1] = i + 1;
			tri_ids[i + 2] = i + 2;

			i += 3;
		}

		// 4. construct mesh using the generated triangles
		var m = new Mesh();
		m.vertices = tri_vxs;
		m.triangles = tri_ids;
		m.RecalculateNormals();
		m.RecalculateBounds();

		return m;
	}



	public static GameObject createMeshFromRingVertices(float r, List<Vector2[]> poly_data, string name)
	{
		var go = new GameObject();
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		// generate mesh	
		try
		{
			go.GetComponent<MeshFilter>().mesh = GetTriangleMeshP2T(poly_data);
		} catch(System.SystemException e)
		{
			Debug.LogError("p2t failed on mesh: " + name+", \n\tmessage: ###"+e.Message + "###");
			go.GetComponent<MeshFilter>().mesh = GetTriangleMeshBasic(poly_data[0]);
		}

		var s = Shader.Find("Standard");
		go.GetComponent<MeshRenderer>().material = new Material(s);
		// tilt to xz plane
		go.transform.rotation = Quaternion.Euler(90, 0, 0);

		return go;
	}
}

