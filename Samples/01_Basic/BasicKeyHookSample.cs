using UnityEngine;
using Kirurobo.UniInputHook;

/// <summary>
/// キーフックの結果をOnGUIで表示する簡単なサンプル
/// </summary>
public class BasicKeyHookSample : MonoBehaviour
{
    private UniInputHook inputHook;

    string message = "Activate something other than Unity window and press any key!";
    string privilegeFailedMessage = "";


    /// <summary>
    /// Initialize the hook
    /// </summary>
    private void Awake()
    {
        inputHook = FindObjectOfType<UniInputHook>();

        //inputHook.OnKeyDown += OnKeyDown;
        inputHook.OnKeyDownArgs += OnKeyDownArgs;

        inputHook.OnPrivilegeCheckFailed += OnPrivilegeFailed;
    }

    /// <summary>
    /// Callback for key down event
    /// </summary>
    /// <param name="keyCode"></param>
    private void OnKeyDown(KeyCode keyCode)
    {
        message = $"OnKeyDown: {keyCode}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    private void OnKeyDown(string key)
    {
        Debug.Log($"OnKeyDown: {key}");
    }

    /// <summary>
    /// Callback for key down event with args
    /// </summary>
    /// <param name="args"></param>
    private void OnKeyDownArgs(KeyboardActionArgs args)
    {
        message = $"{args}";
    }

    /// <summary>
    /// Callback for privilege check failed on macOS
    /// </summary>
    /// <param name="isDialogOpened"></param>
    private void OnPrivilegeFailed(bool isDialogOpened)
    {
        Debug.Log("Privilege failed");
        privilegeFailedMessage = "You did not have the necessary permission to check keystrokes in external apps.\nPlease enable it and then restart the app.\nIf this message appears even if the app is already enabled, please remove this app from the OS system preferences and then launch the app again to enable it.";
    }

    /// <summary>
    /// Show messages on the screen
    /// </summary>
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 100), message);

        if (privilegeFailedMessage != "") {
            if (GUI.Button(new Rect(10, 100, 400, 100), privilegeFailedMessage)) {
                privilegeFailedMessage = "";
            }
        }
    }

    /// <summary>
    /// Rotate attached object to indicate the app is running
    /// </summary>
    private void Update()
    {
        var angle = 90f * Time.deltaTime;
        this.transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
    }
}