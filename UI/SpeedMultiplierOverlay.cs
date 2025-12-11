using UnityEngine;
using MagicShapeHelper.HarmonyPatches;

namespace MagicShapeHelper.UI
{
    public class SpeedMultiplierOverlay : MonoBehaviour
    {
        private GUIStyle labelStyle;
        private Rect displayRect;
        private float posXNorm;
        private float posYNorm;
        private float width = 260f;
        private float height = 90f;

        private void Start()
        {
            InitializeStyle();
            posXNorm = Main.PosXNorm;
            posYNorm = Main.PosYNorm;
            displayRect = new Rect(GetPosX(), GetPosY(), width, height);
        }

        private void InitializeStyle()
        {
            labelStyle = new GUIStyle
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                wordWrap = false
            };
            labelStyle.normal.textColor = Color.white;
            labelStyle.normal.background = null;
        }

        private void OnGUI()
        {
            if (!Main.Enabled)
                return;

            if (!LevelManagerPatch.IsEditorActive())
                return;

            if (!LevelManagerPatch.TryGetAngle(out var angle))
                return;

            float multiplier = LevelManagerPatch.GetCurrentSpeedMultiplier();
            float bpm = Main.BaseBpm * multiplier;
            float prevAngle = LevelManagerPatch.GetPreviousTileAngle();

            string text = BuildText(multiplier, angle, prevAngle, bpm);
            if (string.IsNullOrEmpty(text))
                return;

            // Keep overlay aligned to top-right even if resolution changes
            displayRect.x = GetPosX();
            displayRect.y = GetPosY();

            GUI.Label(displayRect, text, labelStyle);
        }

        private string BuildText(float multiplier, float angle, float prevAngle, float bpm)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (Main.ShowMultiplier)
            {
                sb.Append($"Speed Multiplier: {FormatNumber(multiplier)}");
            }

            if (Main.ShowTileAngle)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append($"Tile Angle: {FormatNumber(angle)}°");
            }

            if (Main.ShowPrevAngle)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append($"Prev Angle: {FormatNumber(prevAngle)}°");
            }

            if (Main.ShowBpm)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append($"BPM: {FormatNumber(bpm)}");
            }

            return sb.ToString();
        }

        private string FormatNumber(float value)
        {
            // Show as few trailing zeros as possible while retaining precision
            // Use "G" to avoid fixed width; default precision ~7 digits
            return value.ToString("G");
        }

        public void SetNormalizedPosition(float xNorm, float yNorm)
        {
            posXNorm = Mathf.Clamp01(xNorm);
            posYNorm = Mathf.Clamp01(yNorm);
        }

        private float GetPosX()
        {
            return Mathf.Clamp(posXNorm * Screen.width - width, 0, Screen.width - width);
        }

        private float GetPosY()
        {
            return Mathf.Clamp(posYNorm * Screen.height, 0, Screen.height - height);
        }
    }
}
