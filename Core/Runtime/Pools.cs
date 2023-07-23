using System.Text;
using UnityEngine.Pool;

namespace Common.Core
{
    public static class StringBuilderPool
    {
        private static readonly ObjectPool<StringBuilder> s_pool = new(
            () => new StringBuilder(),
            sb => sb.Clear());

        public static PooledObject<StringBuilder> Get(out StringBuilder stringBuilder) => s_pool.Get(out stringBuilder);

        public static void Release(StringBuilder stringBuilder) => s_pool.Release(stringBuilder);
    }
}