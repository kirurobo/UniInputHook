using UnityEngine;
using Kirurobo.UniInputHook;
using UnityEngine.UIElements;

public class Cube : MonoBehaviour
{
    private UniInputHook inputHook;

    string message = "Activate something other than Unity window and press any key!";
    string privilegeFailedMessage = "";


    private void Awake()
    {
        inputHook = FindObjectOfType<UniInputHook>();

        //inputHook.OnKeyDown += OnKeyDown;
        inputHook.OnKeyDownArgs += OnKeyDownArgs;

        inputHook.OnPrivilegeCheckFailed += OnPrevilegeFailed;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnKeyDown(KeyCode keyCode)
    {
        message = $"OnKeyDown: {keyCode}";
    }
    private void OnKeyDownArgs(KeyboardActionArgs args)
    {
        message = $"{args}";
    }

    private void OnPrevilegeFailed(uint state)
    {
        Debug.Log("Privilege failed");
        privilegeFailedMessage = "You did not have the necessary permission to check keystrokes in external apps.\nPlease enable it and then restart the app.\nIf this message appears even if the app is already enabled, please remove this app from the OS system preferences and then launch the app again to enable it.";
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 100), message);

        if (privilegeFailedMessage != "") {
            if (GUI.Button(new Rect(10, 100, 400, 100), privilegeFailedMessage)) {
                privilegeFailedMessage = "";
            }
        }
    }

    private void Update()
    {
        var angle = 90f * Time.deltaTime;
        this.transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
    }

    void OnDestroy()
    {
    }

    private void OnKeyDown(string key)
    {
        Debug.Log($"OnKeyDown: {key}");
    }
}