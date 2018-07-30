
class CityLayoutModel
{
	public static string GetSerialisedGeometry()
	{
		//var path = Directory.GetCurrentDirectory();
		//Debug.Log("***current directory: " + path);

		//var filename = "Assets/gj-test1.json";
		var filename = "C:/Users/fish/Documents/jobs/eth-singapore/Parks.geojson";

		if (!System.IO.File.Exists(filename))
		{
			throw new System.Exception("GeoJson file does not exist: " + filename);
		}
		else
		{
			// read text
			//var geo_txt = File.ReadAllText(filename);

			var file = new System.IO.StreamReader(filename);
			string serialised_geometry = file.ReadToEnd();
			file.Close();

			return serialised_geometry;
		}
	}
}
