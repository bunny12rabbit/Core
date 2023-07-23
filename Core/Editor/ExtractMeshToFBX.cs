using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Fbx;
using Common.Core;
using Common.Core.Logs;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Common.Core.Editor
{
    public static class ExtractMeshToFBX
    {
        public readonly struct Params
        {
            public readonly Mesh Mesh;

            public readonly Material[] Materials;

            public readonly string Path;

            public readonly string MeshName;

            public readonly bool SaveFbxAsAscii;

            public readonly FbxAxisSystem FBXAxisSystem;

            public readonly FbxSystemUnit FBXUnit;

            public readonly string FBXFileTitle;
            public readonly string FBXFileSubject;
            public readonly string FBXFileComment;
            public readonly string FBXFileKeywords;
            public readonly string FBXFileAuthor;
            public readonly string FBXFileRevision;
            public readonly string FBXFileApplication;

            /// <param name="mesh">Mesh to convert</param>
            /// <param name="path">Path to resulting FBX (relative, including "Assets/")</param>
            /// <param name="materials">Materials from MeshRenderer, that used by Mesh</param>
            /// <param name="meshName">Name for mesh</param>
            /// <param name="saveFbxAsAscii">true: fbx file is easy-to-debug ascii, false: fbx file is binary.</param>
            /// <param name="fbxAxisSystem">The preferred axis system for the exported fbx file</param>
            /// <param name="fbxUnit">The preferred units of the exported fbx file</param>
            /// <param name="fbxFileTitle"></param>
            /// <param name="fbxFileSubject"></param>
            /// <param name="fbxFileComment"></param>
            /// <param name="fbxFileKeywords"></param>
            /// <param name="fbxFileAuthor"></param>
            public Params(Mesh mesh, string path, Material[] materials, string meshName, FbxAxisSystem fbxAxisSystem, FbxSystemUnit fbxUnit,
                bool saveFbxAsAscii = false, string fbxFileTitle = "", string fbxFileSubject = "", string fbxFileComment = "",
                string fbxFileKeywords = "", string fbxFileAuthor = "")
            {
                Mesh = mesh;
                Materials = materials;
                Path = path;
                MeshName = meshName;
                FBXAxisSystem = fbxAxisSystem;
                FBXUnit = fbxUnit;
                SaveFbxAsAscii = saveFbxAsAscii;
                FBXFileTitle = fbxFileTitle;
                FBXFileSubject = fbxFileSubject;
                FBXFileComment = fbxFileComment;
                FBXFileKeywords = fbxFileKeywords;
                FBXFileAuthor = fbxFileAuthor;
                FBXFileRevision = "1.0";
                FBXFileApplication = "Unity FBX SDK";
            }

            /// <param name="mesh">Mesh to convert</param>
            /// <param name="path">Path to resulting FBX (relative, including "Assets/")</param>
            /// <param name="materials">Materials from MeshRenderer, that used by Mesh</param>
            /// <param name="meshName">Name for mesh</param>
            /// <param name="saveFbxAsAscii">true: fbx file is easy-to-debug ascii, false: fbx file is binary.</param>
            /// <param name="fbxFileTitle"></param>
            /// <param name="fbxFileSubject"></param>
            /// <param name="fbxFileComment"></param>
            /// <param name="fbxFileKeywords"></param>
            /// <param name="fbxFileAuthor"></param>
            public Params(Mesh mesh, string path, Material[] materials = null, string meshName = "", bool saveFbxAsAscii = false,
                string fbxFileTitle = "", string fbxFileSubject = "", string fbxFileComment = "", string fbxFileKeywords = "",
                string fbxFileAuthor = "")
            {
                Mesh = mesh;
                Materials = materials;
                Path = path;
                MeshName = meshName;
                SaveFbxAsAscii = saveFbxAsAscii;
                FBXAxisSystem = FbxAxisSystem.Max;
                FBXUnit = FbxSystemUnit.m;

                FBXFileTitle = fbxFileTitle;
                FBXFileSubject = fbxFileSubject;
                FBXFileComment = fbxFileComment;
                FBXFileKeywords = fbxFileKeywords;
                FBXFileAuthor = fbxFileAuthor;
                FBXFileRevision = "1.0";
                FBXFileApplication = "Unity FBX SDK";
            }
        }

        public static Mesh ExtractSubMeshFromMesh(Mesh mesh, int meshIndex)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;

            var newVerts = new List<Vector3>();
            var newNorms = new List<Vector3>();
            var newTris = new List<int>();
            var triangles = mesh.GetTriangles(meshIndex);

            for (var i = 0; i < triangles.Length; i += 3)
            {
                var a = triangles[i + 0];
                var b = triangles[i + 1];
                var c = triangles[i + 2];
                newVerts.Add(vertices[a]);
                newVerts.Add(vertices[b]);
                newVerts.Add(vertices[c]);
                newNorms.Add(normals[a]);
                newNorms.Add(normals[b]);
                newNorms.Add(normals[c]);
                newTris.Add(newTris.Count);
                newTris.Add(newTris.Count);
                newTris.Add(newTris.Count);
            }

            var subMesh = new Mesh
            {
                indexFormat = newVerts.Count > MeshCombiner.Mesh16BitBufferVertexLimit ? IndexFormat.UInt32 : IndexFormat.UInt16
            };

            subMesh.SetVertices(newVerts);
            subMesh.SetNormals(newNorms);
            subMesh.SetTriangles(newTris, 0, true);
            return subMesh;
        }

        private static string CreateFilePath(string fileName)
        {
            var pathWithoutExtension = $"Assets/ExportedFBX/{fileName}";
            var filePath = $"{pathWithoutExtension}.fbx";

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? $"{Application.dataPath}/CombinedMeshes/");

            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            return filePath;
        }

        [MenuItem("CONTEXT/MeshFilter/Extract to FBX")]
        private static void ContextExtractToFBX(MenuCommand command)
        {
            var meshFilter = (MeshFilter) command.context;
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            var sharedMesh = meshFilter.sharedMesh;

            if (sharedMesh == null)
                Log.Error($"Object :{meshFilter.transform.GetPath()}: doesn't contains MeshRenderer!");

            ExtractToFBX(new Params(sharedMesh, CreateFilePath(sharedMesh.name), meshRenderer.sharedMaterials));
        }

        [MenuItem("Assets/Extract to FBX", validate = true)]
        private static bool MenuExtractToFBXValidate()
        {
            if (Selection.activeObject == null)
                return false;

            return Selection.activeObject is Mesh;
        }

        [MenuItem("Assets/Extract to FBX")]
        private static void MenuExtractToFBX()
        {
            // We assume validation worked and this is always defined.
            Mesh mesh = Selection.activeObject as Mesh;

            // Set up paths
            var meshFilePath = AssetDatabase.GetAssetPath(mesh);
            var meshDirectory = Path.GetDirectoryName(meshFilePath);

            var filename = Path.GetFileNameWithoutExtension(meshFilePath) + ".fbx";
            var filePath = Path.Combine(meshDirectory, filename);

            ExtractToFBX(new Params(mesh, filePath));
        }

        public static void ExtractToFBX(Params @params)
        {
            // Make a temporary copy of the mesh to modify it
            var tempMesh = Object.Instantiate(@params.Mesh);
            tempMesh.name = @params.MeshName.Equals("") ? @params.Mesh.name : @params.MeshName;

            FixMeshRotation(@params, tempMesh);

            var fbxExporter = CreateFbxExporter(@params, out var fbxScene);

            AddMeshToScene(fbxScene, tempMesh, @params.Materials);

            // Finally actually save the scene
            var sceneSuccess = fbxExporter.Export(fbxScene);
            AssetDatabase.Refresh();

            // clean up temporary model
            if (Application.isPlaying)
                Object.Destroy(tempMesh);
            else
                Object.DestroyImmediate(tempMesh);
        }

        private static void FixMeshRotation(Params @params, Mesh tempMesh)
        {
            Vector3[] vertices = tempMesh.vertices;

            for (var i = 0; i < vertices.Length; i++)
            {
                //We fix the vertices by flipping Z axis with Y axis
                //Odly enough X has to be inverted. When we store a positive X in Blender somehow it gets inverted in the *.fbx format O.o
                var vertex = vertices[i];
                vertex = new Vector3(-vertex.x, vertex.y, vertex.z);

                // If meters, divide by 100 since default is cm. Assume centered at origin.
                if (@params.FBXUnit == FbxSystemUnit.m)
                    vertex *= 0.01f;

                vertices[i] = vertex;
            }

            tempMesh.vertices = vertices;

            //Vertex positions have changed, so recalc bounds
            tempMesh.RecalculateBounds();

            for (var i = 0; i < tempMesh.subMeshCount; i++)
            {
                var triangles = tempMesh.GetTriangles(i);

                for (var j = 0; j < triangles.Length; j += 3)
                {
                    var b = triangles[j + 1];
                    var c = triangles[j + 2];

                    triangles[j + 1] = c;
                    triangles[j + 2] = b;
                }

                tempMesh.SetTriangles(triangles, i);
            }

            //Same goes for normals
            Vector3[] normals = tempMesh.normals;

            for (var i = 0; i < normals.Length; i++)
            {
                var normal = normals[i];
                normal = new Vector3(-normal.x, normal.y, normal.z);
                normals[i] = normal;
            }

            tempMesh.normals = normals;
        }

        private static FbxExporter CreateFbxExporter(Params @params, out FbxScene fbxScene)
        {
            // FBX Manager
            FbxManager manager = FbxManager.Create();
            manager.SetIOSettings(FbxIOSettings.Create(manager, Globals.IOSROOT));

            // FBX Exporter
            FbxExporter fbxExporter = FbxExporter.Create(manager, "Exporter");

            // Binary
            var fileFormat = -1;

            // Ascii
            if (@params.SaveFbxAsAscii)
                fileFormat = manager.GetIOPluginRegistry().FindWriterIDByDescription("FBX ascii (*.fbx)");

            fbxExporter.Initialize(@params.Path, fileFormat, manager.GetIOSettings());
            fbxExporter.SetFileExportVersion("FBX201400");

            // FBX Scene
            fbxScene = FbxScene.Create(manager, "Scene");
            FbxDocumentInfo sceneInfo = FbxDocumentInfo.Create(manager, "SceneInfo");

            // Set up scene info
            sceneInfo.mTitle = @params.FBXFileTitle;
            sceneInfo.mSubject = @params.FBXFileSubject;
            sceneInfo.mComment = @params.FBXFileComment;
            sceneInfo.mAuthor = @params.FBXFileAuthor;
            sceneInfo.mRevision = @params.FBXFileRevision;
            sceneInfo.mKeywords = @params.FBXFileKeywords;
            sceneInfo.Original_ApplicationName.Set(@params.FBXFileApplication);
            sceneInfo.LastSaved_ApplicationName.Set(@params.FBXFileApplication);
            fbxScene.SetSceneInfo(sceneInfo);

            // Set up Global settings
            FbxGlobalSettings globalSettings = fbxScene.GetGlobalSettings();
            globalSettings.SetSystemUnit(@params.FBXUnit);
            globalSettings.SetAxisSystem(@params.FBXAxisSystem);
            return fbxExporter;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void AddMeshToScene(FbxScene fbxScene, Mesh mesh, Material[] materials)
        {
            FbxNode modelNode = FbxNode.Create(fbxScene, mesh.name);

            // Add mesh to a node in the scene
            using (ModelExporter modelExporter = new ModelExporter())
            {
                var exportMeshMethod = modelExporter
                    .GetType()
                    .GetMethod(
                        "ExportMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                        Type.DefaultBinder,
                        new[] {typeof(Mesh), typeof(FbxNode), typeof(Material[])},
                        null);

                var result = (bool) (exportMeshMethod?.Invoke(modelExporter, new object[] {mesh, modelNode, materials}) ?? false);

                if (!result)
                    Debug.LogError("Problem Exporting Mesh");
            }

            // add the model to the scene
            fbxScene.GetRootNode().AddChild(modelNode);
        }
    }
}