using UnityEngine;
using UnityEngine.Events;

namespace Adrenak.Spatial {
    // This class should be added to any gameobject in the scene
    // that should react to input based on the user's gaze.
    // It contains events that can be subscribed to by classes that
    // need to know about input specifics to this gameobject.
    public class Interactable : MonoBehaviour {
        public float range = Mathf.Infinity;
        public float timerDuration = 2;

        public UnityEvent onHoverBegin;
        public UnityEvent onHoverEnd;
        public FloatUnityEvent onTimerFilling = new FloatUnityEvent();
        public UnityEvent onTimerFilled;
        public UnityEvent onUp;
        public UnityEvent onDown;
        public float TimerElapsed => timer;
        public float TimerElapsedNormalized => TimerElapsed / timerDuration; 
        public bool IsOver { get; private set; }
        public Pointer Interactor => interactor;

        float timer = 0;
        Pointer interactor;

        protected void Update() {
            if (IsOver) {
                timer += Time.deltaTime;
                var normSelectionDuration = timer / timerDuration;
                normSelectionDuration = Mathf.Clamp01(normSelectionDuration);
                onTimerFilling.Invoke(normSelectionDuration);

                if (timer > timerDuration) {
                    IsOver = false;
                    timer = 0;
                    onTimerFilled.Invoke();
                }
            }
            else {
                timer = 0;
                interactor = null;
            }
        }

        private void OnDrawGizmosSelected() {
            if (range == Mathf.Infinity)
                return;
            Gizmos.color = new Color(1, 0, 0, .1F);
            Gizmos.DrawSphere(transform.position, range);
        }

        public void Over(Pointer interactor) {
            this.interactor = interactor;
            IsOver = true;
            onHoverBegin.Invoke();
        }

        public void Out(Pointer interactor) {
            this.interactor = interactor;
            IsOver = false;
            onHoverEnd.Invoke();
        }
        
        public void Up() => onUp.Invoke();

        public void Down() => onDown.Invoke();
    }
}