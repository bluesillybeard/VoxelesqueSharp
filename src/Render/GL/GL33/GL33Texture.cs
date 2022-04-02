using OpenTK.Graphics.OpenGL;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using System.IO;

using VQoiSharp;
using VQoiSharp.Codec;

//NOTICE: this is a modified version of the texture class from the official OpenTK examples.

namespace Voxelesque.Render.GL33{

    struct GL33TextureHandle{
        public GL33TextureHandle(int id){
            this.id = id;
        }
        public int id;
    }
    
    class GL33Texture: GL33Object, IRenderTexture, IDisposable
    {
        private bool _deleted;
        public GL33Texture(string path)
        {
            string lowerPath = path.ToLower();
            if(lowerPath.EndsWith(".vqoi") || lowerPath.EndsWith(".qoi")){
                VQoiImage image = VQoiDecoder.Decode(File.ReadAllBytes(path));
                LoadTexture(image);
            } else {
                using(Bitmap image = new Bitmap(path)){
                    LoadTexture(image);
                }
            }
        }

        public GL33Texture(Bitmap image){
            LoadTexture(image);
        }

        private void LoadTexture(Bitmap image){
            // Generate handle
            _id = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _id);

            //EDIT: this USED to be the case, HOWEVER the fragment shader *should* invert the Y texture coordinate, thus fixing the issue.
            // Our Bitmap loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            // This will correct that, making the texture display properly.
            //image.RotateFlip(RotateFlipType.RotateNoneFlipY);



            // First, we get our pixels from the bitmap we loaded.
            // Arguments:
            //   The pixel area we want. Typically, you want to leave it as (0,0) to (width,height), but you can
            //   use other rectangles to get segments of textures, useful for things such as spritesheets.
            //   The locking mode. Basically, how you want to use the pixels. Since we're passing them to OpenGL,
            //   we only need ReadOnly.
            //   Next is the pixel format we want our pixels to be in. In this case, ARGB will suffice.
            //   We have to fully qualify the name because OpenTK also has an enum named PixelFormat.
            BitmapData data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Now that our pixels are prepared, it's time to generate a texture. We do this with GL.TexImage2D.
            // Arguments:
            //   The type of texture we're generating. There are various different types of textures, but the only one we need right now is Texture2D.
            //   Level of detail. We can use this to start from a smaller mipmap (if we want), but we don't need to do that, so leave it at 0.
            //   Target format of the pixels. This is the format OpenGL will store our image with.
            //   Width of the image
            //   Height of the image.
            //   Border of the image. This must always be 0; it's a legacy parameter that Khronos never got rid of.
            //   The format of the pixels, explained above. Since we loaded the pixels as ARGB earlier, we need to use BGRA.
            //   Data type of the pixels.
            //   And finally, the actual pixels.
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            SetParametersAndGenerateMipmaps();
        }

        private void LoadTexture(VQoiImage image){
            _id = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _id);

            //determine what format to use.
            PixelFormat format;
            switch (image.Channels){
                case VQoiChannels.Rgb: {
                    format = PixelFormat.Rgb;
                    break;
                }
                case VQoiChannels.RgbWithAlpha: {
                    format = PixelFormat.Rgba;
                    break;
                }
                default: throw new Exception("invalid QOI image type - only RGB and RGBA are supported.");
            }

            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                format,
                PixelType.UnsignedByte,
                image.Data);

            SetParametersAndGenerateMipmaps();          
        }

        private void SetParametersAndGenerateMipmaps(){
            // Now that our texture is loaded, we can set a few settings to affect how the image appears on rendering.

            // First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
            // Here, we use Linear for both. This means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
            // You could also use (amongst other options) Nearest, which just grabs the nearest pixel, which makes the texture look pixelated if scaled too far.
            // NOTE: The default settings for both of these are LinearMipmap. If you leave these as default but don't generate mipmaps,
            // your image will fail to render at all (usually resulting in pure black instead).
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
            // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Next, generate mipmaps.
            // Mipmaps are smaller copies of the texture, scaled down. Each mipmap level is half the size of the previous one
            // Generated mipmaps go all the way down to just one pixel.
            // OpenGL will automatically switch between mipmaps when an object gets sufficiently far away.
            // This prevents moiré effects, as well as saving on texture bandwidth.
            // Here you can see and read about the morié effect https://en.wikipedia.org/wiki/Moir%C3%A9_pattern
            // Here is an example of mips in action https://en.wikipedia.org/wiki/File:Mipmap_Aliasing_Comparison.png
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }


        // Activate texture
        // Multiple textures can be bound, if your shader needs more than just one.
        // If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
        // The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, _id);
        }

        public void Dispose(){
            GL.DeleteTexture(this._id);
            this._deleted = true;
        }

        public static void Dispose(GL33TextureHandle texture){
            GL.DeleteTexture(texture.id);
        }

        ~GL33Texture(){
            //check to see if it's already deleted - if not, it's been leaked and should be taken care of.
            if(!_deleted){
                //add it to the deleted textures buffer, since the C# garbage collector won't have the OpenGl context.
                //I am aware of the fact this is spaghetti code. I just can't think of a better way to do it.
                //any time this code is used, it can be safely cast to a GL33 object, since only GL33Objects can be created with a GL33Render.
                List<GL33TextureHandle> deletedTextures = ((GL33Render)RenderUtils.CurrentRender)._deletedTextures;
                lock(deletedTextures)
                    deletedTextures.Add(new GL33TextureHandle(_id));
            }
        }
    }
}