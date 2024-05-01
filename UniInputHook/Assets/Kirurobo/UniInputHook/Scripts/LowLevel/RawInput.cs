using Kirurobo.UniInputHook;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
public class RawInput : RawInputForWindows {}
#else
public class RawInput : RawInputForMac {}
#endif
