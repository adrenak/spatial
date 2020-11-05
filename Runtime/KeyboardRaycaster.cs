using UnityEngine;

namespace Adrenak.Spatial {
    public class KeyboardRaycaster : Pointer {
        [SerializeField] Pointer pointer;
        [SerializeField] KeyCode keyCode;

        new void Update() {
            base.Update();
            isDown = Input.GetKey(keyCode);
        }
    }
}
