using HarmonyLib;
using UnityEngine;
using ADOFAI;
using System.Reflection;

namespace MagicShapeHelper.HarmonyPatches
{
    [HarmonyPatch(typeof(scnEditor), "Update")]
    internal static class SpeedTriggerHotkeyPatch
    {
        private static void Postfix(scnEditor __instance)
        {
            if (!Main.Enabled)
                return;

            if (__instance == null || __instance.playMode)
            {
                Main.Logger?.Log($"[Hotkey] Skipped: editor null={__instance == null}, playMode={__instance?.playMode}");
                return;
            }

            // ensure editor context
            if (ADOBase.editor == null)
            {
                Main.Logger?.Log("[Hotkey] Skipped: ADOBase.editor is null");
                return;
            }

            // Hotkey not set
            if (Main.HotkeyCode == KeyCode.None)
            {
                Main.Logger?.Log("[Hotkey] Skipped: HotkeyCode is None");
                return;
            }

            if (!Input.GetKeyDown(Main.HotkeyCode))
                return;

            Main.Logger?.Log($"[Hotkey] Key {Main.HotkeyCode} pressed!");

            if (__instance.SelectionIsEmpty())
            {
                Main.Logger?.Log("[Hotkey] Skipped: Selection is empty");
                return;
            }

            var floor = __instance.selectedFloors[0];
            if (floor == null)
            {
                Main.Logger?.Log("[Hotkey] Skipped: floor is null");
                return;
            }

            if (!LevelManagerPatch.TryGetAngle(out _))
            {
                Main.Logger?.Log("[Hotkey] Skipped: TryGetAngle failed");
                return;
            }

            float multiplier = LevelManagerPatch.GetCurrentSpeedMultiplier();

            Main.Logger?.Log($"[Hotkey] Creating speed event: floor={floor.seqID}, multiplier={multiplier}");
            AddOrUpdateSpeedEvent(__instance, floor, multiplier);
            Main.Logger?.Log("[Hotkey] Speed event created/updated successfully");
        }

