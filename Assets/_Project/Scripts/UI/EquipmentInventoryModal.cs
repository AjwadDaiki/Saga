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
    /// Sprint 7 inventory modal. Lists owned SpriteLayerSets grouped by slot, with the
    /// currently-equipped item shown at the top of each section. Click any row to equip
    /// it (the same row in the Body section can only "stay equipped" — body is mandatory).
    /// Built procedurally — Sprint 11 polish will swap for prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public class EquipmentInventoryModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _listContainer;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public RectTransform ListContainer { get => _listContainer; set => _listContainer = value; }

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        private bool _isOpen;

        private void Awake() => CloseInstant();

        private void OnEnable()
        {
            GameEvents.OnEquipmentChanged += HandleEquipmentChanged;
            GameEvents.OnItemAddedToInventory += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnEquipmentChanged -= HandleEquipmentChanged;
            GameEvents.OnItemAddedToInventory -= HandleInventoryChanged;
        }

        private void HandleEquipmentChanged(EquipmentSlot slot, SpriteLayerSet next, SpriteLayerSet previous)
        {
            if (_isOpen) Rebuild();
        }

        private void HandleInventoryChanged(SpriteLayerSet added)
        {
            if (_isOpen) Rebuild();
        }

        public void Open()
        {
            if (_group == null || _listContainer == null) return;
            if (_isOpen) return;
            _isOpen = true;
            Rebuild();

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

        private void Rebuild()
        {
            foreach (var go in _spawnedRows) if (go != null) Destroy(go);
            _spawnedRows.Clear();

            var gm = GameManager.Instance;
            var content = gm?.Content;
            var state = gm?.State;
            if (content == null || state == null || state.inventoryLayerSetIds == null) return;

            BuildSection(EquipmentSlot.Body, state, content);
            BuildSection(EquipmentSlot.Armor, state, content);
            BuildSection(EquipmentSlot.Weapon, state, content);
        }

        private void BuildSection(EquipmentSlot slot, GameState state, ContentDatabase content)
        {
            AddSectionHeader(slot.ToString().ToUpperInvariant());
            var equippedId = EquippedIdFor(state, slot);

            // Equipped first.
            if (!string.IsNullOrEmpty(equippedId))
            {
                var equipped = content.GetSpriteLayerSet(equippedId);
                if (equipped != null) BuildRow(equipped, isEquipped: true);
            }

            // Then owned-but-not-equipped, matching the slot.
            foreach (var id in state.inventoryLayerSetIds)
            {
                if (id == equippedId) continue;
                var l = content.GetSpriteLayerSet(id);
                if (l == null || l.SlotType != slot) continue;
                BuildRow(l, isEquipped: false);
            }
        }

        private void AddSectionHeader(string text)
        {
            var go = new GameObject($"Header_{text}",
                typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            go.transform.SetParent(_listContainer, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 36;
            le.flexibleWidth = 1;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.85f, 0.78f, 0.55f, 1f);
            tmp.text = text;
            tmp.raycastTarget = false;
            _spawnedRows.Add(go);
        }

        private void BuildRow(SpriteLayerSet layer, bool isEquipped)
        {
            var row = new GameObject($"Row_{layer.Id}",
                typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(EquipmentSlotView));
            row.transform.SetParent(_listContainer, false);
            var img = row.GetComponent<Image>();
            img.color = isEquipped
                ? new Color(0.18f, 0.22f, 0.16f, 0.95f)
                : new Color(0.12f, 0.12f, 0.12f, 0.9f);
            var le = row.GetComponent<LayoutElement>();
            le.preferredHeight = 100;
            le.flexibleWidth = 1;

            // Icon placeholder (uses iconSprite if set, else rarity-tinted block).
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(row.transform, false);
            var irt = (RectTransform)iconGo.transform;
            irt.anchorMin = new Vector2(0, 0); irt.anchorMax = new Vector2(0, 1);
            irt.pivot = new Vector2(0, 0.5f);
            irt.offsetMin = new Vector2(10, 10); irt.offsetMax = new Vector2(0, -10);
            irt.sizeDelta = new Vector2(80, 0);
            var iconImg = iconGo.GetComponent<Image>();
            if (layer.IconSprite != null) iconImg.sprite = layer.IconSprite;
            else iconImg.color = TintForRarity(layer.Rarity);

            // Info column.
            var info = new GameObject("Info",
                typeof(RectTransform), typeof(VerticalLayoutGroup));
            info.transform.SetParent(row.transform, false);
            var inforT = (RectTransform)info.transform;
            inforT.anchorMin = new Vector2(0, 0); inforT.anchorMax = new Vector2(1, 1);
            inforT.offsetMin = new Vector2(100, 8); inforT.offsetMax = new Vector2(-160, -8);
            var vlg = info.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            AddLabel(inforT, "Name", layer.DisplayName, 22, FontStyles.Bold,
                new Color(0.98f, 0.98f, 0.98f, 1f));
            AddLabel(inforT, "Subtitle",
                $"{layer.Rarity} · {layer.Voie} · {layer.SlotType}", 14,
                FontStyles.Italic, TintForRarity(layer.Rarity));
            AddLabel(inforT, "Stats",
                $"+{layer.StatsBonusForce} Force / +{layer.StatsBonusCrit:P0} crit",
                14, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f, 1f));

            // Action button.
            var btn = new GameObject("Action",
                typeof(RectTransform), typeof(Image), typeof(Button));
            btn.transform.SetParent(row.transform, false);
            var brt = (RectTransform)btn.transform;
            brt.anchorMin = new Vector2(1, 0.5f); brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-12, 0);
            brt.sizeDelta = new Vector2(140, 60);
            btn.GetComponent<Image>().color = isEquipped
                ? new Color(0.30f, 0.30f, 0.30f, 1f)
                : new Color(0.55f, 0.65f, 0.40f, 1f);

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(btn.transform, false);
            var lrt = (RectTransform)lblGo.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            var ltmp = lblGo.GetComponent<TextMeshProUGUI>();
            ltmp.alignment = TextAlignmentOptions.Center;
            ltmp.color = new Color(0.05f, 0.04f, 0.02f, 1f);
            ltmp.fontSize = 20;
            ltmp.fontStyle = FontStyles.Bold;
            ltmp.text = isEquipped ? "Équipé" : "Équiper";
            ltmp.raycastTarget = false;

            var view = row.GetComponent<EquipmentSlotView>();
            view.LayerSet = layer;
            view.IsEquipped = isEquipped;

            var capturedId = layer.Id;
            var capturedIsEquipped = isEquipped;
            btn.GetComponent<Button>().interactable = !capturedIsEquipped;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (capturedIsEquipped) return;
                var gm = GameManager.Instance;
                if (gm?.Equipment == null) return;
                gm.Equipment.Equip(gm.State, capturedId);
                // Rebuild handled via OnEquipmentChanged event.
            });

            _spawnedRows.Add(row);
        }

        private static string EquippedIdFor(GameState state, EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Body: return state.equippedBodyId;
                case EquipmentSlot.Armor: return state.equippedArmorId;
                case EquipmentSlot.Weapon: return state.equippedWeaponId;
                default: return null;
            }
        }

        private static Color TintForRarity(Rarity r)
        {
            switch (r)
            {
                case Rarity.Commun: return new Color(0.65f, 0.65f, 0.65f, 1f);
                case Rarity.Affute: return new Color(0.55f, 0.78f, 0.55f, 1f);
                case Rarity.Legendaire: return new Color(0.78f, 0.55f, 0.95f, 1f);
                case Rarity.Mythique: return new Color(0.98f, 0.55f, 0.30f, 1f);
                case Rarity.Sacre: return new Color(1.00f, 0.85f, 0.20f, 1f);
                default: return Color.white;
            }
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

        public class ReculerButtonHandler : MonoBehaviour, IPointerClickHandler
        {
            public EquipmentInventoryModal Modal { get; set; }
            public void OnPointerClick(PointerEventData _) => Modal?.Close();
        }
    }
}
