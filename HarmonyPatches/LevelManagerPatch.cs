using System;
using HarmonyLib;
using UnityEngine;
using ADOFAI;

namespace MagicShapeHelper.HarmonyPatches
{
    public static class LevelManagerPatch
    {
        private static float currentTileAngle = 180f; // default straight tile
        private static float previousTileAngle = 180f; // default previous angle
        private static float currentMultiplier = 1f;

        public static float GetCurrentSpeedMultiplier()
        {
            return currentMultiplier;
        }

        public static float GetCurrentTileAngle()
        {
            return currentTileAngle;
        }

        public static float GetPreviousTileAngle()
        {
            return previousTileAngle;
        }

        public static bool IsEditorActive()
        {
            var editor = ADOBase.editor;
            if (editor == null) return false;
            return !editor.playMode;
        }

        public static bool TryGetAngle(out float angleDeg)
        {
            angleDeg = 0f;

            var editor = ADOBase.editor;
            if (editor == null) return false;
            if (editor.playMode) return false;
            if (editor.SelectionIsEmpty()) return false;

            ADOBase.lm.CalculateFloorAngleLengths();

            var floor = editor.selectedFloors[0];
            if (floor == null) return false;

            var angleLengthRadF = (float)floor.angleLength;
            if (angleLengthRadF <= 0f) return false;

            angleDeg = angleLengthRadF * Mathf.Rad2Deg;
            currentTileAngle = angleDeg;

            // Determine previous angle: if there is a previous floor, use its angleLength; otherwise 180°
            float prevAngleDeg = 180f;
            var prevIndex = floor.seqID - 1;
            if (prevIndex >= 0 && prevIndex < editor.floors.Count)
            {
                var prev = editor.floors[prevIndex];
                if (prev != null)
                {
                    var prevAngleRadF = (float)prev.angleLength;
                    if (prevAngleRadF > 0f)
                    {
                        prevAngleDeg = prevAngleRadF * Mathf.Rad2Deg;
                    }
                }
            }

            previousTileAngle = prevAngleDeg;

            // multiplier = current / previous (e.g., 180->90 => 0.5; 90->180 => 2.0)
            currentMultiplier = prevAngleDeg > 0f ? angleDeg / prevAngleDeg : 1f;
            return true;
        }

        public static void ApplyPatches(Harmony harmony, Action<string> log)
        {
            log?.Invoke("Manual angle reading (no Harmony target patches).");
        }
    }
}
