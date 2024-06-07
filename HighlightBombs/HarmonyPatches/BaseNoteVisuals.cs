using HarmonyLib;
using UnityEngine;

namespace HighlightBombs.HarmonyPatches
{
    [HarmonyPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.Awake))]
    internal class BaseNoteVisualsAwakePatch
    {
        private static void Postfix(BaseNoteVisuals __instance)
        {
            if (__instance._noteController is not BombNoteController)
            {
                return;
            }

            // Custom Notes reuse meshes, so the outline may have already been added.
            var outline = __instance.gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = __instance.gameObject.AddComponent<Outline>();
            }

            outline.enabled = false;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 1f;
        }
    }

    [HarmonyPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.HandleNoteControllerDidInit))]
    internal class BaseNoteVisualsHandleNoteControllerDidInitPatch
    {
        private static void Postfix(BaseNoteVisuals __instance)
        {
            if (__instance._noteController is not BombNoteController)
            {
                return;
            }

            var outline = __instance._noteController.gameObject.GetComponent<Outline>();
            outline.enabled = true;
        }
    }
}
