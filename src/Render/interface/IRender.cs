using System;
using System.Drawing;

using Voxelesque.Render.Common;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
namespace Voxelesque.Render{
    interface IRender{

        //mixed bits
        bool Init(RenderSettings settings);

        void Run();

        /**
        <summary>
        This action is called every update - 15 times each second by default
        </summary>
        */
        Action<double> OnVoxelesqueUpdate {get; set;}

        RenderSettings Settings{get;}

        //meshes
        IRenderMesh LoadMesh(float[] vertices, uint[] indices);

        ///**
        //<summary> 
        //loads a VMF file as a mesh (.vemf, .vbmf, .vmf)
        //More details about how vmf files work in the documentation
        //</summary>
        //*/
        //IRenderMesh LoadMesh(string VMFPath);

        void DeleteMesh(IRenderMesh mesh);

        IRenderTexture LoadTexture(string filePath);

        IRenderTexture LoadTexture(Bitmap image);

        void DeleteTexture(IRenderTexture texture);

        //shaders

        /**
        <summary>
        loads, compiles, and links a shader program.

        Note that, for a GL33Render for example, "fragment.glsl" and "vertex.glsl" is appended to the shader path for the pixel and vertex shaders respectively.
        </summary>

        */
        IRenderShader LoadShader(string shaderPath);

        void DeleteShader(IRenderShader shader);
        

        //entities

        IRenderEntity SpawnEntity(EntityPosition pos, IRenderShader shader, IRenderMesh mesh, IRenderTexture texture);

        void DeleteEntity(IRenderEntity entity);

        //camera

        RenderCamera SpawnCamera(Vector3 position, Vector3 rotation, float fovy);

        void SetCamera(RenderCamera camera);
        void DeleteCamera(RenderCamera camera);

        //input
        KeyboardState Keyboard();

        MouseState Mouse();

        bool CursorLocked{get; set;}
    }
}