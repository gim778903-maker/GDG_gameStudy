using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yeeun.SymbolPuzzle;

namespace Yeeun.SymbolPuzzle.EditorTools
{
    /// <summary>
    /// Generates one mesh asset per <see cref="SymbolShape"/> plus the
    /// <see cref="SymbolMeshLibrary"/> that maps them. Re-running updates the meshes
    /// in place, so their GUIDs (and every prefab reference) stay the same.
    /// </summary>
    public static class SymbolMeshGenerator
    {
        public const string MeshFolder = "Assets/Meshes/Yeeun/SymbolPuzzle";
        public const string LibraryPath = MeshFolder + "/SymbolMeshLibrary.asset";
        const float Depth = 0.1f;

        [MenuItem("Tools/Yeeun/Symbol Puzzle/Regenerate Symbol Meshes")]
        public static void RegenerateMenu()
        {
            var library = Generate();
            EditorGUIUtility.PingObject(library);
            Debug.Log($"[SymbolPuzzle] Symbol meshes regenerated in {MeshFolder}");
        }

        public static SymbolMeshLibrary Generate()
        {
            EnsureFolder(MeshFolder);
            var entries = new List<SymbolMeshLibrary.Entry>();

            foreach (SymbolShape shape in Enum.GetValues(typeof(SymbolShape)))
            {
                string path = $"{MeshFolder}/Symbol_{shape}.asset";
                Mesh built = SymbolMeshBuilder.Build(shape, Depth);
                Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(built, path);
                    existing = built;
                }
                else
                {
                    existing.Clear();
                    existing.SetVertices(built.vertices);
                    existing.SetNormals(built.normals);
                    existing.SetUVs(0, built.uv);
                    existing.SetTriangles(built.triangles, 0);
                    existing.RecalculateBounds();
                    existing.RecalculateTangents();
                    EditorUtility.SetDirty(existing);
                    UnityEngine.Object.DestroyImmediate(built);
                }
                entries.Add(new SymbolMeshLibrary.Entry { shape = shape, mesh = existing });
            }

            var library = AssetDatabase.LoadAssetAtPath<SymbolMeshLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<SymbolMeshLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }
            library.entries = entries.ToArray();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
