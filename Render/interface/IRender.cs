using System;
using System.Collections.Generic;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using StbImageSharp;

using libvmodel;
namespace Render{
    public interface IRender{
        public static IRender CurrentRender;
        public static ERenderType CurrentRenderType;

        //mixed bits
        bool Init(RenderSettings settings);

        void Run();

        bool DebugRendering{get;set;}

        /**
        <summary>
        This action is called every update - 30 times each second.
        Entity components are automatically updated, 
        </summary>
        */
        Action<double> OnUpdate {get; set;}

        RenderSettings Settings{get;}

        Vector2 WindowSize();

        uint EntityCount();

        uint EntityCapacity();

        //meshes
        IRenderMesh LoadMesh(float[] vertices, uint[] indices);

        IRenderMesh LoadMesh(VMesh mesh);

        /**
         <summary> 
          loads a vmesh file into a GPU-stored mesh.
         </summary>
        */
        IRenderMesh LoadMesh(string VMFPath);

        void DeleteMesh(IRenderMesh mesh);
        //textures

        /**
        <summary>
        loads a texture into the GPU.
        Supports png, jpg, jpeg, qoi, vqoi
        </summary>
        */
        IRenderTexture LoadTexture(string filePath);

        /**
        <summary>
        loads a texture into the GPU
        </summary>
        */
        IRenderTexture LoadTexture(ImageResult image);

        IRenderTexture LoadTexture(float r, float g, float b, float a);

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

        //models

        /**
        <summary>
        loads the mesh and texture from a vmf, vemf, or vbmf model
        </summary>
        */
        RenderEntityModel LoadModel(string folder, string file);

        RenderEntityModel LoadModel(VModel model);

        /**
        <summary>
        deletes the internal mesh and texture of a model.
        </summary>
        */

        void DeleteModel(RenderEntityModel model);
        

        //entities
        
        IRenderEntity SpawnEntity(EntityPosition pos, IRenderShader shader, IRenderMesh mesh, IRenderTexture texture, bool depthTest, IEntityBehavior behavior);
        ///<summary>Waits until the end of the update cycle to spawn an entity </summary>
        IRenderEntity SpawnEntityDelayed(EntityPosition pos, IRenderShader shader, IRenderMesh mesh, IRenderTexture texture, bool depthTest, IEntityBehavior behavior);

        //text entities. A normal entity, but it has text mesh generation built-in.
        IRenderTextEntity SpawnTextEntity(EntityPosition pos, string text, bool centerX, bool centerY, IRenderShader shader, IRenderTexture texture, bool depthTest, IEntityBehavior behavior);
        ///<summary>Waits until the end of the update cycle to spawn an entity </summary>
        IRenderTextEntity SpawnTextEntityDelayed(EntityPosition pos, string text, bool centerX, bool centerY, IRenderShader shader, IRenderTexture texture, bool depthTest, IEntityBehavior behavior);

        //Entities are deleted using the same method as normal entities
        /**
        <summary>
        Deletes an entity.
        Note that this can be used to delete both normal and text entities.
        </summary>
        */
        void DeleteEntity(IRenderEntity entity);
        /**
        <summary>
        Waits until the end of the update cycle to delete the entity.
        Note that this can be used to delete both normal and text entities.
        </summary>
        */
        void DeleteEntityDelayed(IRenderEntity entity);

        /**
        <summary>
        Returns the list of entities.
        Note that there WILL be null elements. If an entity is 'null', it means that it has been removed.
        </summary>
        */
        IEnumerable<IRenderEntity> GetEntities();
        //camera

        RenderCamera SpawnCamera(Vector3 position, Vector3 rotation, float fovy);

        void SetCamera(RenderCamera camera);
        RenderCamera GetCamera();
        void DeleteCamera(RenderCamera camera);

        //input
        KeyboardState Keyboard();

        MouseState Mouse();

        bool CursorLocked{get; set;}
    }
}