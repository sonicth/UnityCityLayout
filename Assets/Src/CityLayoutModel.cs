
class CityLayoutModel
{
	public static string GetSerialisedGeometry()
	{
		string filename = "";

		filename = "Assets/ShapeData/Parks.geojson";
		//filename = "Assets/ShapeData/Feature_88621.geojson";

		// default shape file (in the repository!)
		if (!System.IO.File.Exists(filename))
		{
			filename = "Assets/ShapeData/Example1.geojson";
		}

		if (!System.IO.File.Exists(filename))
		{
			throw new System.Exception("GeoJson file does not exist: " + filename);
		}
		else
		{
			var file = new System.IO.StreamReader(filename);
			string serialised_geometry = file.ReadToEnd();
			file.Close();

			return serialised_geometry;
		}
	}
}
