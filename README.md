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

