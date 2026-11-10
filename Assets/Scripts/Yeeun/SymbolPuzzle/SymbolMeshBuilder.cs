using System.Collections.Generic;
using UnityEngine;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Builds extruded symbol meshes (flat front face + hard-edged sides) so the
    /// silhouette stays sharp for an outline / edge-detection effect.
    /// The front face points along local +Z; the back face sits at z = 0.
    /// </summary>
    public static class SymbolMeshBuilder
    {
        /// <summary>Outline points (counter-clockwise, fits in a circle of radius 0.5).</summary>
        public static Vector2[] GetOutline(SymbolShape shape)
        {
            switch (shape)
            {
                case SymbolShape.Circle: return Regular(40, 0.5f, 90f);
                case SymbolShape.Triangle: return Regular(3, 0.5f, 90f);
                case SymbolShape.Square: return Regular(4, 0.5f, 45f);
                case SymbolShape.Hexagon: return Regular(6, 0.5f, 90f);
                case SymbolShape.Diamond:
                {
                    var p = Regular(4, 0.5f, 90f);
                    for (int i = 0; i < p.Length; i++) p[i].x *= 0.65f;
                    return p;
                }
                case SymbolShape.Star:
                {
                    var p = new Vector2[10];
                    for (int i = 0; i < 10; i++)
                    {
                        float r = (i % 2 == 0) ? 0.5f : 0.21f;
                        float a = (90f + i * 36f) * Mathf.Deg2Rad;
                        p[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
                    }
                    return p;
                }
                case SymbolShape.Cross:
                {
                    const float o = 0.5f, w = 0.17f;
                    return new[]
                    {
                        new Vector2(w, -w), new Vector2(o, -w), new Vector2(o, w), new Vector2(w, w),
                        new Vector2(w, o), new Vector2(-w, o), new Vector2(-w, w), new Vector2(-o, w),
                        new Vector2(-o, -w), new Vector2(-w, -w), new Vector2(-w, -o), new Vector2(w, -o),
                    };
                }
                case SymbolShape.XMark:
                {
                    // The cross rotated by 45 degrees (thicker arms so it still reads when small).
                    const float o = 0.5f, w = 0.19f;
                    var plus = new[]
                    {
                        new Vector2(w, -w), new Vector2(o, -w), new Vector2(o, w), new Vector2(w, w),
                        new Vector2(w, o), new Vector2(-w, o), new Vector2(-w, w), new Vector2(-o, w),
                        new Vector2(-o, -w), new Vector2(-w, -w), new Vector2(-w, -o), new Vector2(w, -o),
                    };
                    float c = Mathf.Cos(45f * Mathf.Deg2Rad), s = Mathf.Sin(45f * Mathf.Deg2Rad);
                    for (int i = 0; i < plus.Length; i++)
                        plus[i] = new Vector2(plus[i].x * c - plus[i].y * s, plus[i].x * s + plus[i].y * c);
                    return plus;
                }
                default: return Regular(40, 0.5f, 90f);
            }
        }

        /// <summary>
        /// Creates a new mesh for the shape. All supported shapes are star-shaped around
        /// the origin, so the faces are triangulated as a fan from the centre.
        /// </summary>
        public static Mesh Build(SymbolShape shape, float depth = 0.1f)
        {
            Vector2[] outline = GetOutline(shape);
            int n = outline.Length;
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            // Front face (z = depth, normal +Z).
            int frontCenter = verts.Count;
            AddVertex(verts, normals, uvs, new Vector3(0f, 0f, depth), Vector3.forward);
            for (int i = 0; i < n; i++)
                AddVertex(verts, normals, uvs, new Vector3(outline[i].x, outline[i].y, depth), Vector3.forward);
            for (int i = 0; i < n; i++)
                tris.AddRange(new[] { frontCenter, frontCenter + 1 + i, frontCenter + 1 + (i + 1) % n });

            // Back face (z = 0, normal -Z).
            int backCenter = verts.Count;
            AddVertex(verts, normals, uvs, Vector3.zero, Vector3.back);
            for (int i = 0; i < n; i++)
                AddVertex(verts, normals, uvs, new Vector3(outline[i].x, outline[i].y, 0f), Vector3.back);
            for (int i = 0; i < n; i++)
                tris.AddRange(new[] { backCenter, backCenter + 1 + (i + 1) % n, backCenter + 1 + i });

            // Sides: one quad per edge with its own vertices for hard edges.
            for (int i = 0; i < n; i++)
            {
                Vector2 p0 = outline[i];
                Vector2 p1 = outline[(i + 1) % n];
                Vector2 edge = p1 - p0;
                Vector3 normal = new Vector3(edge.y, -edge.x, 0f).normalized;
                int s = verts.Count;
                AddVertex(verts, normals, uvs, new Vector3(p0.x, p0.y, 0f), normal);
                AddVertex(verts, normals, uvs, new Vector3(p1.x, p1.y, 0f), normal);
                AddVertex(verts, normals, uvs, new Vector3(p1.x, p1.y, depth), normal);
                AddVertex(verts, normals, uvs, new Vector3(p0.x, p0.y, depth), normal);
                tris.AddRange(new[] { s, s + 1, s + 2, s, s + 2, s + 3 });
            }

            var mesh = new Mesh { name = "Symbol_" + shape };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        static void AddVertex(List<Vector3> v, List<Vector3> n, List<Vector2> uv, Vector3 pos, Vector3 normal)
        {
            v.Add(pos);
            n.Add(normal);
            uv.Add(new Vector2(pos.x + 0.5f, pos.y + 0.5f));
        }

        static Vector2[] Regular(int count, float radius, float startDeg)
        {
            var p = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float a = (startDeg + i * 360f / count) * Mathf.Deg2Rad;
                p[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            return p;
        }
    }
}
