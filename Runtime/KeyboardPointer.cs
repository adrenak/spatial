using UnityEngine;

namespace Adrenak.Spatial {
    public class KeyboardPointer : Pointer {
        [SerializeField] KeyCode keyCode = KeyCode.Space;

        new void Update() {
            base.Update();
            isDown = Input.GetKey(keyCode);
        }
    }
}
