using System;
using System.Collections.Generic;
using Script.GridGen.Enums;
using Script.GridGen.Tiles;
using UnityEngine;

namespace Script.GridGen.Math
{
    public static class GridMath
    {
        private const float One6 = 1f / 6f;

        public static Vector3 CalculateSnapGrid(Vector3 tileSize)
        {
            return new Vector3(tileSize.x * 0.125f, tileSize.y * 0.25f * One6, 1f);
        }

        public static Vector2 CalculateTileDiv(Vector3 tileSize)
        {
            return new Vector2(tileSize.x * 0.125f, tileSize.y);
        }

        public static float GridSnap(float value, float gridSize)
        {
            if (gridSize == 0) return value;
            return Mathf.Round(value / gridSize) * gridSize;
        }

        public static Vector3 SnapVectorToVector(Vector3 vectorToSnap, Vector3 gridVector)
        {
            var resultX = GridSnap(vectorToSnap.x, gridVector.x);
            var resultY = GridSnap(vectorToSnap.y, gridVector.y);
            var resultZ = GridSnap(vectorToSnap.z, gridVector.z);

            return new Vector3(resultX, resultY, resultZ);
        }

        public static Vector3 CalculateGridBottomLeft(Vector3 center, Vector2 gridSize, Vector3 tileSize)
        {
            // Your original offset logic
            var gridOffsetMagicNumber = new Vector2(3f, 3f);
            var gridOffset = gridSize / gridOffsetMagicNumber;
            var snappedTileCount = new Vector3(tileSize.x * gridOffset.x, tileSize.y * gridOffset.y, 0f);

            return center - SnapVectorToVector(snappedTileCount, tileSize);
        }

        public static void GetValidRowForColumn(int column, Vector2Int gridSize, out int outLowerBound, out int outUpperBound)
        {
            var bShouldNotSubtract = ((gridSize.x + 1) / 2) % 2 == 0;

            var bounds = Mathf.Abs((gridSize.x - (column * 2 + 1)) / 2);

            outLowerBound = bShouldNotSubtract ? bounds + 1 : bounds;

            var rowOffset = bounds % 2 == 0 ? bounds : bounds - 1;

            var endY = gridSize.y * 2 - 1 - rowOffset;

            outUpperBound = bShouldNotSubtract ? endY : endY - 1;
        }

        public static void ForEachValidHexRow(int column, Vector2Int gridSize, Action<int> func)
        {
            GetValidRowForColumn(column, gridSize, out var lowerBound, out var upperBound);

            for (var y = lowerBound; y <= upperBound; y += 2)
                func(y);
        }

        public static Vector2Int MakeTileIndex(int x, int y)
        {
            return new Vector2Int(x * 6, y * 6);
        }

        public static Vector3 TileIndexToWorld(Vector2Int tileIndex, Vector3 gridBottomLeft, Vector3 tileSize)
        {
            var tileX075 = tileSize.x * 0.125f;
            var tileY05 = tileSize.y * 0.5f * One6;

            return gridBottomLeft + new Vector3(tileIndex.x * tileX075, tileIndex.y * tileY05, 0f);
        }

        public static void GetHexagonVertices(Vector2Int hexagonCenter, out List<Vector2Int> outVertices)
        {
            outVertices = new List<Vector2Int>
            {
                hexagonCenter + new Vector2Int(4, 0),
                hexagonCenter + new Vector2Int(2, 6),
                hexagonCenter + new Vector2Int(-2, 6),
                hexagonCenter + new Vector2Int(-4, 0),
                hexagonCenter + new Vector2Int(-2, -6),
                hexagonCenter + new Vector2Int(2, -6)
            };
        }

        public static void GetHexagonEdges(Vector2Int hexagonCenter, out List<Vector2Int> outEdges)
        {
            outEdges = new List<Vector2Int>
            {
                hexagonCenter + new Vector2Int(3, 3),
                hexagonCenter + new Vector2Int(0, 6),
                hexagonCenter + new Vector2Int(-3, 3),
                hexagonCenter + new Vector2Int(-3, -3),
                hexagonCenter + new Vector2Int(0, -6),
                hexagonCenter + new Vector2Int(3, -3)
            };
        }

        public static void SpawnVerticesAndEdges(Transform parentTile, Vector2Int tileIndex, Vector3 gridBottomLeft, Vector3 tileSize, Vector3 settlementScale, Dictionary<Vector2Int, GridTiles> outSpawnTiles, GridConfig config)
        {
            var tileX075 = tileSize.x * 0.125f;
            var tileY05 = tileSize.y * 0.5f * One6;

            GetHexagonVertices(tileIndex, out var vertices);

            for (var i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];

                var vertexWorld = gridBottomLeft + new Vector3(vertex.x * tileX075, vertex.y * tileY05, tileSize.z * 1.1f);

                /* ===== Settlement ===== */

                if (!outSpawnTiles.TryGetValue(vertex, out var settlement) || settlement == null)
                {
                    var obj = UnityEngine.Object.Instantiate(config.settlementTileClass, vertexWorld, Quaternion.identity);

                    obj.transform.localScale = settlementScale;
                    obj.transform.SetParent(parentTile, true);

                    settlement = obj;
                    settlement.tileCoordinates = vertex;
                    settlement.tileType = GridTileTypes.Settlement;

                    outSpawnTiles[vertex] = settlement;
                }

                /* ===== Road ===== */

                var nextIndex = (i + 1) % vertices.Count;
                var nextVertex = vertices[nextIndex];

                var nextWorld = gridBottomLeft + new Vector3(nextVertex.x * tileX075, nextVertex.y * tileY05, tileSize.z * 1.1f);

                var midPoint = (vertex + nextVertex) / 2;
                var midPointWorld = (vertexWorld + nextWorld) * 0.5f;

                if (outSpawnTiles.ContainsKey(midPoint))
                    continue;

                var roadLength = Vector3.Distance(vertexWorld, nextWorld);

                var rotation = Quaternion.LookRotation(nextWorld - vertexWorld);

                var roadObj = UnityEngine.Object.Instantiate(config.roadTileClass, midPointWorld, rotation);

                roadObj.transform.localScale = new Vector3(1f, config.roadMeshSizeY, roadLength); // (roadLength, config.roadMeshSizeY, 1f);
                roadObj.transform.SetParent(settlement.transform, true);
                roadObj.tileCoordinates = midPoint;
                roadObj.tileType = GridTileTypes.Road;

                outSpawnTiles[midPoint] = roadObj;
            }
        }
    }
}