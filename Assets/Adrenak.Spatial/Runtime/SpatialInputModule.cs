using UnityEngine;
using UnityEngine.EventSystems;

namespace Adrenak.Spatial {
    public class SpatialInputModule : BaseInputModule {
        public PointerEventData EventData { get; private set; }
        public RaycastResult RaycastResult { get; private set; }

        public Pointer interactor;

        Camera eventCamera;

        static SpatialInputModule instance;
        public static SpatialInputModule Instance {
            get {
                if (EventSystem.current == null)
                    EventSystem.current = FindObjectOfType<EventSystem>();

                if (EventSystem.current == null)
                    EventSystem.current = new GameObject("EventSystem").AddComponent<EventSystem>();

                if (instance == null)
                    instance = FindObjectOfType<SpatialInputModule>();

                if (instance == null)
                    instance = EventSystem.current.gameObject.AddComponent<SpatialInputModule>();

                return instance;
            }
        }

        protected override void Awake() {
            CreateEventCamera();
            InitializeCanvases();

            EventData = new PointerEventData(eventSystem);
            EventData.position = new Vector2(eventCamera.pixelWidth / 2, eventCamera.pixelHeight / 2);
        }

        void CreateEventCamera() {
            if (eventCamera == null) {
                var go = new GameObject("SUI Event Camera");
                go.hideFlags = HideFlags.DontSave;
                eventCamera = go.AddComponent<Camera>();
                eventCamera.fieldOfView = 5f;
                eventCamera.nearClipPlane = 0.01f;
                eventCamera.clearFlags = CameraClearFlags.Nothing;
                eventCamera.enabled = false;
            }
        }

        void InitializeCanvases() {
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
                InitializeCanvas(canvas, eventCamera);
        }

        void InitializeCanvas(Canvas canvas, Camera eventCamera) {
            if (canvas.renderMode != RenderMode.WorldSpace) {
                Debug.LogWarning($"Canvas on {canvas.gameObject} GameObject is not in WorldSpace more and will not work with spatial input");
                return;
            }
            canvas.worldCamera = eventCamera;
        }

        bool lastInputReady = false;
        public override void Process() {
            PointEventCamera(interactor.transform);

            eventSystem.RaycastAll(EventData, m_RaycastResultCache);
            EventData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();

            RaycastResult = EventData.pointerCurrentRaycast;

            HandlePointerExitAndEnter(EventData, RaycastResult.gameObject);
            ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.dragHandler);

            bool inputReady = InputReady();
            if (inputReady) {
                if (!lastInputReady)
                    Down();
                else
                    Hold();
            }
            else {
                if (lastInputReady)
                    Release();
            }

            lastInputReady = inputReady;
        }

        void PointEventCamera(Transform pointer) {
            if (pointer == null) return;

            eventCamera.transform.parent = pointer;
            eventCamera.transform.localPosition = Vector3.zero;
            eventCamera.transform.localEulerAngles = Vector3.zero;
        }

        public virtual bool InputReady() => interactor.isDown;

        GameObject lastPressed;
        void Down() {
            EventData.pointerPressRaycast = EventData.pointerCurrentRaycast;
            var target = EventData.pointerPressRaycast.gameObject;

            if (lastPressed != null) {
                ExecuteEvents.Execute(lastPressed, EventData, ExecuteEvents.deselectHandler);
                lastPressed = null;
            }

            var pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            EventData.pointerPress = pressed;
            ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerDownHandler);

            var dragged = ExecuteEvents.GetEventHandler<IDragHandler>(target);
            EventData.pointerDrag = dragged;
            ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.beginDragHandler);

            lastPressed = pressed;
        }

        void Hold() {
            EventData.pointerPressRaycast = EventData.pointerCurrentRaycast;
            var target = EventData.pointerPressRaycast.gameObject;

            var pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            EventData.pointerPress = pressed;
            ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerDownHandler);

            var draggedObject = ExecuteEvents.GetEventHandler<IDragHandler>(target);
            EventData.pointerDrag = draggedObject;
            ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.beginDragHandler);
        }

        void Release() {
            var target = EventData.pointerCurrentRaycast.gameObject;
            if (target == null) return;

            var released = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);

            if (EventData.pointerPress == released)
                ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerClickHandler);

            ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.endDragHandler);

            EventData.pointerCurrentRaycast.Clear();
            EventData.pointerPress = null;
            EventData.pointerDrag = null;
        }

        public virtual void RegisterCanvas(Canvas canvas) =>
            InitializeCanvas(canvas, eventCamera);
    }
}