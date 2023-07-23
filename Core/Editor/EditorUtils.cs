namespace Common.Core.Editor
{
    public static class EditorUtils
        {
            public static bool ContainsDefine(string define)
            {
                var defineString = GetDefineString();
                return defineString.Contains(define);
            }

            public static void AddNewDefine(string newDefine)
            {
                var defineString = GetDefineString();

                if (ContainsDefine(newDefine))
                    return;

                var newDefineString = $"{defineString};{newDefine}";
#if UNITY_ANDROID
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android, newDefineString);
#elif UNITY_IOS
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.iOS, newDefineString);
#endif
            }

            public static void RemoveDefine(string defineToRemove)
            {
                var defineString = GetDefineString();
                var defineArray = defineString.Split(';');
                var newDefineString = string.Empty;

                foreach (var define in defineArray)
                {
                    if (!define.Equals(defineToRemove))
                    {
                        newDefineString = newDefineString.Equals(string.Empty)
                            ? $"{define};"
                            : $"{newDefineString}{define};";
                    }
                }

#if UNITY_ANDROID
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android, newDefineString);
#elif UNITY_IOS
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.iOS, newDefineString);
#endif
            }

            private static string GetDefineString()
            {
#if UNITY_ANDROID
                return UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android);
#elif UNITY_IOS
                return UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.iOS);
#else
                return string.Empty;
#endif
            }
        }
}