        private static void AddOrUpdateSpeedEvent(scnEditor editor, scrFloor floor, float multiplier)
        {
            try
            {
                // Try to find existing SetSpeed on this floor
                LevelEvent existing = null;
                foreach (var ev in editor.events)
                {
                    if (ev.eventType == LevelEventType.SetSpeed && ev.floor == floor.seqID)
                    {
                        existing = ev;
                        break;
                    }
                }

                LevelEvent e = existing ?? new LevelEvent(floor.seqID, LevelEventType.SetSpeed);
                
                // CRITICAL: Set values directly in the data dictionary BEFORE adding to events list
                // This ensures the editor UI sees the correct value when opening the event
                try
                {
                    var dataField = e.GetType().GetField("data", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (dataField != null)
                    {
                        var dataDict = dataField.GetValue(e);
                        if (dataDict != null)
                        {
                            var dictType = dataDict.GetType();
                            var setMethod = dictType.GetMethod("set_Item", new[] { typeof(string), typeof(object) });
                            if (setMethod != null)
                            {
                                // Set all required fields directly in dictionary BEFORE adding to events
                                setMethod.Invoke(dataDict, new object[] { "speedType", SpeedType.Multiplier });
                                
                                // Try multiple possible key names for multiplier
                                // The editor UI shows "Множитель ВРМ" which might use different key
                                setMethod.Invoke(dataDict, new object[] { "multiplier", (double)multiplier });
                                setMethod.Invoke(dataDict, new object[] { "bpmMultiplier", (double)multiplier });
                                setMethod.Invoke(dataDict, new object[] { "speedMultiplier", (double)multiplier });
                                setMethod.Invoke(dataDict, new object[] { "mult", (double)multiplier });
                                
                                setMethod.Invoke(dataDict, new object[] { "angle", 0.0 });
                                setMethod.Invoke(dataDict, new object[] { "duration", 0.0 });
                                Main.Logger?.Log($"[Hotkey] Set values in dict BEFORE adding to events: multiplier={multiplier} (tried multiple keys)");
                            }
                        }
                    }
                }
                catch (System.Exception dex)
                {
                    Main.Logger?.Log($"[Hotkey] Pre-add dictionary set failed: {dex.Message}");
                }
                
                // Also set through indexer for compatibility (try multiple key names)
                e["speedType"] = SpeedType.Multiplier;
                e["multiplier"] = multiplier;
                // Try alternative key names that editor might use
                try { e["bpmMultiplier"] = multiplier; } catch { }
                try { e["speedMultiplier"] = multiplier; } catch { }
                try { e["mult"] = multiplier; } catch { }
                e["angle"] = 0f;
                e["duration"] = 0f;

                if (existing == null)
                {
                    editor.events.Add(e);
                    Main.Logger?.Log($"[Hotkey] Added new SetSpeed event to floor {floor.seqID}");
                }
                else
                {
                    Main.Logger?.Log($"[Hotkey] Updated existing SetSpeed event on floor {floor.seqID}");
                }

                // Apply events to floors
                editor.ApplyEventsToFloors();
                
                // Re-verify and ensure values are still correct after ApplyEventsToFloors
                try
                {
                    var dataField = e.GetType().GetField("data", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (dataField != null)
                    {
                        var dataDict = dataField.GetValue(e);
                        if (dataDict != null)
                        {
                            var dictType = dataDict.GetType();
                            var setMethod = dictType.GetMethod("set_Item", new[] { typeof(string), typeof(object) });
                            var getMethod = dictType.GetMethod("get_Item", new[] { typeof(string) });
                            
                            if (setMethod != null && getMethod != null)
                            {
                                // Ensure values are still correct after ApplyEventsToFloors
                                setMethod.Invoke(dataDict, new object[] { "speedType", SpeedType.Multiplier });
                                
                                // Try multiple possible key names for multiplier
                                setMethod.Invoke(dataDict, new object[] { "multiplier", (double)multiplier });
                                setMethod.Invoke(dataDict, new object[] { "bpmMultiplier", (double)multiplier });
                                setMethod.Invoke(dataDict, new object[] { "speedMultiplier", (double)multiplier });
                                setMethod.Invoke(dataDict, new object[] { "mult", (double)multiplier });
                                
                                setMethod.Invoke(dataDict, new object[] { "angle", 0.0 });
                                setMethod.Invoke(dataDict, new object[] { "duration", 0.0 });
                                
                                // Verify
                                var finalMult = getMethod.Invoke(dataDict, new object[] { "multiplier" });
                                Main.Logger?.Log($"[Hotkey] After ApplyEventsToFloors: multiplier in dict = {finalMult} (expected {multiplier})");
                            }
                        }
                    }
                }
                catch (System.Exception dex)
                {
                    Main.Logger?.Log($"[Hotkey] Post-apply dictionary set failed: {dex.Message}");
                }
                
                // Also set through indexer after ApplyEventsToFloors for UI (try multiple key names)
                e["speedType"] = SpeedType.Multiplier;
                e["multiplier"] = multiplier;
                // Try alternative key names that editor might use
                try { e["bpmMultiplier"] = multiplier; } catch { }
                try { e["speedMultiplier"] = multiplier; } catch { }
                try { e["mult"] = multiplier; } catch { }
                
                // Try to refresh the editor UI by calling UpdateFloor or similar method
                try
                {
                    var updateMethod = editor.GetType().GetMethod("UpdateFloor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (updateMethod != null)
                    {
                        updateMethod.Invoke(editor, new object[] { floor.seqID });
                        Main.Logger?.Log($"[Hotkey] Called UpdateFloor({floor.seqID})");
                    }
                }
                catch (System.Exception uex)
                {
                    Main.Logger?.Log($"[Hotkey] UpdateFloor call failed (expected): {uex.Message}");
                }
                
                // Apply one more time to ensure everything is synced
                editor.ApplyEventsToFloors();
                
                // Verify final value
                var finalMultiplier = e["multiplier"];
                var finalSpeedType = e["speedType"];
                Main.Logger?.Log($"[Hotkey] Final: speedType={finalSpeedType}, multiplier={finalMultiplier} (expected {multiplier})");
                
                // Also try to read from floor directly to see what value it has
                try
                {
                    var floorSpeedProp = floor.GetType().GetProperty("speedMultiplier");
                    if (floorSpeedProp != null)
                    {
                        var floorSpeed = floorSpeedProp.GetValue(floor);
                        Main.Logger?.Log($"[Hotkey] Floor speedMultiplier property: {floorSpeed}");
                    }
                }
                catch (System.Exception fex)
                {
                    Main.Logger?.Log($"[Hotkey] Floor property read failed (expected): {fex.Message}");
                }
            }
            catch (System.Exception ex)
            {
                Main.Logger?.LogException(ex);
            }
        }
    }
}
