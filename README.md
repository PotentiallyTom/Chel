# CHEL - The arbitrary 4D rendering engine

## Introduction

`Chel` is a rendering engine for arbitrary 4-polytopes. It is possible to load objects via `styl` files, which contain the 4D object data for a given object. The rendering method is controlled by a system of "Render Packs". A render pack corresponds to a single rendering method, and is composed of a compute, vertex, and fragment shader, as well as a `YAML` file containing additional information about how the object should be rendered. It is written in C#, using OpenTK to interact with the GPU.

## Installation

To install, clone the git repo and use `dotnet build` to build from source, the dependancies should be automatically downloaded. If not, the specific version is stored in the `Refactor.csproj` file.

## Usage

To use, either run `dotnet run` from the command line, or run the executable produced by `dotnet build`. Paths to the specific render pack and object can be supplied as arguments

```
> Chel.exe <path/to/renderpack.yml> <path/to/object.styl>
> dotnet run <path/to/renderpack.yml> <path/to/object.styl>
```

Once open, the object can be interacted with in 3D using the mouse, and 4D using the keyboard.

Left-click and drag for `XZ` and `YZ` rotations, Right-click and drag for `XY` rotation. Use the scroll wheel to zoom in and out.

The `Q,W,E,A,S,D` are used to control rotation in 4D.

|Key|Plane of Rotation|
|-|-|
|Q|XW|
|W|YW|
|E|ZW|
|A|XY|
|S|XZ|
|D|YZ|

Hold `Shift` to reverse the direction of rotation, and press `R` to reset the 4D view.

Arbitrary inputs are handled by the bottom row of the keyboard: `Z, X, V, B, N, M`, and the `,` key. These correspond to the values of `ComputeShaderUniforms` and `VertexFragmentShaderUniforms`. Hold shift to reverse the input.

## Additional Render Packs

Additional render packs can be created using the format provided.

## Project Structure

* `/Models/`: Contains `styl` files that contain the object data.

* `/Parse/`: Contains the `HyperObject` and `HyperTetrahedron` structs, and the code for parsing `styl` files.

* `/Render/`: Contains the rendering code, and the Render Pack Parser.

* `/RenderPacks/`: Contains the render pack `YAML` files

* `/Shaders/`: Contains the shaders used by the render packs

* `/generate_pointcloud.py`: Contains code used to generate the test data for the point cloud render pack

* `/Program.cs`: Program Entry Point.

## License

This software is distributed under the [MIT License](https://mit-license.org/)