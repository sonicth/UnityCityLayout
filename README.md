# UnityCityLayout

## About
Render and visualise layout stored in GeoJson format. The target use of this App is to preview city layouts, however it can be used to view other kinds of shapes.

### How to Use

Checkout and open in Unity (version 2018.2.0f2+). Replace `Assets/ShapeData/Example1.geojson` or add another GeoJson file and update  `Assets/Src/InputModel.cs` say by adding a line:

```filename = "Assets/ShapeData/MyShapes.geojson";```.

Start the Game in Unity. Use the *Alternative-Click* to pan around, Scroll Wheel to zoom in/out and *Primary-Click* to select an object and to display its attributes (properties) in the console.

## Implemented Details

### Code Layout

Functionality is divided among the following modules stored in separate files:

1. **Input model**, which reads (currently only) a hardcoded file and returns a *GeoJson* text as string. This can be changed to accept user input in an arbitrary form.

2. **GeoJson model**, which parses the text and returns `PolygonData`, which consists of *polygon vertices* and *properties* (currently shared with other polygons). Polygon vertices are represented as a list of *ring*s, where the first ring is the *outer boundary* and, optionally more rings which are polygon *holes*. Ring vertices is an array of Unity `Vector2` types.

3. **Mesh model**, which takes `PolygonData` and a Unity `GameObject` and creates everything that is necessary for rendering the shape, including a triangluated *mesh*.

4. **BoxMapping module**, maps Geo coordinates to a plane. Currently uses spherical coordinates only.

5. **Scene controller**, responsible of loading of the scene data. It has minimum functionality and basically used *Input*, *GeoJson*, *BoxMapping", and *Mesh* modules to great `GameObject`s. A fixed number of shapes per frame update are produced to begin with (for example 64 at a time at the time of writing).

6. **Camera controller**, responsible for interaction of user by taking mouse input. and navigation and selection within the scene. Once the Scene controller is finished loading of GameObjects, Camera controller is responsible any for functionality as the scene is being navigated by the user.

### (GeoJson) Geometry Input

GeoJson module processes text data into a Unity-compatible data structure. A GeoJson data file, which, for example, may represent a part of City Layout, rougly speaking, is organised as a list *Feature*s. A Feature represents a collection of polygons, *multipolygons*, which can relate to a particular object in a city. Polygon is represented as a list of rings or boundary loops, where the first ring is its *outer boundary* and other, optional, rings are *hole*s or *inner boundaries*. 

[*GeoJson.NET*](https://github.com/ahokinson/GeoJson.NET-Unity) Unity port is used to parse the input, which is in turn based on [*Json.NET*](https://github.com/ahokinson/GeoJson.NET-Unity). The parsed data structure contains read-only features. This means that if we want to modify one vertex we need to regenerate the entire multipolygon during the conversion to JSON. This is probably not a big problem in case the App needs to be extended to write data out, since the update only needs to be performed once when the scene state needs to be saved (however currently this is not supported).

### Coordinate Mapping

The GeoJson references Geo data, say in WGS 84 format, and it needs to be projected into a plane. 

Currently, the coordinates (*Longitude* and *Lattitude*) are interpreted as spherical coordinates and are simply flattened out (mapped to *x* and *y* on the plane). Futhermore, coordinates are fitted into a *square* of a given size centered at the origin.

### Mesh Processing and Rendering

The Mesh model is used by Scene controller to convert geometric data into Unity scene objects, represented as `GameObject` instances. 

Since Unity can only render triangles, a shape needs to be triangulated. To acomplish this, first I have tried the `Triangulator` class from the [Unity Wiki](http://wiki.unity3d.com/index.php/Triangulator), though it only works for the most basic polygons.
I also have tried [*Poly2Tri*](https://github.com/greenm01/poly2tri) library, but even though it can handle some concave shapes, library fails for more complex geometry (probably because the work on this library is still in progress). This App uses the third library that was tested, [*Triangle.NET*](https://github.com/eppz/Triangle.NET), which correctly triangulates every polygon tested (with the exception of an occasional stack overflow), including the ones with holes. It is based on another [tool](http://www.cs.cmu.edu/~quake/triangle.html) and developed by Jonathan Shewchuk and written in C, which works by using a *Delaunay graph*.

Polygon boundaries are rendered by creating additional `GameObject`s for each ring and adding `LineRenderer` component to each of them. Line width is rescaled each time the user zooms in or out.

### Problems
Line rendering results in a number of artefacts and I assume that Unity also triangulates the line. In the future, it would be interesting to try OpenGL line rendering.
