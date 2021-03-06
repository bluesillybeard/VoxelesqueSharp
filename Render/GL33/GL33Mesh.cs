using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using libvmodel;


namespace Render.GL33{

    struct GL33MeshHandle{

        public GL33MeshHandle(int indexBuffer, int vertexBufferObject, int vertexArrayObject){
            this.indexBuffer = indexBuffer;
            this.vertexBufferObject = vertexBufferObject;
            this.vertexArrayObject = vertexArrayObject;
        }
        public int indexBuffer;

        public int vertexBufferObject;
        public int vertexArrayObject;
    }

    class GL33Mesh: GL33Object, IRenderMesh, IDisposable{
        bool _deleted;
        private int _indexBuffer;

        private int _vertexBufferObject;

        private int _indexCount;

        private int _vertexCount;

        public GL33Mesh(string vmeshPath){
            VMesh mesh = new VMesh(vmeshPath, out ICollection<string> err);
            LoadMesh(mesh.vertices, mesh.indices, false);
            if(err != null){
                RenderUtils.PrintErrLn(string.Join("\n\n", err));
            }
        }
        public GL33Mesh(VMesh mesh){
            LoadMesh(mesh.vertices, mesh.indices, false);
        }
        /**
        <summary>
            Creates a Mesh from an element array
            Each vertec has 8 elements: X pos, y pos, z pos, x tex coord, y tex coord, x normal, y normal, z normal.
            the X Y Z coordinates should be obvious.
            the X Y tex coords are the X and Y texture coordinates, also often refered to as UV
            the X Y Z normals form a cartesian vector of the surface normal.
        </summary>
        */
        public GL33Mesh(float[] vertices, uint[] indices){
            LoadMesh(vertices, indices, false);
        }

        /**
        <summary>
            Creates a Mesh from an element array
            Each vertec has 8 elements: X pos, y pos, z pos, x tex coord, y tex coord, x normal, y normal, z normal.
            the X Y Z coordinates should be obvious.
            the X Y tex coords are the X and Y texture coordinates, also often refered to as UV
            the X Y Z normals form a cartesian vector of the surface normal.
        </summary>
        */
        public GL33Mesh(float[] vertices, uint[] indices, bool dynamic){
            LoadMesh(vertices, indices, dynamic);
        }
        public void ReData(VMesh mesh){
            ReData(mesh.vertices, mesh.indices);
        }
        
        public void ReData(float[] vertices, uint[] indices){
            _indexCount = indices.Length;
            _vertexCount = vertices.Length;
            GL.BindVertexArray(_id);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }

        public void AddData(VMesh mesh){
            AddData(mesh.vertices, mesh.indices);
        }
        
        //Implementing this function was a huge pain.
        //Mostly since OpenGL doesn't do it for me.
        public unsafe void AddData(float[] vertices, uint[] indices){
            //modify the indices so we can haphazardly add them on.
            for(int i=0; i<indices.Length; i++){
                indices[i] += (uint)_indexCount;
            }
            //The fact that OpenGL has no built-in way to expand the size of a buffer without overriding it is annoying.
            
            //TODO: finish implimenting this surprisingly complicated method.
            GL.BindVertexArray(_id);
            {
                //This is in a separate block to keep the stack from leacking.
                //I'm so tired that leacking doesn't even look like a word anymore.
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                //load the buffer into memory
                float[] bufferVertices = new float[_vertexCount + vertices.Length];
                GL.GetBufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertexCount*sizeof(float), bufferVertices);
                //add the new data
                for(int i=0; i<vertices.Length; i++){
                    bufferVertices[i+_vertexCount] = vertices[i];
                }
                //upload the new buffer.
                GL.BufferData(BufferTarget.ArrayBuffer, ((_vertexCount+vertices.Length)*sizeof(float)), bufferVertices, BufferUsageHint.DynamicDraw);
            }
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
                //load the buffer into memory
                uint[] bufferIndices = new uint[_indexCount + indices.Length];
                GL.GetBufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, _indexCount*sizeof(uint), bufferIndices);
                //add the new data
                for(int i=0; i<indices.Length; i++){
                    bufferIndices[i+_indexCount] = indices[i];
                }
                //upload the new buffer.
                GL.BufferData(BufferTarget.ElementArrayBuffer, ((_indexCount+indices.Length)*sizeof(uint)), bufferIndices, BufferUsageHint.DynamicDraw);
            }
            _indexCount += indices.Length;
            _vertexCount += vertices.Length;
        }

        private void LoadMesh(float[] vertices, uint[] indices, bool dynamic){
            _indexCount = indices.Length;
            _vertexCount = vertices.Length;
            _id = GL.GenVertexArray();
            GL.BindVertexArray(_id);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            if(dynamic)GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            else GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            if(dynamic)GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            else GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

            //coordinates
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            //texture coordinates
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            //surface normals
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
        }
        public void Bind(){
            GL.BindVertexArray(_id);
        }

        public int ElementCount(){
            return _indexCount;
        }

        ~GL33Mesh(){
            //check to see if it's already deleted - if not, it's been leaked and should be taken care of.
            if(!_deleted){
                //add it to the deleted meshes buffer, since the C# garbage collector won't have the OpenGl context.
                //I am aware of the fact this is spaghetti code. I just can't think of a better way to do it.
                //any time this code is used, it can be safely cast to a GL33 object, since only GL33Objects can be created with a GL33Render.
                List<GL33MeshHandle> deletedMeshes = ((GL33Render)IRender.CurrentRender)._deletedMeshes;
                lock(deletedMeshes)
                    deletedMeshes.Add(new GL33MeshHandle(_indexBuffer, _vertexBufferObject, _id));
            }
        }

        //dispose of a garbage-collected mesh
        public static void Dispose(GL33MeshHandle mesh){
            GL.DeleteBuffer(mesh.vertexBufferObject);
            GL.DeleteBuffer(mesh.indexBuffer);

            GL.DeleteVertexArray(mesh.vertexArrayObject);
        }

        public void Dispose(){
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_indexBuffer);

            GL.DeleteVertexArray(_id);
            _deleted = true;
        }
    }
}