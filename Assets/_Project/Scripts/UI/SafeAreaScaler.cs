using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.5 Polish Phase 3 — wraps a RectTransform inside <see cref="Screen.safeArea"/>
    /// (iPhone notch / Android cutout / home indicator). All HUD elements parented to this
    /// container automatically stay inside the usable screen area on every device.
    ///
    /// Pattern : Farrukh Sajjad (Medium, https://farrukhsajjad.medium.com/the-right-way-to-wrap-your-ui-inside-the-safe-area-unity-71668119f02d).
    /// DIRECTION_ARTISTIQUE.md §3.1 — "Safe area mobile : prévoir iPhone avec encoche et Android avec coins arrondis. 60 px marge latérale min, 90-120 px haut/bas selon appareil."
    ///
    /// Usage :
    ///   - Attach this script to a child RectTransform of the MainCanvas.
    ///   - The container fills the safe area via anchorMin/anchorMax normalized to Screen.width/height.
    ///   - Parent all HUD/UI elements to this container (not to the Canvas directly).
    ///   - Cinematics + full-screen overlays may still parent to the Canvas if they should bleed past the notch.
    ///
    /// Auto-rebuilds on any orientation/dimension change via <see cref="OnRectTransformDimensionsChange"/>
    /// so portrait→landscape transitions update the safe area immediately.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public sealed class SafeAreaScaler : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastApplied;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rt = transform as RectTransform;
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            // Safe area can change without RectTransform dimensions changing (e.g. iOS rotates the
            // device but the canvas stays full-screen). Poll cheaply.
            var s = new Vector2Int(Screen.width, Screen.height);
            if (s != _lastScreenSize || Screen.orientation != _lastOrientation || Screen.safeArea != _lastApplied)
            {
                Apply();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void Apply()
        {
            if (_rt == null) _rt = transform as RectTransform;
            if (_rt == null) return;

            var area = Screen.safeArea;
            var w = Screen.width;
            var h = Screen.height;
            if (w <= 0 || h <= 0) return;

            var min = area.position;
            var max = area.position + area.size;
            min.x /= w; min.y /= h;
            max.x /= w; max.y /= h;

            _rt.anchorMin = min;
            _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;

            _lastApplied = area;
            _lastScreenSize = new Vector2Int(w, h);
            _lastOrientation = Screen.orientation;
        }
    }
}
