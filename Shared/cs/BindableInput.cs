using InputStateManager;
using Microsoft.Xna.Framework.Input;

namespace Shared;

public abstract class BindableInput {
    public bool Pressed  { get; protected set; }
    public bool JustDown { get; private set; }
    public bool JustUp   { get; private set; }

    private bool _lastPressed;
    
    public void Update(InputManager input) {
        _lastPressed = Pressed;
        
        UpdateState(input);
        
        JustUp = _lastPressed && !Pressed;
        JustDown = !_lastPressed && Pressed;
    }

    protected abstract void UpdateState(InputManager input);

    public class GamePad(Buttons button) : BindableInput {
        protected override void UpdateState(InputManager input) {
            Pressed = input.Pad(0).Is.Press(button);
        }
    }
    
    public class Mouse(InputStateManager.Inputs.Mouse.Button button) : BindableInput {
        protected override void UpdateState(InputManager input) {
            Pressed = input.Mouse.Is.Press(button);
        }
    }
    
    public class Key(Keys key) : BindableInput {
        protected override void UpdateState(InputManager input) {
            Pressed = input.Key.Is.Press(key);
        }
    }
}