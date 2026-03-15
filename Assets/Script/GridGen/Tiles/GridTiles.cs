using UnityEngine;
using Script.GridGen.Enums;

namespace Script.GridGen.Tiles
{
    public class GridTiles : MonoBehaviour
    {
        public Vector2Int tileCoordinates;

        public Color startTileColor;

        public Color CurrentTileColor
        {
            set => meshRenderer.material.color = value;
        }

        public GridTileTypes tileType = GridTileTypes.None;

        public int diceNumber;

        public int owningPlayerIndex = -1;

        public MeshRenderer meshRenderer;

        private void Start()
        {
            CurrentTileColor = startTileColor;
        }
    }
}