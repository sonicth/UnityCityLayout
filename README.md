# UnityCityLayout

## About
Render and visualise layout stored in GeoJson format. Possible use of this App is to preview city layouts. However, it can be used to view other kinds of shapes.

### How to Use

Checkout and open in Unity (version 2018.2.0f2+). Replace `Assets/ShapeData/Example1.geojson` or add another GeoJson file and update  `Assets/Src/InputModel.cs` say by adding a line:

```filename = "Assets/ShapeData/MyShapes.geojson";```.

Start the Game in Unity. Use the *Alternative-Click* to pan around, Scroll Wheel to zoom in/out and *Primary-Click* to select an object and to display its attributes (properties) in the console.

## Implemented Details

### Code Layout

Functionality is divided among the following modules stored in separate files:

1. **Input model**, which reads (currently only) a hardcoded file and returns a *GeoJson* text as a string. This detail can be changed to accept user input in an arbitrary form.

2. **GeoJson model**, which parses the text and returns `PolygonData`, which consists of *polygon vertices* and *properties* (currently shared with other polygons). Polygon vertices are represented as a list of *ring*s, where the first ring is the *outer boundary* and, optionally more rings which are polygon *holes*. Ring vertices is an array of Unity `Vector2` types.

3. **Mesh model**, which takes `PolygonData` and a Unity `GameObject` and creates everything that is necessary for rendering the shape, including a triangulated *mesh*.

4. **BoxMapping module**, maps Geo coordinates to a plane. Currently uses spherical coordinates only.

5. **Scene controller**, responsible for the loading of the scene data. It has minimum functionality and mainly uses *Input*, *GeoJson*, *BoxMapping", and *Mesh* modules to create `GameObject`s. A fixed number of shapes per frame update are produced, to begin with (for example 64 at a time at the time of writing).

6. **Camera controller**, responsible for the interaction of user by taking mouse input. Once the Scene controller is finished loading of GameObjects, Camera controller is responsible any for functionality as the scene is being navigated by the user.

### (GeoJson) Geometry Input

GeoJson module processes text data into a Unity-compatible data structure. A GeoJson data file, which, for example, may represent a part of City Layout, roughly speaking, is organised as a list *Feature*s. A Feature represents a collection of polygons, *multipolygons*, which can relate to a particular object in a city. Polygon is represented as a list of rings or boundary loops, where the first ring is its *outer boundary* and other, optional, rings are *hole*s or *inner boundaries*. 

[*GeoJson.NET*](https://github.com/ahokinson/GeoJson.NET-Unity) Unity port is used to parse the input, which is in turn based on [*Json.NET*](https://github.com/ahokinson/GeoJson.NET-Unity). The parsed data structure contains read-only features.

### Coordinate Mapping

The GeoJson references Geo data, say in WGS 84 format, and it needs to be projected onto a plane. 

Currently, the coordinates (*Longitude* and *Lattitude*) are interpreted as spherical coordinates and are merely flattened out (mapped to *x* and *y* on the plane). Furthermore, coordinates are fitted into a *square* of a given size centred at the origin.

### Mesh Processing and Rendering

The Mesh model is used by Scene controller to convert geometric data into Unity scene objects, represented as `GameObject` instances. 

Unity can only render triangles, so a shape needs to be triangulated before it can be displayed. To accomplish this, first I have tried the `Triangulator` class from the [Unity Wiki](http://wiki.unity3d.com/index.php/Triangulator), though it only works for the most basic polygons.
I also have tried [*Poly2Tri*](https://github.com/greenm01/poly2tri) library, but even though it can handle some concave shapes, library fails for more complex geometry (probably because the work on this library is still in progress). The third library that I came across,  [*Triangle.NET*](https://github.com/eppz/Triangle.NET), correctly triangulates every polygon tested (except for an occasional stack overflow), including the ones with holes, and it is used in this App. The library is based on [another tool](http://www.cs.cmu.edu/~quake/triangle.html) that was developed by Jonathan Shewchuk and written in C (triangulation is performed with the help of a *Delaunay graph*).

Polygon boundaries are rendered by creating additional `GameObject`s for each ring and adding `LineRenderer` component to each of them. Line width is rescaled each time the user zooms in or out.

### User Interface

For navigation using a mouse an [external script](https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input) has been modified. The script allows to *pan* and *zoom* the camera. Since the camera is *orthographic*, zoom is implemented by scaling its width. I also added the adaptive zoom, where the change is proportional to the camera width -- in other words camera zooms less when closer.

Selection is implemented by raycasting a `GameObject` with a *mesh collider*. Three states are possible for selection: 1) *normal*, 2) *picked* (the pointer hovers over the polygon) and 3) *selected* (user clicks during picking). Each of the states has three separate colours for both shapes and boundaries.


### Problems
Triangulation with Triangle.NET causes a stack overflow on some objects. Line rendering results in some artefacts, and I assume that Unity also triangulates the line. In the future, it would be interesting to try OpenGL line rendering.
