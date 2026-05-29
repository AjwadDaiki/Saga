using Saga.Core;
using UnityEngine;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 5 round-2 — upgrade card panel (3 cards stacked vertically in portrait pivot).
    /// </summary>
    public static class UpgradesBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            BuildUpgradePanel(ctx.Canvas);
        }

        private static void BuildUpgradePanel(Canvas canvas)
        {
            var gm = GameManager.Instance;
            if (gm?.Content == null) return;
            var upgrades = gm.Content.AllUpgrades;
            if (upgrades == null || upgrades.Count == 0)
            {
                Debug.LogWarning("[MainSceneBootstrap] No upgrades in ContentDatabase — run menu \"Saga > Sprint 2 > Generate Upgrade Assets\" then reload Play.");
                return;
            }

            // Sprint 5 round-2: switched from absolute (sizeDelta 220px, anchoredPosition 96px)
            // Sprint 7.5 portrait pivot : cards stack VERTICALLY (was a 3-column row in landscape).
            // Band 8-34% from bottom of screen = ~500px on a 1920 canvas → ~165px per card.
            var panel = new GameObject("UpgradePanel", typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.04f, 0.08f);
            panelRt.anchorMax = new Vector2(0.96f, 0.34f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var count = upgrades.Count;
            var frac = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var card = new GameObject($"Card_{upgrades[i].UpgradeId}", typeof(RectTransform), typeof(UpgradeCardView));
                card.transform.SetParent(panelRt, false);
                var rt = (RectTransform)card.transform;
                // Stack top->bottom: card 0 at top, card N-1 at bottom.
                // Unity anchors: y=1 is top, y=0 is bottom. Card i occupies (1 - (i+1)*frac) .. (1 - i*frac).
                rt.anchorMin = new Vector2(0, 1f - (i + 1) * frac);
                rt.anchorMax = new Vector2(1, 1f - i * frac);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(8, 6);
                rt.offsetMax = new Vector2(-8, -6);

                card.GetComponent<UpgradeCardView>().Init(upgrades[i]);
            }
        }
    }
}
