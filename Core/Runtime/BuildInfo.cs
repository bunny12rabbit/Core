using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Common
{
    [Serializable]
    public class BuildInfo
    {
        private const string VersionPrefsKey = "Version";

        [SerializeField]
        private long _buildTimestamp;
        public DateTime BuildTime
        {
            get => DateTime.FromFileTimeUtc(_buildTimestamp);
            set => _buildTimestamp = value.ToFileTimeUtc();
        }

        public string BuildNumber;
        public string BranchName;
        public string CommitHash;

        [SerializeField]
        private int _version;
        public int Version
        {
            get
            {
#if !PRODUCTION_BUILD
                if (PlayerPrefs.HasKey(VersionPrefsKey))
                    return PlayerPrefs.GetInt(VersionPrefsKey, -1);
#endif

                return _version;
            }

            set => _version = value;
        }

        public string Platform
        {
            get
            {
#if UNITY_IOS
                return "ios";
#elif (UNITY_ANDROID)
				return "android";
#elif (UNITY_STANDALONE_WIN)
                return "win";
#elif (UNITY_STANDALONE_OSX)
                return "osx";
#elif (UNITY_STANDALONE_LINUX)
                return "linux";
#endif
            }
        }

        /// <summary>
        /// Влияет на включение в билд отладочных символов, доступность профайлера, различные оптимизации.
        /// </summary>
        public bool IsDevelopmentBuild;

        /// <summary>
        /// Влияет на доступность дебажной панели, читов и т.д.
        /// </summary>
        public bool IsProductionBuild;
    }
}