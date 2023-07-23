namespace Common.Core
{
    public interface IBufferOwner<T>
    {
        void Return(ArrayBuffer<T> buffer);
    }
}