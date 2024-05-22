using UnityEngine;
using System.Linq;

public class InputManager : MonoBehaviour
{
    public enum ControlScheme { PC, PS, XB }

    public ControlScheme controller
    {
        get { return _controller; }
        private set
        {
            if (_controller == value) return;
            _controller = value;
            OnControllerSchemeChange(_controller);
        }
    }
    private ControlScheme _controller;
    void OnControllerSchemeChange(ControlScheme scheme)
    {
        switch (scheme)
        {
            case ControlScheme.PC:
                break;
            case ControlScheme.PS:
                break;
            case ControlScheme.XB:
                break;
            default:
                controller = ControlScheme.PC;
                break;
        }

        GameManager.instance.UI.RefreshControlsUI();

        Debug.Log("<color=green>Current scheme: " + scheme + "</color>");
    }

    public Vector2 playerInput { get; private set; }
    public bool interact { get; private set; }
    public bool interactDown { get; private set; }
    public bool interactUp { get; private set; }
    public bool shapeshift { get; private set; }

    bool lastInputWasKeyboard;

    void Start()
    {
        // Force input reset, safety
        Input.ResetInputAxes();
    }

    private void Update()
    {
        ButtonInput();
        InputModeDetector();
    }

    private void FixedUpdate()
    {
        AxisInput();
    }

    private void AxisInput()
    {
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"),
                                  Input.GetAxisRaw("Vertical"));
    }

    private void InputModeDetector()
    {
        Vector2 moveAxis = new Vector2(Input.GetAxisRaw("Horizontal"),
                                  Input.GetAxisRaw("Vertical"));

        bool anyKeyDown = interactDown || shapeshift
            || Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical")
            ? true : false;

        // Was last input from keyboard?
        if (anyKeyDown || moveAxis != Vector2.zero && !Input.anyKey)
        {
            lastInputWasKeyboard = false;

            if (Input.inputString != string.Empty) lastInputWasKeyboard = true;
            //Debug.Log(Input.inputString);
        }

        // Which controller
        if (!lastInputWasKeyboard)
        {
            string controllerName = Input.GetJoystickNames().Length > 0 ? Input.GetJoystickNames()[0] : string.Empty;
            //Debug.Log(controllerName);
            ControlScheme curController = controllerName.Contains("Wireless") ? ControlScheme.PS : ControlScheme.XB;
            controller = curController;
        }
        else controller = ControlScheme.PC;

    }

    private void ButtonInput()
    {
        interact = Input.GetButton("Interact");
        interactDown = Input.GetButtonDown("Interact");
        interactUp = Input.GetButtonUp("Interact");
        shapeshift = Input.GetButtonDown("Shape");
    }
}
