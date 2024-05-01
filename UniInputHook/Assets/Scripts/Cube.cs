using UnityEngine;
using Kirurobo.UniInputHook;
using UnityEngine.UIElements;

public class Cube : MonoBehaviour
{
    private UniInputHook inputHook;

    string message = "";
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
        privilegeFailedMessage = "外部アプリでのキー入力チェックに必要な権限がありませんでした。有効にした後、アプリを再起動してください。¥nすでに有効でもこのメッセージが出る場合、環境設定から当アプリを除去した後、再度アプリを起動して有効化してください。";
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