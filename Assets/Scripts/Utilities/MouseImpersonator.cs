using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using WindowsInput;
using WindowsInput.Native;

public class MouseImpersonator
{
    private static InputSimulator _inputSimulator;

    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    public static void MoveMouse(int xPos, int yPos)
    {
        SetCursorPos(xPos, yPos);
    }

    static MouseImpersonator()
    {
        _inputSimulator = new();
    }

    public static void SimualteKeyboardPress(Key key)
    {
        VirtualKeyCode keyCode = MatchKeyboardKey(key);

        if (keyCode != VirtualKeyCode.None)
        {
            _inputSimulator.Keyboard.KeyDown(keyCode);
        }
    }

    public static void SimualteKeyboardRelease(Key key)
    {
        VirtualKeyCode keyCode = MatchKeyboardKey(key);

        if (keyCode != VirtualKeyCode.None)
        {
            _inputSimulator.Keyboard.KeyUp(keyCode);
        }
    }

    public static void SimualteMousePress(int key)
    {
        VirtualKeyCode keyCode = MatchMouseKey(key);

        switch (keyCode)
        {
            case VirtualKeyCode.LBUTTON:
                _inputSimulator.Mouse.LeftButtonDown();
                break;
            case VirtualKeyCode.RBUTTON:
                _inputSimulator.Mouse.RightButtonDown();
                break;
            default:
                break;
        }
    }

    public static void SimualteMouseRelease(int key)
    {
        VirtualKeyCode keyCode = MatchMouseKey(key);

        switch (keyCode)
        {
            case VirtualKeyCode.LBUTTON:
                _inputSimulator.Mouse.LeftButtonUp();
                break;
            case VirtualKeyCode.RBUTTON:
                _inputSimulator.Mouse.RightButtonUp();
                break;
            default:
                break;
        }
    }

    public static void Scroll(int direction)
    {
        if (direction == 0)
        {
            return;
        }

        _inputSimulator.Mouse.VerticalScroll(direction);
    }

