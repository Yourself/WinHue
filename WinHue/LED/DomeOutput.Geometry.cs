using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace WinHue.LED
{
    internal sealed partial class DomeOutput
    {
        // The dome geometry can be computed by tesselating the faces of an icosahedron.
        // The 5-way symmetry of the icosahedron can be exploited here. It can be split into 5 strips of 4 triangles:
        //       A
        //      / \
        //     /   \
        //    B-----C
        //   / \   / 
        //  /   \ /
        // D-----E
        //  \   /
        //   \ /   Since we're working with a dome, this bottom triangle can be ignored.
        //    F

        // The tesselation proceeds by splitting each edge into 4 smaller edges and each face then into 16 smaller triangles:
        //                         A     <-- Apex
        //                        / \
        //                       /   \
        //                      x-----x
        //                     / \   / \
        //                    /   \ /   \
        //                   x-----x-----x
        //                  / \   / \   / \
        //                 /   \ /   \ /   \
        //                x-----x-----x-----x
        //               / \   / \   / \   / \
        //              /   \ /   \ /   \ /   \
        //             B-----x-----x-----x-----C     Vertices numbered left to right top to bottom (A = 0, B = 10, C = 14)
        //            / \   / \   / \   / \   /
        //           /   \ /   \ /   \ /   \ /
        //          x-----x-----x-----x-----x     <-- Lighting extends down to here
        //         / \   / \   / \   / \   /
        //        /   \ /   \ /   \ /   \ /
        //       x-----x-----x-----x-----x        <-- Equator (bottom of dome)
        //      · ·   · ·   · ·   · ·   ·
        //     ·   · ·   · ·   · ·   · ·
        //    x · · x · · x · · x · · x
        //   · ·   · ·   · ·   · ·   ·
        //  ·   · ·   · ·   · ·   · ·
        // D · · x · · x · · x · · E
        const int LeafCount = 5;
        const int LeafVertexCount = 20;

        private static readonly Vector3[] VertexPositions;
        private static readonly (int, int)[] StrutVertices;

        private static (Vector3, Vector3) GetStrutEndPositions(int strut)
        {
            var (a, b) = StrutVertices[strut];
            return (VertexPositions[a], VertexPositions[b]);
        }

        private static Vector3 GetLedPosition(int strut, int led)
        {
            var (a, b) = GetStrutEndPositions(strut);
            int n = GetStrutLEDCount(strut);
            float t = (float)(led + 1) / (n + 2);
            return (b - a) * t + a;
        }

        private static Vector3 GetUncompressedVertexPosition(int uncompressedVert)
        {
            const double TwoPiOver5 = 2 * Math.PI / 5;
            int leaf = uncompressedVert / LeafVertexCount;
            int vert = uncompressedVert % LeafVertexCount;
            const double Sqrt5 = 2.2360679774997896964091736687313;
            const double R = 2 / Sqrt5;
            const double Z = 1 / Sqrt5;
            Vector3[] icoFaceVerts = new Vector3[]
            {
                new(0, 0, 1),
                new((float)(R * Math.Cos(leaf * TwoPiOver5)), (float)(R * Math.Sin(leaf * TwoPiOver5)), (float)Z),
                new((float)(R * Math.Cos((leaf + 1) * TwoPiOver5)), (float)(R * Math.Sin((leaf + 1) * TwoPiOver5)), (float)Z),
                new((float)(R * Math.Cos((leaf - 0.5) * TwoPiOver5)), (float)(R * Math.Sin((leaf - 0.5) * TwoPiOver5)), (float)-Z),
                new((float)(R * Math.Cos((leaf + 0.5) * TwoPiOver5)), (float)(R * Math.Sin((leaf + 0.5) * TwoPiOver5)), (float)-Z),
            };
            float[] weights;
            if (vert < 15)
            {
                // Overlaps triangle ABC
                int row = (int)((Math.Sqrt(1 + 8 * vert) - 1) / 2);
                int col = vert - row * (row + 1) / 2;
                float a = 1f - 0.25f * row;
                float b = 0.25f * col;
                weights = new[] { a, b, 1f - a - b, 0, 0 };
            }
            else if (vert == 15)
            {
                // Overlaps triangle BDE
                weights = new[] { 0, 0.75f, 0, 0.25f, 0 };
            }
            else
            {
                // Overlaps triangle BEC
                float c = 0.25f * (vert - 16);
                weights = new[] { 0, 0.75f - c, c, 0, 0.25f };
            }
            return Vector3.Normalize(icoFaceVerts.Zip(weights).Select((vw) => vw.First * vw.Second).Aggregate((a, b) => a + b));
        }

        private static void InitializeGeometry(out (int, int)[] struts, out Vector3[] positions)
        {
            (int, int)[] leafEdges = new[] {
                (12, 17), (17, 11), (11, 16), (16, 10), (10, 11), // Path 1 (Green)
                (12,  8), ( 8,  9), ( 9,  5), ( 5,  2), ( 2,  0), // Path 2 (Red)
                (11,  6), ( 6,  7), ( 7,  3), ( 3,  4), ( 4,  1), // Path 3 (Purple)
                (12, 18), (18, 17), (17, 16), (16, 15),           // Path 4 (Orange)
                (12, 13), (13, 14), (14,  9), ( 9, 13), (13, 19), // Path 5 (Violet)
                (12,  7), ( 7,  8), ( 8,  4), ( 4,  5), ( 5,  8), // Path 6 (Yellow)
                (12, 11), (11,  7), ( 7,  4), ( 4,  2), ( 4,  1), // Path 7 (Blue)
                ( 8, 13), (13, 18), (18, 19), (19, 14)            // Path 8 (Teal)
            };
            struts = new (int, int)[leafEdges.Length * LeafCount];
            // We can't just copy LeafEdges 5 times and shift the vertex indices since this will introduce duplicates
            // To avoid this we'll build a periodicity table mapping leaf vertex IDs to a periodicity group
            // 0 is somewhat special here since it's shared among all leaves since it's the apex, it does not appear in this table
            // The other vertices get either a positive value indicating the part of the current leaf they group with
            // Likewise a negative value indicates which part of the previous leaf they group with (taking the one's complement gets the group index)
            Dictionary<int, int> periodicVerts = new()
            {
                [1] = ~0,
                [2] = 0,
                [3] = ~1,
                [5] = 1,
                [6] = ~2,
                [9] = 2,
                [10] = ~3,
                [14] = 3,
                [15] = ~4,
                [19] = 4
            };
            List<int> apexVertices = new();
            var leafBinsQuery = Enumerable.Range(0, periodicVerts.Max((pair) => pair.Value) + 1).Select(_ => new List<int>());
            List<int>[][] leafPeriodicities = Enumerable.Range(0, LeafCount).Select(_ => leafBinsQuery.ToArray()).ToArray();

            List<int> GetGroup(int leaf, int group)
            {
                if (group < 0)
                {
                    leaf = (leaf + LeafCount - 1) % LeafCount;
                    group = ~group;
                }
                return leafPeriodicities[leaf][group];
            }

            for (int leaf = 0; leaf < LeafCount; ++leaf)
            {
                int offset = LeafVertexCount * leaf;
                for (int edge = 0; edge < leafEdges.Length; ++edge)
                {
                    var (v1, v2) = leafEdges[edge];
                    if (v1 == 0) apexVertices.Add(v1 + offset);
                    else if (periodicVerts.TryGetValue(v1, out int group))
                    {
                        GetGroup(leaf, group).Add(v1 + offset);
                    }

                    if (v2 == 0) apexVertices.Add(v2 + offset);
                    else if (periodicVerts.TryGetValue(v2, out int group))
                    {
                        GetGroup(leaf, group).Add(v2 + offset);
                    }
                    struts[leaf * leafEdges.Length + edge] = (v1 + offset, v2 + offset);
                }
            }

            // For each of the periodicity groups pick a candidate member (the minimum index) to replace all the others in the group
            Dictionary<int, int> replacements = new();
            SortedSet<int> removedSet = new();
            foreach (var group in leafPeriodicities.SelectMany(_ => _))
            {
                int candidate = group.Min();
                foreach (int vert in group)
                {
                    if (vert == candidate) continue;
                    replacements[vert] = candidate;
                    removedSet.Add(vert);
                }
            }
            positions = new Vector3[LeafCount * LeafVertexCount - removedSet.Count];
            var removed = removedSet.ToList();
            // Renumber all vertices shifting down to fill gaps
            for (int i = 0; i < struts.Length; ++i)
            {
                var (v1, v2) = struts[i];
                var p1 = GetUncompressedVertexPosition(v1);
                var p2 = GetUncompressedVertexPosition(v2);
                replacements.TryGetValue(v1, out v1);
                replacements.TryGetValue(v2, out v2);

                // Use BinarySearch to figure out how many vertices with smaller indices have been removed;
                // Since the remaining vertices can't be in the removed list, BinarySearch will always return
                // a negative number, `n`, where |n| is 1 more than the number to shift by
                v1 += removed.BinarySearch(v1) + 1;
                v2 += removed.BinarySearch(v2) + 1;
                struts[i] = (v1, v2);
                positions[v1] = p1;
                positions[v2] = p2;
            }
        }
    }
}
