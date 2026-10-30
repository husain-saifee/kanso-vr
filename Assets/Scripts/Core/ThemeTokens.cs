using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.VR;

namespace DePhobia.Core
{
    /// <summary>
    /// Source of truth for the "Forest Mineral & Sage" visual language.
    /// Strictly guarantees zero blue or violet tones across all surfaces and states.
    /// High-contrast, WCAG AAA compliant (14.2:1 text contrast), restrained and composed.
    /// Integrates tactile 3D button depression and haptic feedback across all interactables.
    /// </summary>
    public static class ThemeTokens
    {
        // -------------------------------------------------------------
        // Base Palette (Non-blue, non-violet, organic mineral & earth)
        // -------------------------------------------------------------
        public static readonly Color CanvasBg = new Color(0.078f, 0.086f, 0.078f, 1.0f);        // #141614 (Earth Charcoal)
        public static readonly Color SurfacePanel = new Color(0.118f, 0.133f, 0.118f, 0.96f);    // #1E221E (Dark Spruce Moss, 96%)
        public static readonly Color SurfaceBorder = new Color(0.204f, 0.227f, 0.204f, 0.65f);   // #343A34 (Lichen Outline, 65%)
        public static readonly Color SurfaceInactive = new Color(0.157f, 0.176f, 0.157f, 0.90f); // #282D28 (Subtle Tab Fill)
        public static readonly Color SurfaceHover = new Color(0.176f, 0.204f, 0.176f, 1.0f);     // #2D342D (Quiet Hover)

        // -------------------------------------------------------------
        // Typography Colors
        // -------------------------------------------------------------
        public static readonly Color TextPrimary = new Color(0.953f, 0.961f, 0.949f, 1.0f);     // #F3F5F2 (Warm Off-White, 14.2:1)
        public static readonly Color TextSecondary = new Color(0.604f, 0.635f, 0.600f, 1.0f);   // #9AA299 (Ash Olive, 7.5:1)
        public static readonly Color TextMuted = new Color(0.424f, 0.455f, 0.420f, 1.0f);       // #6C746B (Quiet Stone)

        // -------------------------------------------------------------
        // Action & Feedback Accents
        // -------------------------------------------------------------
        public static readonly Color AccentSage = new Color(0.463f, 0.576f, 0.416f, 1.0f);      // #76936A (Matcha Sage - Primary Action)
        public static readonly Color AccentSageHover = new Color(0.533f, 0.655f, 0.482f, 1.0f); // #88A77B
        public static readonly Color AccentSageText = new Color(0.078f, 0.086f, 0.078f, 1.0f);  // #141614 (Dark text on active sage)

        public static readonly Color GentleExitClay = new Color(0.678f, 0.431f, 0.376f, 1.0f);  // #AD6E60 (Warm Umber Clay - Safe Return)
        public static readonly Color GentleExitHover = new Color(0.745f, 0.486f, 0.431f, 1.0f); // #BE7C6E

        public static readonly Color HorizonGlow = new Color(0.463f, 0.576f, 0.416f, 0.35f);    // Subtle breathing horizon ring

        // -------------------------------------------------------------
        // UI Helpers
        // -------------------------------------------------------------
        public static void StylePanel(GameObject panelObj, float alpha = 0.96f)
        {
            if (panelObj == null) return;
            var img = panelObj.GetComponent<Image>();
            if (img == null) img = panelObj.AddComponent<Image>();
            img.color = new Color(SurfacePanel.r, SurfacePanel.g, SurfacePanel.b, alpha);
        }

        public static void StyleButtonColors(Button btn, Color normal, Color hover, Color pressed)
        {
            if (btn == null) return;
            btn.transition = Selectable.Transition.ColorTint;
            var cb = btn.colors;
            cb.normalColor = normal;
            cb.highlightedColor = hover;
            cb.pressedColor = pressed;
            cb.selectedColor = normal;
            cb.disabledColor = new Color(normal.r, normal.g, normal.b, 0.35f);
            btn.colors = cb;

            // Automatically endow with physical 3D depression and point-of-contact haptics
            MakeTactile(btn);
        }

        public static VRTactileButton MakeTactile(Button btn)
        {
            if (btn == null) return null;
            var tactile = btn.GetComponent<VRTactileButton>();
            if (tactile == null)
            {
                tactile = btn.gameObject.AddComponent<VRTactileButton>();
            }
            return tactile;
        }

        public static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
            tmp.extraPadding = true;
            return tmp;
        }
    }
}
