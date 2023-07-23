using UnityEngine.Networking;

namespace Common.Core
{
    public static class UnityWebRequestExtensions
    {
        public static bool IsFailed(this UnityWebRequest unityWebRequest)
        {
            switch (unityWebRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    return true;
            }

            return false;
        }
    }
}
