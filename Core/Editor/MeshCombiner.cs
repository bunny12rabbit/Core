using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Core;
using Common.Core.Logs;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Common.Core.Editor
{
    public static class MeshCombiner
    {
        public const int Mesh16BitBufferVertexLimit = 65535;

        public readonly struct Params
        {
            public readonly Transform ParentForContainer;

            public readonly string CombinedMeshContainerName;

            public readonly bool DestroyInsteadOfDisablingMeshes;
            public readonly bool MarkCreatedAsStatic;

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public readonly bool ShowCreatedMeshInfo;

            /// <summary>
            /// It is a slow operation that generates a UV map (required for the lightmap). Can be used only in the Editor.
            /// </summary>
            public readonly bool GenerateUvMap;
            public readonly bool IgnoreDisabledMeshes;

            public readonly string PathToSaveMeshes;

            public readonly GameObject[] ObjectsToCombine;

            public Params(Transform parentForContainer, string combinedMeshContainerName, bool destroyInsteadOfDisablingMeshes,
                bool markCreatedAsStatic, bool showCreatedMeshInfo, bool generateUvMap, string pathToSaveMeshes, bool ignoreDisabledMeshes,
                params GameObject[] objectsToCombine)
            {
                ParentForContainer = parentForContainer;
                CombinedMeshContainerName = combinedMeshContainerName;
                DestroyInsteadOfDisablingMeshes = destroyInsteadOfDisablingMeshes;
                MarkCreatedAsStatic = markCreatedAsStatic;
                ShowCreatedMeshInfo = showCreatedMeshInfo;
                GenerateUvMap = generateUvMap;
                PathToSaveMeshes = pathToSaveMeshes;
                IgnoreDisabledMeshes = ignoreDisabledMeshes;
                ObjectsToCombine = objectsToCombine;
            }
        }

        private readonly struct InitialTransformParams
        {
            private readonly Quaternion _rotation;

            private readonly Vector3 _position;
            private readonly Vector3 _localScale;
            private readonly Vector3 _oldScaleAsChild;

            private readonly int _positionInParentHierarchy;

            private readonly Transform _parent;

            public InitialTransformParams(Quaternion oldRotation, Vector3 oldPosition, Vector3 oldScale, Transform parent,
                int positionInParentHierarchy, Vector3 oldScaleAsChild)
            {
                _rotation = oldRotation;
                _position = oldPosition;
                _localScale = oldScale;
                _parent = parent;
                _positionInParentHierarchy = positionInParentHierarchy;
                _oldScaleAsChild = oldScaleAsChild;
            }

            public void SetParametersBackToInitialOn(Transform transform)
            {
                transform.rotation = _rotation;
                transform.position = _position;
                transform.localScale = _localScale;
                transform.parent = _parent;
                transform.SetSiblingIndex(_positionInParentHierarchy);
                transform.localScale = _oldScaleAsChild;
            }
        }

        private readonly struct InitialObjectsData
        {
            private readonly InitialTransformParams _containerData;

            private readonly Dictionary<Transform, InitialTransformParams> _objectsDataMap;

            public InitialObjectsData(InitialTransformParams containerData, Dictionary<Transform, InitialTransformParams> objectsDataMap)
            {
                _containerData = containerData;
                _objectsDataMap = objectsDataMap;
            }

            public void ResetToInitialState(Transform container, IEnumerable<GameObject> objects)
            {
                _containerData.SetParametersBackToInitialOn(container);

                foreach (var gameObject in objects)
                {
                    var transform = gameObject.transform;

                    if (_objectsDataMap.TryGetValue(transform, out var data))
                        data.SetParametersBackToInitialOn(transform);
                }
            }
        }

        private readonly struct CombinedMeshContainerInfo
        {
            public readonly GameObject Container;

            public readonly MeshFilter MeshFilter;

            public readonly MeshRenderer MeshRenderer;

            public CombinedMeshContainerInfo(GameObject container, MeshFilter meshFilter, MeshRenderer meshRenderer)
            {
                Container = container;
                MeshFilter = meshFilter;
                MeshRenderer = meshRenderer;
            }
        }

        [CanBeNull]
        public static GameObject CombineMeshes(Params @params)
        {
            var objectsToCombine = @params.ObjectsToCombine.Where(gameObject => gameObject != null);
            var meshFilters = GetMeshFilters(objectsToCombine,@params.IgnoreDisabledMeshes);

            if (meshFilters.Length == 0)
                return null;

            var combinedMeshContainerInfo = CreateCombinedMeshContainer(@params);
            var initialObjectsData = Prepare(combinedMeshContainerInfo.Container, objectsToCombine);

            using (ListPool<Material>.Get(out var uniqueMaterialsList))
            {
                var meshRenderers = GatherData(meshFilters, uniqueMaterialsList);

                using (ListPool<CombineInstance>.Get(out var finalMeshCombineInstances))
                {
                    long verticalLenght = 0;

                    foreach (var uniqueMaterial in uniqueMaterialsList)
                    {
                        using (ListPool<CombineInstance>.Get(out var subMeshCombineInstances))
                        {
                            for (var i = 0; i < meshFilters.Length; i++)
                            {
                                if (meshRenderers[i] == null)
                                    continue;

                                var subMeshMaterials = meshRenderers[i].sharedMaterials;

                                for (var j = 0; j < subMeshMaterials.Length; j++)
                                {
                                    if (subMeshMaterials[j] != uniqueMaterial)
                                        continue;

                                    var combineInstance = new CombineInstance
                                    {
                                        subMeshIndex = j,
                                        mesh = meshFilters[i].sharedMesh,
                                        transform = meshFilters[i].transform.localToWorldMatrix
                                    };

                                    subMeshCombineInstances.Add(combineInstance);
                                    verticalLenght += combineInstance.mesh.vertices.Length;
                                }
                            }

                            var subMesh = new Mesh();

                            if (verticalLenght > Mesh16BitBufferVertexLimit)
                                subMesh.indexFormat = IndexFormat.UInt32;

                            subMesh.CombineMeshes(subMeshCombineInstances.ToArray(), true);

                            var finalCombineInstance = new CombineInstance {subMeshIndex = 0, mesh = subMesh, transform = Matrix4x4.identity};

                            finalMeshCombineInstances.Add(finalCombineInstance);
                        }
                    }

                    combinedMeshContainerInfo.MeshRenderer.sharedMaterials = uniqueMaterialsList.ToArray();

                    var combinedMesh = new Mesh {name = @params.CombinedMeshContainerName};

                    if (verticalLenght > Mesh16BitBufferVertexLimit)
                        combinedMesh.indexFormat = IndexFormat.UInt32;

                    combinedMesh.CombineMeshes(finalMeshCombineInstances.ToArray(), false);

                    if (@params.GenerateUvMap)
                        GenerateUV(combinedMesh);

                    combinedMesh.Optimize();

                    var fbxPath = ExtractMeshToFBX(@params, combinedMeshContainerInfo, combinedMesh);

                    ProcessCombinedMeshes(meshFilters, @params.DestroyInsteadOfDisablingMeshes);

                    if (@params.ShowCreatedMeshInfo)
                        ShowCreatedMeshInfo(@params.CombinedMeshContainerName, meshFilters.Length, finalMeshCombineInstances.Count,
                            verticalLenght, fbxPath);
                }
            }

            initialObjectsData.ResetToInitialState(combinedMeshContainerInfo.Container.transform, objectsToCombine);

            //ExtractMeshToFBX(@params, combinedMeshContainerInfo);

            Resources.UnloadUnusedAssets();

            return combinedMeshContainerInfo.Container;
        }

        private static MeshFilter[] GetMeshFilters(IEnumerable<GameObject> objectsToCombine, bool ignoreDisabledMeshes)
        {
            using (ListPool<MeshFilter>.Get(out var meshFiltersTmp))
            {
                foreach (var gameObject in objectsToCombine)
                {
                    var childrenMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>(!ignoreDisabledMeshes).Where(filter => filter.sharedMesh != null);

                    if(ignoreDisabledMeshes)
                        childrenMeshFilters = childrenMeshFilters.Where(filter => filter.GetComponent<MeshRenderer>().enabled);

                    meshFiltersTmp.AddRange(childrenMeshFilters);
                }

                return meshFiltersTmp.ToArray();
            }
        }

        private static CombinedMeshContainerInfo CreateCombinedMeshContainer(Params @params)
        {
            var gameObject = new GameObject(@params.CombinedMeshContainerName);
            gameObject.transform.parent = @params.ParentForContainer;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.isStatic = @params.MarkCreatedAsStatic;

            var meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            var meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();

            return new CombinedMeshContainerInfo(gameObject, meshFilter, meshRenderer);
        }

        private static InitialObjectsData Prepare(GameObject container, IEnumerable<GameObject> objectsToCombine)
        {
            var containerTransform = container.transform;
            var objectsDataMap = new Dictionary<Transform, InitialTransformParams>();

            foreach (var gameObject in objectsToCombine)
            {
                if (gameObject == null)
                    continue;

                var gameObjectTransform = gameObject.transform;
                objectsDataMap[gameObjectTransform] = CreateInitialTransformParams(gameObjectTransform, false);

            }

            foreach (var gameObject in objectsToCombine)
            {
                gameObject.transform.parent = containerTransform;
            }

            var containerData = CreateInitialTransformParams(containerTransform, true);

            return new InitialObjectsData(containerData, objectsDataMap);

            InitialTransformParams CreateInitialTransformParams(Transform transform, bool isContainer)
            {
                var oldScaleAsChild = transform.localScale;
                var positionInParentHierarchy = transform.GetSiblingIndex();
                var parent = transform.parent;
                var oldRotation = transform.rotation;
                var oldPosition = transform.position;
                var oldScale = transform.localScale;

                if (isContainer)
                {
                    transform.parent = null;
                    transform.rotation = Quaternion.identity;
                    transform.position = Vector3.zero;
                    transform.localScale = Vector3.one;
                }

                return new InitialTransformParams(oldRotation, oldPosition, oldScale, parent, positionInParentHierarchy, oldScaleAsChild);
            }
        }

        private static MeshRenderer[] GatherData(IReadOnlyList<MeshFilter> meshFilterers, ICollection<Material> uniqueMaterialsList)
        {
            var meshRenderers = new MeshRenderer[meshFilterers.Count];

            for (var i = 0; i < meshFilterers.Count; i++)
            {
                meshRenderers[i] = meshFilterers[i].GetComponent<MeshRenderer>();

                if (meshRenderers[i] == null)
                    continue;

                var materials = meshRenderers[i].sharedMaterials;

                foreach (var material in materials)
                {
                    if (!uniqueMaterialsList.Contains(material))
                        uniqueMaterialsList.Add(material);
                }
            }

            return meshRenderers;
        }


        private static void GenerateUV(Mesh combinedMesh)
        {
#if UNITY_EDITOR
            UnwrapParam unwrapParam = new UnwrapParam();
            UnwrapParam.SetDefaults(out unwrapParam);
            Unwrapping.GenerateSecondaryUVSet(combinedMesh, unwrapParam);
#endif
        }

        private static string ExtractMeshToFBX(Params @params, CombinedMeshContainerInfo combinedMeshContainerInfo, Mesh mesh)
        {
            var pathWithoutExtension = $"{@params.PathToSaveMeshes}/{@params.CombinedMeshContainerName}";
            var path = $"{pathWithoutExtension}.fbx";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Assets/CombinedMeshes/");

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            Editor.ExtractMeshToFBX.ExtractToFBX(
                new ExtractMeshToFBX.Params(mesh, path, combinedMeshContainerInfo.MeshRenderer.sharedMaterials));

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport |
                                  ImportAssetOptions.DontDownloadFromCacheServer);

            var loadedFBX = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var loadedMesh = loadedFBX.GetComponent<MeshFilter>().sharedMesh;

            combinedMeshContainerInfo.MeshFilter.sharedMesh = loadedMesh;

            return path;
        }

        private static void ProcessCombinedMeshes(IReadOnlyList<MeshFilter> meshFilters, bool destroyInsteadOfDisabling)
        {
            for (var i = 0; i < meshFilters.Count; i++)
            {
                var gameObject = meshFilters[i].gameObject;

                if (destroyInsteadOfDisabling)
                {
                    DestroyComponentOn<MeshRenderer>(gameObject);
                    DestroyComponentOn<MeshFilter>(gameObject);
                }
                else
                {
                    var mRenderer = gameObject.GetComponent<MeshRenderer>();

                    if(mRenderer)
                        mRenderer.enabled = false;
                }
            }
        }

        private static void DestroyComponentOn<T>(GameObject gameObject) where T : Component
        {
            var componentToDestroy = gameObject.GetComponent<T>();

            if (componentToDestroy != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(componentToDestroy);
#else
                Object.Destroy(componentToDestroy);
#endif
            }
        }

        private static void ShowCreatedMeshInfo(string name, int childrenMeshesAmount, int subMeshesAmount, long verticesAmount, string fbxPath)
        {
            var message =
                $"{name} was created from {childrenMeshesAmount} children meshes and has {subMeshesAmount} subMeshes, and {verticesAmount} vertices. \n" +
                $"Combined mesh saved to FBX file at {fbxPath}";

            Log.Info(message.Colorize(verticesAmount > Mesh16BitBufferVertexLimit ? Color.red : Color.green));
        }
    }
}