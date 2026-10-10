using Script.GridGen;
using Script.GridGen.Math;
using UnityEngine;

namespace Script.Mouse
{
    [RequireComponent(typeof(Camera))]
    public class MouseInteraction : MonoBehaviour
    {
        public GridConfig gridConfig;
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void FixedUpdate()
        {
            var result = GridMath.TraceMouseToGrid(_camera, gridConfig.gridCenterLocation, out var intersection);
            if (!result) return;

            var gridHitResult = GridMath.GetGridHitResult(intersection, gridConfig.gridBottomLeftLocation, gridConfig.gridTileSize);
            print(gridHitResult);
        }
    }
}