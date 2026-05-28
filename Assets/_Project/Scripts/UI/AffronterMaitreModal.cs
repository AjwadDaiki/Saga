using System.Collections.Generic;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Modal listing the Maîtres the player can invoke (Sprint 6). Shows portrait placeholder,
    /// name, voie, intro citation + "Affronter" button per Maître. "Reculer" closes.
    /// Built procedurally — Sprint 11 polish will swap for scene-authored prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public class AffronterMaitreModal : MonoBehaviour
    {
        private static readonly Color VoieTintSamurai   = new Color(0.98f, 0.78f, 0.46f, 1f);
        private static readonly Color VoieTintViking    = new Color(0.49f, 0.65f, 0.79f, 1f);
        private static readonly Color VoieTintWuxia     = new Color(0.62f, 0.75f, 0.66f, 1f);
        private static readonly Color VoieTintSpartiate = new Color(0.79f, 0.47f, 0.29f, 1f);
        private static readonly Color VoieTintMongol    = new Color(0.72f, 0.61f, 0.42f, 1f);
        private static readonly Color VoieTintSaladin   = new Color(0.91f, 0.72f, 0.36f, 1f);
        private static readonly Color VoieTintAztec     = new Color(0.75f, 0.23f, 0.17f, 1f);
        private static readonly Color VoieTintGaulois   = new Color(0.36f, 0.54f, 0.31f, 1f);

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _listContainer;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public RectTransform ListContainer { get => _listContainer; set => _listContainer = value; }

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        private bool _isOpen;

        private void Awake()
        {
            CloseInstant();
        }

        public void Open()
        {
            if (_group == null || _listContainer == null) return;
            if (_isOpen) return;
            _isOpen = true;

            // Clear previous rows
            foreach (var go in _spawnedRows) if (go != null) Destroy(go);
            _spawnedRows.Clear();

            // Build rows
            var gm = GameManager.Instance;
            var available = gm?.Maitres?.GetAvailableMaitres();
            if (available == null) return;
            foreach (var maitre in available) BuildRow(maitre);

            _group.alpha = 0f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, 0.25f)
                .SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void Close()
        {
            if (_group == null || !_isOpen) return;
            _isOpen = false;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, 0.2f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    if (_group == null) return;
                    _group.interactable = false;
                    _group.blocksRaycasts = false;
                })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void CloseInstant()
        {
            _isOpen = false;
            if (_group == null) return;
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void BuildRow(MaitreData maitre)
        {
            if (maitre == null) return;
            var row = new GameObject($"Row_{maitre.Id}",
                typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            row.transform.SetParent(_listContainer, false);
            var img = row.GetComponent<Image>();
            img.color = new Color(0.10f, 0.10f, 0.10f, 0.9f);
            var le = row.GetComponent<LayoutElement>();
            le.preferredHeight = 140;
            le.flexibleWidth = 1;

            // Portrait placeholder (colored rect on the left)
            var portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(row.transform, false);
            var prt = (RectTransform)portrait.transform;
            prt.anchorMin = new Vector2(0, 0); prt.anchorMax = new Vector2(0, 1);
            prt.pivot = new Vector2(0, 0.5f);
            prt.offsetMin = new Vector2(12, 12); prt.offsetMax = new Vector2(0, -12);
            prt.sizeDelta = new Vector2(80, 0);
            portrait.GetComponent<Image>().color = TintForVoie(maitre.Voie);

            // Text content (name + voie + citation) — middle
            var info = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
            info.transform.SetParent(row.transform, false);
            var infoRt = (RectTransform)info.transform;
            infoRt.anchorMin = new Vector2(0, 0); infoRt.anchorMax = new Vector2(1, 1);
            infoRt.offsetMin = new Vector2(108, 12); infoRt.offsetMax = new Vector2(-160, -12);
            var infoLg = info.GetComponent<VerticalLayoutGroup>();
            infoLg.spacing = 4;
            infoLg.childAlignment = TextAnchor.MiddleLeft;
            infoLg.childForceExpandWidth = true; infoLg.childForceExpandHeight = false;

            AddLabel(infoRt, "Name", maitre.DisplayName, 28, FontStyles.Bold, new Color(0.98f, 0.98f, 0.98f, 1f));
            AddLabel(infoRt, "Voie", maitre.Voie.ToString(), 18, FontStyles.Italic, new Color(0.65f, 0.65f, 0.65f, 1f));
            AddLabel(infoRt, "Citation", $"« {maitre.IntroCitation} »", 16, FontStyles.Normal, new Color(0.78f, 0.78f, 0.78f, 1f));

            // "Affronter" button on the right
            var btn = new GameObject("Affronter",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btn.transform.SetParent(row.transform, false);
            var brt = (RectTransform)btn.transform;
            brt.anchorMin = new Vector2(1, 0.5f); brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-12, 0);
            brt.sizeDelta = new Vector2(140, 80);
            btn.GetComponent<Image>().color = TintForVoie(maitre.Voie) * new Color(1, 1, 1, 0.85f);
            var btnLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnLbl.transform.SetParent(btn.transform, false);
            var blrt = (RectTransform)btnLbl.transform;
            blrt.anchorMin = Vector2.zero; blrt.anchorMax = Vector2.one;
            blrt.offsetMin = Vector2.zero; blrt.offsetMax = Vector2.zero;
            var blTmp = btnLbl.GetComponent<TextMeshProUGUI>();
            blTmp.alignment = TextAlignmentOptions.Center;
            blTmp.color = new Color(0.05f, 0.04f, 0.02f, 1f);
            blTmp.fontSize = 24;
            blTmp.fontStyle = FontStyles.Bold;
            blTmp.text = "Affronter";
            blTmp.raycastTarget = false;

            var capturedId = maitre.Id;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                var gm = GameManager.Instance;
                if (gm == null) return;
                if (gm.Maitres != null && gm.Maitres.TryInvokeMaitre(gm.State, capturedId))
                {
                    Close();
                }
            });

            _spawnedRows.Add(row);
        }

        private static void AddLabel(Transform parent, string name, string text, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.text = text;
            tmp.raycastTarget = false;
        }

        private static Color TintForVoie(Voie voie)
        {
            switch (voie)
            {
                case Voie.Samurai: return VoieTintSamurai;
                case Voie.Viking: return VoieTintViking;
                case Voie.Wuxia: return VoieTintWuxia;
                case Voie.Spartiate: return VoieTintSpartiate;
                case Voie.Mongol: return VoieTintMongol;
                case Voie.Saladin: return VoieTintSaladin;
                case Voie.Aztec: return VoieTintAztec;
                case Voie.Gaulois: return VoieTintGaulois;
                default: return new Color(0.55f, 0.55f, 0.55f, 1f);
            }
        }

        public class ReculerButtonHandler : MonoBehaviour, IPointerClickHandler
        {
            public AffronterMaitreModal Modal { get; set; }
            public void OnPointerClick(PointerEventData _) => Modal?.Close();
        }
    }
}
