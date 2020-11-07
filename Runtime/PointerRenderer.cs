using UnityEngine;

namespace Adrenak.Spatial {
    [RequireComponent(typeof(Pointer))]
    [RequireComponent(typeof(LineRenderer))]
    public class PointerRenderer : MonoBehaviour {
        LineRenderer lineRenderer = null;
        public Transform marker = null;

        public float defaultLength = 1;

        Pointer pointer = null;
        Ray ray;

        protected void Awake() {
            pointer = GetComponent<Pointer>();
            lineRenderer = GetComponent<LineRenderer>();
        }

        protected void Update() {
            Transform pt = pointer.transform;
            ray = new Ray(pt.position, pt.forward);

            if (SpatialInputModule.Instance.interactor == pointer) {
                float distance;
                if (CheckCanvasHit(out distance)) {
                    SetLength(distance);
                    EnableMarker(distance);
                }
                else if (CheckInteractableHit(out distance)) {
                    SetLength(distance);
                    EnableMarker(distance);
                }
                else {
                    SetLength(defaultLength);
                    DisableMarker();
                }
            }
            else {
                SetLength(defaultLength);
                DisableMarker();
            }
        }

        void SetLength(float distance) {
            if (lineRenderer != null) {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, ray.origin + ray.direction * distance);
            }
        }

        void DisableMarker() {
            if (marker != null)
                marker.gameObject.SetActive(false);
        }

        void EnableMarker(float distance) {
            if (marker != null) {
                if (!marker.gameObject.activeInHierarchy)
                    marker.gameObject.SetActive(true);
                marker.position = transform.position + transform.forward * distance;
                marker.LookAt(transform);
            }
        }

        bool CheckCanvasHit(out float distance) {
            var result = SpatialInputModule.Instance.RaycastResult;

            if (result.isValid) {
                distance = result.distance;
                return true;
            }
            else {
                distance = defaultLength;
                return false;
            }
        }

        bool CheckInteractableHit(out float distance) {
            var hit = pointer.Hit;

            if (hit.collider != null) {
                distance = hit.distance;
                return true;
            }
            else {
                distance = defaultLength;
                return false;
            }
        }
    }
}