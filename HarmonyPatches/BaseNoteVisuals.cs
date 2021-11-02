using HarmonyLib;
using NiceMiss;
using UnityEngine;

namespace HighlightBombs.HarmonyPatches
{
    [HarmonyPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.Awake))]
    internal class BaseNoteVisualsAwakePatch
    {
        private static void Postfix(NoteController ____noteController)
        {
            var outline = ____noteController.gameObject.AddComponent<Outline>();
            outline.CheckRenderersValidity();
            outline.enabled = false;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 1f;
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.HandleNoteControllerDidInit))]
    internal class BaseNoteVisualsHandleNoteControllerDidInitEventPatch
    {
        private static void Postfix(NoteController ____noteController)
        {
            if (____noteController.noteData.colorType != ColorType.None)
            {
                return;
            }

            var outline = ____noteController.gameObject.GetComponentInChildren<Outline>();
            outline.enabled = true;
        }
    }
}