    private static VirtualKeyCode MatchKeyboardKey(Key key)
    {
        return key switch
        {
            Key.Space => VirtualKeyCode.SPACE,
            Key.Enter => VirtualKeyCode.RETURN,
            Key.Tab => VirtualKeyCode.TAB,
            Key.Backquote => VirtualKeyCode.OEM_3,
            Key.Quote => VirtualKeyCode.OEM_7,
            Key.Semicolon => VirtualKeyCode.OEM_1,
            Key.Comma => VirtualKeyCode.OEM_COMMA,
            Key.Period => VirtualKeyCode.OEM_PERIOD,
            Key.Slash => VirtualKeyCode.OEM_2,
            Key.Backslash => VirtualKeyCode.OEM_5,
            Key.LeftBracket => VirtualKeyCode.OEM_4,
            Key.RightBracket => VirtualKeyCode.OEM_6,
            Key.Minus => VirtualKeyCode.OEM_MINUS,
            Key.Equals => VirtualKeyCode.OEM_PLUS,
            Key.A => VirtualKeyCode.VK_A,
            Key.B => VirtualKeyCode.VK_B,
            Key.C => VirtualKeyCode.VK_C,
            Key.D => VirtualKeyCode.VK_D,
            Key.E => VirtualKeyCode.VK_E,
            Key.F => VirtualKeyCode.VK_F,
            Key.G => VirtualKeyCode.VK_G,
            Key.H => VirtualKeyCode.VK_H,
            Key.I => VirtualKeyCode.VK_I,
            Key.J => VirtualKeyCode.VK_J,
            Key.K => VirtualKeyCode.VK_K,
            Key.L => VirtualKeyCode.VK_L,
            Key.M => VirtualKeyCode.VK_M,
            Key.N => VirtualKeyCode.VK_N,
            Key.O => VirtualKeyCode.VK_O,
            Key.P => VirtualKeyCode.VK_P,
            Key.Q => VirtualKeyCode.VK_Q,
            Key.R => VirtualKeyCode.VK_R,
            Key.S => VirtualKeyCode.VK_S,
            Key.T => VirtualKeyCode.VK_T,
            Key.U => VirtualKeyCode.VK_U,
            Key.V => VirtualKeyCode.VK_V,
            Key.W => VirtualKeyCode.VK_W,
            Key.X => VirtualKeyCode.VK_X,
            Key.Y => VirtualKeyCode.VK_Y,
            Key.Z => VirtualKeyCode.VK_Z,
            Key.Digit1 => VirtualKeyCode.VK_1,
            Key.Digit2 => VirtualKeyCode.VK_2,
            Key.Digit3 => VirtualKeyCode.VK_3,
            Key.Digit4 => VirtualKeyCode.VK_4,
            Key.Digit5 => VirtualKeyCode.VK_5,
            Key.Digit6 => VirtualKeyCode.VK_6,
            Key.Digit7 => VirtualKeyCode.VK_7,
            Key.Digit8 => VirtualKeyCode.VK_8,
            Key.Digit9 => VirtualKeyCode.VK_9,
            Key.Digit0 => VirtualKeyCode.VK_0,
            Key.LeftShift => VirtualKeyCode.SHIFT,
            Key.RightShift => VirtualKeyCode.RSHIFT,
            Key.LeftAlt => VirtualKeyCode.MENU,
            Key.RightAlt => VirtualKeyCode.RMENU,
            Key.LeftCtrl => VirtualKeyCode.CONTROL,
            //Key.RightCtrl => VirtualKeyCode.CONTROL,
            Key.LeftWindows => VirtualKeyCode.LWIN,
            Key.RightWindows => VirtualKeyCode.RWIN,
            Key.Escape => VirtualKeyCode.ESCAPE,
            Key.LeftArrow => VirtualKeyCode.LEFT,
            Key.RightArrow => VirtualKeyCode.RIGHT,
            Key.UpArrow => VirtualKeyCode.UP,
            Key.DownArrow => VirtualKeyCode.DOWN,
            Key.Backspace => VirtualKeyCode.BACK,
            Key.PageDown => VirtualKeyCode.NEXT,
            Key.PageUp => VirtualKeyCode.PRIOR,
            Key.Home => VirtualKeyCode.HOME,
            Key.End => VirtualKeyCode.END,
            Key.Insert => VirtualKeyCode.INSERT,
            Key.Delete => VirtualKeyCode.DELETE,
            Key.CapsLock => VirtualKeyCode.CAPITAL,
            Key.NumLock => VirtualKeyCode.NUMLOCK,
            Key.PrintScreen => VirtualKeyCode.SNAPSHOT,
            Key.ScrollLock => VirtualKeyCode.SCROLL,
            Key.Pause => VirtualKeyCode.PAUSE,
            Key.NumpadEnter => VirtualKeyCode.RETURN,
            Key.NumpadDivide => VirtualKeyCode.DIVIDE,
            Key.NumpadMultiply => VirtualKeyCode.MULTIPLY,
            Key.NumpadPlus => VirtualKeyCode.OEM_PLUS,
            Key.NumpadMinus => VirtualKeyCode.OEM_MINUS,
            Key.NumpadPeriod => VirtualKeyCode.OEM_PERIOD,
            Key.NumpadEquals => VirtualKeyCode.OEM_PLUS,
            Key.Numpad0 => VirtualKeyCode.NUMPAD0,
            Key.Numpad1 => VirtualKeyCode.NUMPAD1,
            Key.Numpad2 => VirtualKeyCode.NUMPAD2,
            Key.Numpad3 => VirtualKeyCode.NUMPAD3,
            Key.Numpad4 => VirtualKeyCode.NUMPAD4,
            Key.Numpad5 => VirtualKeyCode.NUMPAD5,
            Key.Numpad6 => VirtualKeyCode.NUMPAD6,
            Key.Numpad7 => VirtualKeyCode.NUMPAD7,
            Key.Numpad8 => VirtualKeyCode.NUMPAD8,
            Key.Numpad9 => VirtualKeyCode.NUMPAD9,
            Key.F1 => VirtualKeyCode.F1,
            Key.F2 => VirtualKeyCode.F2,
            Key.F3 => VirtualKeyCode.F3,
            Key.F4 => VirtualKeyCode.F4,
            Key.F5 => VirtualKeyCode.F5,
            Key.F6 => VirtualKeyCode.F6,
            Key.F7 => VirtualKeyCode.F7,
            Key.F8 => VirtualKeyCode.F8,
            Key.F9 => VirtualKeyCode.F9,
            Key.F10 => VirtualKeyCode.F10,
            Key.F11 => VirtualKeyCode.F11,
            Key.F12 => VirtualKeyCode.F12,
            _ => VirtualKeyCode.None,
        };
    }

    private static VirtualKeyCode MatchMouseKey(int key)
    {
        switch (key)
        {
            case 0:
                return VirtualKeyCode.LBUTTON;
            case 1:
                return VirtualKeyCode.RBUTTON;
            case 2:
                return VirtualKeyCode.MBUTTON;
            default:
                return VirtualKeyCode.None;
        }
    }
}