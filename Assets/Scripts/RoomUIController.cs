using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace GetMentality
{
    /// <summary>Sketch HUD, inventory tray and bounded navigation between room views.</summary>
    public sealed class RoomUIController : MonoBehaviour
    {
        [Serializable]
        public sealed class RoomView
        {
            public string title;
            [TextArea] public string description;
            public Color background = new Color(0.98f, 0.97f, 0.95f);
        }

        [SerializeField] private RoomView[] rooms =
        {
            new RoomView { title = "Study", description = "A quiet desk. Your journey begins here.", background = new Color(0.98f, 0.97f, 0.95f) },
            new RoomView { title = "Living room", description = "Take a breath and look around.", background = new Color(0.95f, 0.97f, 0.98f) },
            new RoomView { title = "Entrance", description = "The door leads to your next adventure.", background = new Color(0.96f, 0.98f, 0.95f) }
        };
        [SerializeField, Min(1)] private int inventorySlots = 10;

        public int CurrentRoomIndex { get; private set; }
        public bool IsInventoryOpen { get; private set; }
        public event Action<int> RoomChanged;
        public event Action<bool> InventoryChanged;

        private UIDocument document;
        private PanelSettings panelSettings;
        private VisualElement root, room, overlay;
        private Label title, description, counter;
        private Button previous, next, inventoryButton, closeButton;

        private void OnEnable()
        {
            if (rooms == null || rooms.Length == 0)
            {
                Debug.LogError("Room UI needs at least one room.", this);
                enabled = false;
                return;
            }

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("RoomTheme");
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1280, 720);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            root = document.rootVisualElement;
            var stylesheet = Resources.Load<StyleSheet>("RoomUI");
            if (stylesheet != null) root.styleSheets.Add(stylesheet);
            BuildUI();
            CurrentRoomIndex = Mathf.Clamp(CurrentRoomIndex, 0, rooms.Length - 1);
            RefreshRoom();
            SetInventoryOpen(false);
        }

        private void BuildUI()
        {
            root.AddToClassList("app");
            room = Element(root, "room");
            room.pickingMode = PickingMode.Ignore;

            var location = Element(root, "location");
            location.pickingMode = PickingMode.Ignore;
            title = new Label { name = "room-title", pickingMode = PickingMode.Ignore };
            description = new Label { name = "room-description", pickingMode = PickingMode.Ignore };
            counter = new Label { name = "room-counter", pickingMode = PickingMode.Ignore };
            location.Add(title);
            location.Add(description);
            location.Add(counter);

            var status = Element(root, "status");
            status.pickingMode = PickingMode.Ignore;
            var bars = Element(status, "status-bars");
            AddStatusBar(bars, "HEALTH", 0.85f);
            AddStatusBar(bars, "MENTALITY", 0.65f);
            var portrait = Element(status, "portrait");
            portrait.Add(new Label("M") { pickingMode = PickingMode.Ignore });
            status.tooltip = "Status display (placeholder values)";

            var hand = new SketchGraphic(SketchGraphic.Shape.Hand);
            hand.AddToClassList("hand");
            root.Add(hand);

            previous = Button(root, "", () => Navigate(-1));
            previous.AddToClassList("arrow");
            previous.AddToClassList("arrow-left");
            previous.tooltip = "Previous room [Left]";
            previous.Add(new SketchGraphic(SketchGraphic.Shape.LeftArrow));
            next = Button(root, "", () => Navigate(1));
            next.AddToClassList("arrow");
            next.AddToClassList("arrow-right");
            next.tooltip = "Next room [Right]";
            next.Add(new SketchGraphic(SketchGraphic.Shape.RightArrow));

            overlay = Element(root, "overlay");
            overlay.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == overlay) SetInventoryOpen(false);
            });
            var panel = Element(overlay, "inventory-panel");
            var grid = Element(panel, "inventory-grid");
            for (var i = 0; i < Mathf.Max(1, inventorySlots); i++)
            {
                var slot = Element(grid, "inventory-slot");
                slot.tooltip = $"Empty slot {i + 1}";
                slot.Add(new Label((i + 1).ToString("00")));
            }
            // The same always-visible control opens and closes the bottom-left tray.
            inventoryButton = Button(root, "INVENTORY  [I]  +", ToggleInventory);
            inventoryButton.AddToClassList("inventory-toggle");
            closeButton = inventoryButton;
        }

        private static void AddStatusBar(VisualElement parent, string caption, float amount)
        {
            var row = Element(parent, "status-row");
            if (caption == "MENTALITY") row.AddToClassList("mentality-row");
            row.Add(new Label(caption));
            var track = Element(row, "status-track");
            var fill = Element(track, "status-fill");
            fill.style.width = Length.Percent(amount * 100f);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame && IsInventoryOpen)
            {
                SetInventoryOpen(false);
                return;
            }
            if (keyboard.iKey.wasPressedThisFrame)
            {
                ToggleInventory();
                return;
            }
            if (IsInventoryOpen) return;
            if (keyboard.leftArrowKey.wasPressedThisFrame) Navigate(-1);
            else if (keyboard.rightArrowKey.wasPressedThisFrame) Navigate(1);
        }

        public void ToggleInventory() => SetInventoryOpen(!IsInventoryOpen);

        public void SetInventoryOpen(bool open)
        {
            if (overlay == null) return;
            var changed = IsInventoryOpen != open;
            IsInventoryOpen = open;
            overlay.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            previous.SetEnabled(!open && CurrentRoomIndex > 0);
            next.SetEnabled(!open && CurrentRoomIndex < rooms.Length - 1);
            inventoryButton.text = open ? "INVENTORY  [I]  −" : "INVENTORY  [I]  +";
            inventoryButton.EnableInClassList("inventory-open", open);
            if (open) closeButton.Focus();
            else if (changed) inventoryButton.Focus();
            if (changed) InventoryChanged?.Invoke(open);
        }

        public void Navigate(int direction)
        {
            if (IsInventoryOpen || rooms == null || rooms.Length == 0) return;
            var target = Mathf.Clamp(CurrentRoomIndex + Math.Sign(direction), 0, rooms.Length - 1);
            if (target == CurrentRoomIndex) return;
            CurrentRoomIndex = target;
            RefreshRoom();
            RoomChanged?.Invoke(CurrentRoomIndex);
        }

        private void RefreshRoom()
        {
            var view = rooms[CurrentRoomIndex];
            title.text = view.title;
            description.text = view.description;
            counter.text = $"{CurrentRoomIndex + 1:00} / {rooms.Length:00}";
            room.style.backgroundColor = view.background;
            previous.SetEnabled(CurrentRoomIndex > 0);
            next.SetEnabled(CurrentRoomIndex < rooms.Length - 1);
        }

        private static VisualElement Element(VisualElement parent, string className)
        {
            var element = new VisualElement();
            element.AddToClassList(className);
            parent.Add(element);
            return element;
        }

        private static Button Button(VisualElement parent, string text, Action action)
        {
            var button = new Button(action) { text = text };
            parent.Add(button);
            return button;
        }

        private void OnDisable()
        {
            IsInventoryOpen = false;
            root?.Clear();
            overlay = null;
            if (document != null)
            {
                document.enabled = false;
                Destroy(document);
            }
            if (panelSettings != null) Destroy(panelSettings);
        }
    }
    /// <summary>Resolution-independent line drawings for the sketch HUD.</summary>
    internal sealed class SketchGraphic : VisualElement
    {
        internal enum Shape { Hand, LeftArrow, RightArrow }
        private readonly Shape shape;

        internal SketchGraphic(Shape shape)
        {
            this.shape = shape;
            pickingMode = PickingMode.Ignore;
            style.flexGrow = 1;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            var width = contentRect.width;
            var height = contentRect.height;
            if (width <= 0 || height <= 0) return;
            var pen = context.painter2D;
            pen.strokeColor = new Color(0.16f, 0.18f, 0.17f);
            pen.fillColor = new Color(0.98f, 0.97f, 0.94f, 0.96f);
            pen.lineWidth = 2.5f;
            pen.lineCap = LineCap.Round;
            pen.lineJoin = LineJoin.Round;
            Vector2 Point(float x, float y) => new Vector2(x * width, y * height);

            if (shape != Shape.Hand)
            {
                var left = shape == Shape.LeftArrow;
                pen.BeginPath();
                pen.MoveTo(Point(left ? 0.2f : 0.8f, 0.5f));
                pen.LineTo(Point(left ? 0.78f : 0.22f, 0.12f));
                pen.LineTo(Point(left ? 0.78f : 0.22f, 0.88f));
                pen.ClosePath();
                pen.Fill();
                pen.Stroke();
                return;
            }

            // Outline of an open right hand, normalized to its bounds.
            pen.BeginPath();
            pen.MoveTo(Point(0.23f, 1.05f));
            Curve(pen, width, height, 0.26f, 0.96f, 0.30f, 0.89f, 0.31f, 0.82f);
            Curve(pen, width, height, 0.26f, 0.72f, 0.12f, 0.58f, 0.10f, 0.48f);
            Curve(pen, width, height, 0.06f, 0.34f, 0.13f, 0.32f, 0.17f, 0.42f);
            Curve(pen, width, height, 0.20f, 0.49f, 0.26f, 0.53f, 0.33f, 0.57f);
            Curve(pen, width, height, 0.42f, 0.45f, 0.43f, 0.25f, 0.44f, 0.10f);
            Curve(pen, width, height, 0.44f, 0.00f, 0.49f, -0.01f, 0.52f, 0.10f);
            Curve(pen, width, height, 0.55f, 0.22f, 0.52f, 0.36f, 0.53f, 0.43f);
            Curve(pen, width, height, 0.55f, 0.50f, 0.61f, 0.27f, 0.65f, 0.13f);
            Curve(pen, width, height, 0.67f, 0.03f, 0.74f, 0.02f, 0.74f, 0.14f);
            Curve(pen, width, height, 0.74f, 0.25f, 0.66f, 0.43f, 0.67f, 0.47f);
            Curve(pen, width, height, 0.69f, 0.52f, 0.77f, 0.31f, 0.81f, 0.20f);
            Curve(pen, width, height, 0.85f, 0.11f, 0.91f, 0.13f, 0.88f, 0.24f);
            Curve(pen, width, height, 0.87f, 0.34f, 0.80f, 0.46f, 0.80f, 0.51f);
            Curve(pen, width, height, 0.86f, 0.49f, 0.91f, 0.33f, 0.92f, 0.30f);
            Curve(pen, width, height, 0.97f, 0.22f, 1.00f, 0.30f, 0.98f, 0.40f);
            Curve(pen, width, height, 0.96f, 0.56f, 0.87f, 0.69f, 0.82f, 0.77f);
            Curve(pen, width, height, 0.79f, 0.87f, 0.78f, 0.97f, 0.77f, 1.05f);
            pen.ClosePath();
            pen.Fill();
            pen.Stroke();
            pen.lineWidth = 1.4f;
            pen.BeginPath();
            pen.MoveTo(Point(0.37f, 0.64f));
            Curve(pen, width, height, 0.48f, 0.67f, 0.52f, 0.77f, 0.48f, 0.84f);
            pen.MoveTo(Point(0.52f, 0.58f));
            Curve(pen, width, height, 0.63f, 0.56f, 0.71f, 0.60f, 0.77f, 0.63f);
            pen.Stroke();
        }

        private static void Curve(Painter2D pen, float width, float height,
            float ax, float ay, float bx, float by, float cx, float cy)
        {
            pen.BezierCurveTo(new Vector2(ax * width, ay * height),
                new Vector2(bx * width, by * height), new Vector2(cx * width, cy * height));
        }
    }
}


