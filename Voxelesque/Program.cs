using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Render;
using Render.GL33;

using System;

using libvmodel;
namespace Voxelesque.Game
{
    public static class Program
    {

        static double time;
        static IRender render;
        static RenderEntityModel model;
        static VMesh cpuMesh;
        static IRenderShader shader;
        static IRenderShader cameralessShader;
        static IRenderTextEntity debugText;

        static IRenderTexture grass;
        static IRenderTexture ascii;
        static Random random;

        static RemoveOnTouchBehavior GrassCubeBehavior;

        static RenderCamera camera;
        private static void Main()
        {
            //Goes without saying, but the Main method here is an absolutely atrocious mess.
            //I'll fix it someday, but for now it stays.
            //TODO: fix horrible mess of a Main method
            System.Threading.Thread.CurrentThread.Name = "Main Thread";

            
            random = new Random((int)DateTime.Now.Ticks);

            render = RenderUtils.CreateIdealRender();

            render.Init(new RenderSettings()); //todo: use something other than the default settings

            render.OnUpdate += new System.Action<double>(update); //subscribe the the update event

            //initial loading stuff here - move to update method and make asynchronous when loading bar is added

            VModel cpuModel = new VModel("Resources/vmf/models", "GrassCube.vmf", out var ignored, out var errors);
            cpuMesh = cpuModel.mesh;

            if(errors != null)RenderUtils.PrintErrLn(string.Join("/n", errors));
            model = render.LoadModel(cpuModel);
            shader = render.LoadShader("Resources/Shaders/");
            cameralessShader = render.LoadShader("Resources/Shaders/cameraless");
            render.SpawnEntity(new EntityPosition(
                Vector3.Zero,
                Vector3.Zero,
                Vector3.One
            ), shader, model.mesh, model.texture, true, null);
            ascii = render.LoadTexture("Resources/ASCII-Extended.png");
            debugText = render.SpawnTextEntity(new EntityPosition(-Vector3.UnitX+Vector3.UnitY,Vector3.Zero,Vector3.One/30), "", false, false, cameralessShader, ascii, false, null);
            grass = model.texture;

            camera = render.SpawnCamera(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 90);

            GrassCubeBehavior = new RemoveOnTouchBehavior(cpuMesh);
            render.SetCamera(camera);
            render.Run();
        }
        static void update(double d){
            time += d;
            debugText.Text = "Entities: " + render.EntityCount() + "\n"
             + "Camera Position: " + camera.Position + "\n"
             + "Camera Rotation: " + camera.Rotation;
            KeyboardState keyboard = render.Keyboard();
            MouseState mouse = render.Mouse();
            //between -1 and 1
            if(keyboard.IsKeyDown(Keys.F)){
                render.SpawnEntity(new EntityPosition(camera.Position - Vector3.UnitY, new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle()), Vector3.One), shader, model.mesh, model.texture, true, GrassCubeBehavior); 
            }
            
            if (keyboard.IsKeyReleased(Keys.C))
            {
                render.CursorLocked  = !render.CursorLocked;
            }

            Vector3 cameraInc = new Vector3();
            if (keyboard.IsKeyDown(Keys.W)) {
                cameraInc.Z = -1;
            } else if (keyboard.IsKeyDown(Keys.S)) {
                cameraInc.Z = 1;
            }
            if (keyboard.IsKeyDown(Keys.A)) {
                cameraInc.X = -1;
            } else if (keyboard.IsKeyDown(Keys.D)) {
                cameraInc.X = 1;
            }
            if (keyboard.IsKeyDown(Keys.LeftControl)) {
                cameraInc.Y = -1;
            } else if (keyboard.IsKeyDown(Keys.Space)) {
                cameraInc.Y = 1;
            }
            // Update camera position
            float cameraSpeed = 1f / 6f;
            if(keyboard.IsKeyDown(Keys.LeftShift)) cameraSpeed = 1f;

            camera.Move(cameraInc * cameraSpeed);

            // Update camera baseda on mouse
            float sensitivity = 0.5f;

            if (render.CursorLocked || mouse.IsButtonDown(MouseButton.Right)) {
                camera.Rotation += new Vector3((mouse.Y - mouse.PreviousY) * sensitivity, (mouse.X - mouse.PreviousX) * sensitivity, 0);
            }
        }
    }
}
