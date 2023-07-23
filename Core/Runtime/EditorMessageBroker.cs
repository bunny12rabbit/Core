#if UNITY_EDITOR

using UniRx;

namespace Common.Core
{
    /// <summary>
    /// Шина событий для редакторских целей.
    /// </summary>
    public class EditorMessageBroker
    {
        public static readonly IMessageBroker Instance = new MessageBroker();
    }
}

#endif