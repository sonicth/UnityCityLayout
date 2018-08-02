using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

//TODO share with others!
using PolygonData = System.Tuple<
						System.Collections.Generic.List<UnityEngine.Vector2[]>,
						ShapeProperties>;

using ShapeColors = System.Tuple<
						UnityEngine.Color,
						UnityEngine.Color
						>;

class CityLayoutMesh
{
	// line drawing config
	private const float LINE_WIDTH_DEFAULT = 2f;
	private const float LINE_WIDTH_SELECTED = 8f;
	enum ELineDrawing { None, All, SelectedOnly };
	private const ELineDrawing LINE_DRAWING = ELineDrawing.All;

	public static readonly Color colorDefault = new Color(0.0452f, 0.6376f, 0.0613f);
	public static readonly Color colorDefaultLine = new Color(0.1f, 0.1f, 0.1f);

	public static readonly Color colorPicking = colorDefault + new Color(0.2f, 0.2f, 0.2f);
	public static readonly Color colorPickingLine = colorDefaultLine + new Color(0.2f, 0.2f, 0.2f);

	public static readonly Color colorSelected = new Color(0.3419f, 0.1384f, 0.0168f);
	public static readonly Color colorSelectedLine = new Color(1, 0, 0);




	public enum PickingState { Normal, Picking, Selected };

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

	private static Mesh GetTriangleMeshP2T(List<Vector2[]> poly_vertices)
	{
		var num_rings = poly_vertices.Count;
		if (num_rings == 0)
			throw new System.Exception("should be at least one ring in a polygon to create a mesh!");

		// 1. convert rings to intermediate polygons
		var polys = new List<Poly2Tri.Polygon>(num_rings);
		foreach (var ring_vertices in poly_vertices)
		{
			var points = new List<Poly2Tri.PolygonPoint>(ring_vertices.Length);

			foreach (var vx in ring_vertices)
			{
				//NOTE float -> double conversion
				points.Add(new Poly2Tri.PolygonPoint((double)vx.x, (double)vx.y));
			}
			polys.Add( new Poly2Tri.Polygon(points) );
		}

		// 2. if there are more than one polygon, they are inner rings or holes!!
		if (num_rings > 1)
		{
			for (int ri = 1; ri < num_rings; ++ri)
			{
				polys[0].AddHole(polys[ri]);
			}
		}

		// 3. triangulate!
		Poly2Tri.P2T.Triangulate(polys[0]);

		// 4. read triangle data
		//NOTE vertices are duplicated, e.g. each vertex to each index!
		var tri_vxs = new Vector3[polys[0].Triangles.Count * 3];
		var tri_ids = new int[polys[0].Triangles.Count * 3];
		int i = 0;
		foreach (var tri in polys[0].Triangles)
		{
			tri_vxs[i + 0] = new Vector3(tri.Points[0].Xf, tri.Points[0].Yf, 0);
			tri_vxs[i + 1] = new Vector3(tri.Points[2].Xf, tri.Points[2].Yf, 0);
			tri_vxs[i + 2] = new Vector3(tri.Points[1].Xf, tri.Points[1].Yf, 0);

			tri_ids[i + 0] = i + 0;
			tri_ids[i + 1] = i + 1;
			tri_ids[i + 2] = i + 2;

			i += 3;
		}

		// 5. construct mesh using the generated triangles
		var m = new Mesh();
		m.vertices = tri_vxs;
		m.triangles = tri_ids;
		m.RecalculateNormals();
		m.RecalculateBounds();

		return m;
	}

	private static Mesh GetTriangleDotNetMesh(List<Vector2[]> poly_vertices)
	{
		var num_vxs = 0;
		foreach (var ring_vxs in poly_vertices)
		{ num_vxs += ring_vxs.Length + 1; }

		var poly = new TriangleNet.Geometry.Polygon(num_vxs);

		// set vertices
		var ri = 0;
		foreach (var ring_vxs in poly_vertices)
		{
			var vxs_input = new List<TriangleNet.Geometry.Vertex>(ring_vxs.Length + 1);
			foreach (var vx in ring_vxs)
			{
				vxs_input.Add(new TriangleNet.Geometry.Vertex(vx.x, vx.y));
			}

			// add to poly
			bool is_hole = ri != 0;
			var contour = new TriangleNet.Geometry.Contour(vxs_input);
			poly.Add(contour, is_hole);
			++ri;
		}

		// triangulate
		var opts = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
		var tn_mesh = (TriangleNet.Mesh) poly.Triangulate(opts);

		// write
		var tri_vxs = new Vector3[tn_mesh.Triangles.Count * 3];
		var tri_ids = new int[tn_mesh.Triangles.Count * 3];
		int i = 0;
		foreach (var tri in tn_mesh.Triangles)
		{

			tri_vxs[i + 0] = new Vector3((float)tri.GetVertex(0).x, (float)tri.GetVertex(0).y, 0);
			tri_vxs[i + 1] = new Vector3((float)tri.GetVertex(2).x, (float)tri.GetVertex(2).y, 0);
			tri_vxs[i + 2] = new Vector3((float)tri.GetVertex(1).x, (float)tri.GetVertex(1).y, 0);

			tri_ids[i + 0] = i + 0;
			tri_ids[i + 1] = i + 1;
			tri_ids[i + 2] = i + 2;

			i += 3;
		}

		// create mesh
		var m = new Mesh();

		// assign to mesh
		m.vertices = tri_vxs;
		m.triangles = tri_ids;

		// recompute geometry
		// QUESTION why do we need bounds?
		m.RecalculateNormals();
		m.RecalculateBounds();

		return m;

	}

