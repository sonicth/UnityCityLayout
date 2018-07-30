using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public class FitBox
{

	public Vector2 vmin;
	public Vector2 vmax;

	readonly float radius;

	public Vector2 Scale
	{
		get
		{
			// scale
			var diff = vmax - vmin;
			return new Vector2(radius * 2, radius * 2) / diff;
		}
	}

	public Vector2 Translate
	{
		get
		{
			// translation to centre! opposite to the centre of the box
			var vcentre = (vmax + vmin) * 0.5f;
			return -vcentre;
		}

	}

	public FitBox(float radius_)
	{
		radius = radius_;

		// initialise min/max
		vmin = new Vector2(float.MaxValue, float.MaxValue);
		vmax = new Vector2(float.MinValue, float.MinValue);
	}

	public void UpdateWithVertex(Vector2 vx)
	{
		vmin = Vector2.Min(vmin, vx);
		vmax = Vector2.Max(vmax, vx);
	}

	internal Vector2 TransformAffine(Vector2 vx)
	{
		var scale = Math.Min(Scale.x, Scale.y);
		return (vx + Translate) * scale;
	}
}
