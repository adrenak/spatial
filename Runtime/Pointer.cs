using UnityEngine;

namespace Adrenak.Spatial {
    // In order to interact with objects in the scene
    // this class casts a ray into the scene and if it finds
    // a Interactable it exposes it for other classes to use.
    // This script should be generally be placed on the camera.
    public class Pointer : MonoBehaviour {
        public LayerMask m_ExcludedLayers = 0;
        public bool debug = true;
        public float rayLength = 1000;
        public bool isDown;

        bool wasDown;
        Ray ray;
        RaycastHit hit;
        Interactable currentInteractable;
        Interactable lastInteractable = null;

        public Ray Ray => ray;
        public RaycastHit Hit => hit;
        public Interactable CurrentInteractable => currentInteractable;

        protected void Update() {
            Raycast();
            Click();
        }

        void Raycast() {
            ray = new Ray(transform.position, transform.forward);

            if (debug)
                Debug.DrawRay(transform.position, transform.forward * rayLength, Color.blue, Time.deltaTime);

            if (Physics.Raycast(ray, out hit, rayLength, ~m_ExcludedLayers)) {
                currentInteractable = hit.collider.gameObject.GetComponent<Interactable>();

                if (currentInteractable == null) {
                    TryLog("Hitting Interactable " + currentInteractable);
                    TryDeactivateLastInteractable();
                    return;
                }
                else
                    TryLog("Hitting non Interactable " + hit.collider.name);

                var distance = Vector3.Distance(transform.position, currentInteractable.transform.position);
                if (distance > currentInteractable.range)
                    return;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (currentInteractable != lastInteractable) {
                    TryDeactivateLastInteractable();
                    currentInteractable.Over(this);
                }
                lastInteractable = currentInteractable;
            }
            else {
                TryDeactivateLastInteractable();
            }
        }

        void Click() {
            if (isDown) {
                if (!wasDown)
                    currentInteractable?.Down();
            }
            else {
                if (wasDown)
                    currentInteractable?.Up();
            }
            wasDown = isDown;
        }

        void TryDeactivateLastInteractable() {
            if (lastInteractable == null)
                return;

            TryLog("Deselected last interactable " + lastInteractable);

            lastInteractable.Out(this);
            lastInteractable = null;
        }

        void TryLog(object msg) {
            if (debug)
                Debug.Log(msg);
        }
    }
}