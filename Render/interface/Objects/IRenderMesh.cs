using libvmodel;

namespace Render{
    public interface IRenderMesh: IRenderObject{
        int ElementCount();
        void ReData(float[] vertices, uint[] indices);
        void ReData(VMesh mesh);
        void AddData(float[] vertices, uint[] indices);
        void AddData(VMesh mesh);
    }
}