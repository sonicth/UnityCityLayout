using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeProperties : MonoBehaviour {

	private Dictionary<string, object> _properties;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public string Name
	{
		get
		{
			// polygon_idx.ToString("D6") + "_"
			var desc = PropertiesDict.ContainsKey("LU_DESC") ? PropertiesDict["LU_DESC"] as string : "";
			var object_id = PropertiesDict.ContainsKey("OID_") ? PropertiesDict["OID_"].ToString() : "";
			string mpoly_name = "shape_" + desc + "_" + object_id;
			return mpoly_name;
		}

	}

	public Dictionary<string, object> PropertiesDict
	{
		get
		{
			return _properties;
		}

		set
		{
			_properties = value;
		}
	}

	// NOTE temporary method
	public override string ToString()
	{
		string props_serialised = "";
		foreach (var key in PropertiesDict.Keys)
		{
			props_serialised += key + " => " + PropertiesDict[key]+"\n";
		}
		return "Properties: {"+ props_serialised + "}";
	}
}