	public static void createMeshFromPolygonData(GameObject go, List<Vector2[]> poly_vertices, bool enable_lighting)
	{
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		// collider for raycasting
		go.AddComponent<MeshCollider>();


		// generate mesh	
		Mesh mesh;
		try
		{
			//mesh = GetTriangleMeshP2T(poly_vertices);
			mesh = GetTriangleDotNetMesh(poly_vertices);
		} catch(System.SystemException e)
		{
			Debug.LogWarning("p2t failed on mesh: " + go.name + ", \n\tmessage: ###"+e.Message + "###");
			mesh = GetTriangleMeshBasic(poly_vertices[0]);
		}
		go.GetComponent<MeshFilter>().mesh = mesh;

		// standart or unlit shader
		Shader shader = enable_lighting 
			? Shader.Find("Standard")
			: Shader.Find("Unlit/Color");

		go.GetComponent<MeshRenderer>().material = new Material(shader);
		
		// tilt to xz plane
		go.transform.rotation = Quaternion.Euler(90, 0, 0);

		// same collider mesh for raycasting
		//NOTE needs to be last, EVEN after transforms!
		go.GetComponent<MeshCollider>().sharedMesh = mesh;

#pragma warning disable 0162
		if (LINE_DRAWING != ELineDrawing.None)
		{
			var ri = 0;
			foreach (var ring_vertices in poly_vertices)
			{
				var line_object = createLinesFromPolygonData(ring_vertices, ri, shader);
				line_object.transform.parent = go.transform;
				++ri;
			}
		}
#pragma warning restore 0162
	}

	public static GameObject createLinesFromPolygonData(Vector2[] ring_vertices, int ring_index, Shader shader)
	{
		var name = "line_" + ring_index.ToString();
		var go = new GameObject(name);
		var lrenderer = go.AddComponent<LineRenderer>();
		//TODO reverse vertices
		var vxs3d = System.Array.ConvertAll<Vector2, Vector3>(ring_vertices, v => new Vector3(v.x, -0.01f, v.y));
		lrenderer.positionCount = vxs3d.Length;
		lrenderer.SetPositions(vxs3d);
		
		// start with zero width lines to begin with (updated later!)
		lrenderer.startWidth = 0;
		lrenderer.endWidth = 0;

		lrenderer.loop = true;
		lrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lrenderer.material = new Material(shader);
		return go;
	}

	private static ShapeColors getColors(PickingState state)
	{
		switch (state)
		{
			case PickingState.Picking:
				return new ShapeColors(colorPicking, colorPickingLine);

			case PickingState.Selected:
				return new ShapeColors(colorSelected, colorSelectedLine);

			case PickingState.Normal:
			default:
				return new ShapeColors(colorDefault, colorDefaultLine);
		}
	}

	public static void updateShapeLines(GameObject go, PickingState state, float pix_to_world_scale)
	{
		Color line_color = getColors(state).Item2;
		float line_width = (state == PickingState.Selected ? LINE_WIDTH_SELECTED : LINE_WIDTH_DEFAULT) * pix_to_world_scale;

#pragma warning disable 0162
		if (LINE_DRAWING != ELineDrawing.None)
		{
			for (int i = 0; i < go.transform.childCount; ++i)
			{
				var line_object = go.transform.GetChild(i).gameObject;

				// selected only: enable only if selected
				if (LINE_DRAWING == ELineDrawing.SelectedOnly)
				{

					line_object.SetActive(state == PickingState.Selected);
				}

				// update light
				var line_renderer = line_object.GetComponent<LineRenderer>();
				line_renderer.material.color = line_color;
				line_renderer.startWidth = line_width;
				line_renderer.endWidth = line_width;
			}
		}
#pragma warning restore 0162

	}

	public static void highlightShape(GameObject go, PickingState state, float pix_to_world_scale)
	{
		// get colours
		Color region_color = getColors(state).Item1;
		go.GetComponent<MeshRenderer>().material.color = region_color;

		updateShapeLines(go, state, pix_to_world_scale);
	}


}

