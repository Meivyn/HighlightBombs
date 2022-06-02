using HarmonyLib;
using UnityEngine;

namespace HighlightBombs.HarmonyPatches
{
    [HarmonyPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.Awake))]
    internal class BaseNoteVisualsAwakePatch
    {
        private static void Postfix(NoteController ____noteController)
        {
            if (____noteController is not BombNoteController)
            {
                return;
            }

            // Custom Notes reuse meshes, so the outline may have already been added.
            var outline = ____noteController.gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = ____noteController.gameObject.AddComponent<Outline>();
            }

            outline.CheckRenderersValidity();
            outline.enabled = false;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 1f;
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.HandleNoteControllerDidInit))]
    internal class BaseNoteVisualsHandleNoteControllerDidInitPatch
    {
        private static void Postfix(NoteControllerBase ____noteController)
        {
            if (____noteController is not BombNoteController)
            {
                return;
            }

            var outline = ____noteController.gameObject.GetComponent<Outline>();
            outline.enabled = true;
        }
    }
}
