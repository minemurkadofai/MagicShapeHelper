using System;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using MagicShapeHelper.UI;

namespace MagicShapeHelper
{
    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger;
        private static GameObject _overlayObject;
        private static bool _showHotkeyList = false;
        private static Vector2 _hotkeyScroll = Vector2.zero;
        public static float PosXNorm = 0.9f; // normalized 0-1
        public static float PosYNorm = 0.02f;
        public static float BaseBpm = 120f;
        public static string HotkeyString = "F";
        public static KeyCode HotkeyCode = KeyCode.F;
        public static bool ShowMultiplier = true;
        public static bool ShowTileAngle = true;
        public static bool ShowPrevAngle = true;
        public static bool ShowBpm = true;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            try
            {
                var harmony = new Harmony(modEntry.Info.Id);
                var log = Logger != null ? new Action<string>(Logger.Log) : null;
                HarmonyPatches.LevelManagerPatch.ApplyPatches(harmony, log);
                harmony.PatchAll(); // Apply all [HarmonyPatch] attributes
                Logger.Log("Harmony patches applied (manual + PatchAll).");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }

            // Create overlay GameObject
            CreateOverlay();
            return true;
        }

        private static void CreateOverlay()
        {
            if (_overlayObject != null) return;

            _overlayObject = new GameObject("MagicShapeHelperOverlay");
            GameObject.DontDestroyOnLoad(_overlayObject);
            _overlayObject.hideFlags = HideFlags.HideAndDontSave;
            _overlayObject.AddComponent<SpeedMultiplierOverlay>();
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            if (_overlayObject != null)
            {
                _overlayObject.SetActive(value);
            }
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Overlay position", UnityModManager.UI.h2);
            GUILayout.Label($"X: {PosXNorm:F2}");
            PosXNorm = GUILayout.HorizontalSlider(PosXNorm, 0f, 1f, GUILayout.Width(200f));
            GUILayout.Label($"Y: {PosYNorm:F2}");
            PosYNorm = GUILayout.HorizontalSlider(PosYNorm, 0f, 1f, GUILayout.Width(200f));
            GUILayout.Space(8f);
            GUILayout.Label("Base BPM:");
            var bpmStr = GUILayout.TextField(BaseBpm.ToString("0.###"), GUILayout.Width(120f));
            if (float.TryParse(bpmStr, out var parsed))
            {
                BaseBpm = parsed;
            }

            GUILayout.Space(8f);
            GUILayout.Label($"Hotkey: {HotkeyString}");
            if (GUILayout.Button(_showHotkeyList ? "Hide keys" : "Choose key", GUILayout.Width(120f)))
            {
                _showHotkeyList = !_showHotkeyList;
            }
            if (_showHotkeyList)
            {
                float itemHeight = 20f;
                int visibleItems = 10;
                float viewHeight = itemHeight * visibleItems;
                _hotkeyScroll = GUILayout.BeginScrollView(_hotkeyScroll, GUILayout.Height(viewHeight), GUILayout.Width(220f));
                foreach (var key in GetKeyCodes())
                {
                    if (GUILayout.Button(key.ToString(), GUILayout.Height(itemHeight)))
                    {
                        HotkeyCode = key;
                        HotkeyString = key.ToString();
                        _showHotkeyList = false;
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.Space(8f);
            GUILayout.Label("Lines:");
            ShowMultiplier = GUILayout.Toggle(ShowMultiplier, "Speed Multiplier");
            ShowTileAngle = GUILayout.Toggle(ShowTileAngle, "Tile Angle");
            ShowPrevAngle = GUILayout.Toggle(ShowPrevAngle, "Prev Angle");
            ShowBpm = GUILayout.Toggle(ShowBpm, "BPM");

            if (_overlayObject != null)
            {
                var overlay = _overlayObject.GetComponent<SpeedMultiplierOverlay>();
                if (overlay != null)
                {
                    overlay.SetNormalizedPosition(PosXNorm, PosYNorm);
                }
            }
        }

        private static System.Collections.Generic.IEnumerable<KeyCode> GetKeyCodes()
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (code == KeyCode.None) continue;
                var name = code.ToString();
                if (name.StartsWith("Joystick") || name.StartsWith("Mouse"))
                    continue;
                yield return code;
            }
        }
    }
}
