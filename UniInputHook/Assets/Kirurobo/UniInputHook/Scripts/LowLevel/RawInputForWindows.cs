using AOT;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


// 参考
// https://github.com/malaybaku/VMagicMirror
// http://hongliang.seesaa.net/article/7539988.html

namespace Kirurobo.UniInputHook
{

    ///<summary>キーボードの操作をフックし、任意のメソッドを挿入する。</summary>
    public class RawInputForWindows: IRawInput
    {
        public static new IRawInput Current
        {
            get {
                if (Instance == null)
                {
                    Instance = new RawInputForWindows();
                }
                return Instance;
            }
        }


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(WH hookId, KeyboardHookDelegate hookDelegate, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, KeyboardMessage message, ref KBDLLHOOKSTRUCT state);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// フックプロシージャーの種類
        /// </summary>
        /// <see cref="https://learn.microsoft.com/ja-jp/windows/win32/api/winuser/nf-winuser-setwindowshookexw"/>
        internal enum WH : int
        {
            KEYBOARD    = 2,
            KEYBOARD_LL = 13,
            MOUSE       = 7,
            MOUSE_LL    = 14,
        }

        private GCHandle hookDelegate;
        private IntPtr hook;

        // リピートへの反応を防ぐため、押下状態を記憶するバッファ
        private bool[] pressed = new bool[256];

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr KeyboardHookDelegate(int code , KeyboardMessage wParam, ref KBDLLHOOKSTRUCT lParam);

        /// <summary>
        /// フック開始
        /// </summary>
        private protected override void StartHook()
        {
            Stop();

            KeyboardHookDelegate callback = new KeyboardHookDelegate(KeyboardHookProcedure);
            hookDelegate = GCHandle.Alloc(callback);

            IntPtr hmod = IntPtr.Zero;
#if ENABLE_IL2CPP
            // IL2CPP では Process.GetCurrentProcess().MainModlue.ModuleName が未定義なよう？
            // 呼ぶとプロセス名取得に失敗する
#else
            // プロセス名を取得
            // GetProcessById(PID) で指定PIDが存在しない例外になる場合があるため try {} を使用
            try
            {
                string moduleName = Process.GetCurrentProcess().MainModule?.ModuleName ?? "";
                hmod = string.IsNullOrEmpty(moduleName) ? IntPtr.Zero : GetModuleHandle(moduleName);
            }
            catch
            {
                //UnityEngine.Debug.Log("Getting process name by PID failed");
            }
#endif
            hook = SetWindowsHookEx(WH.KEYBOARD_LL, callback, hmod, 0);
        }

        private protected override void StopHook()
        {
            if (hookDelegate.IsAllocated)
            {
                UnhookWindowsHookEx(hook);
                hook = IntPtr.Zero;
                hookDelegate.Free();
            }
        }


        /// <summary>
        /// フック時のプロシージャ―
        /// 
        /// static でないと IL2CPP で動作しない。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="kbd"></param>
        /// <returns></returns>
        [MonoPInvokeCallback(typeof(KeyboardHookDelegate))]
        private static IntPtr KeyboardHookProcedure(int code, KeyboardMessage message, ref KBDLLHOOKSTRUCT kbd)
        {
            if (code >= 0)
            {
                var self = (RawInputForWindows)Current;
                if (message == KeyboardMessage.KeyUp || message == KeyboardMessage.SysKeyUp)
                {
                    // キーが解放されれば押下フラグを下ろす
                    self.pressed[kbd.VkCode] = false;
                } else if (!self.pressed[kbd.VkCode]) {
                    // キーの押下で、かつ押下フラグがまだ立っていなければ、押下イベントを発火
                    self.pressed[kbd.VkCode] = true;
                    var args = self.CreateKeyboardActionArgs(message, kbd);
                    self.DoKeyAction(args);
                }
            }
            return CallNextHookEx(IntPtr.Zero, code, message, ref kbd);
        }

        /// <summary>
        /// Winodws 専用の値から汎用の値に変換
        /// </summary>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private KeyboardActionArgs CreateKeyboardActionArgs(KeyboardMessage message, KBDLLHOOKSTRUCT kbd)
        {
            var args = new KeyboardActionArgs();
            args.IsUp = (message ==  KeyboardMessage.KeyUp || message == KeyboardMessage.SysKeyUp);
            args.VkCode = ConvertKeyCode(kbd.VkCode);
            args.ScanCode = kbd.ScanCode;
            args.Modifires = kbd.Flag.RawValue;
            return args;
        }

        /// <summary>
        /// VKコードをUnityのKeyCode互換に変換
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int ConvertKeyCode(int key)
        {
            if (key >= 65 && key <= 90)
            {
                // アルファベット大文字は小文字とする
                return key + 32;
            } else if (key >= 0x60 && key <= 0x69)
            {
                // テンキーの数字
                return key - 0x60 + 0x100;
            } else if (key >= 0x70 && key <= 0x7E)
            {
                return key - 0x70 + 282;
            } else if (key >= 0x80 && key <= 0x8F)
            {
                // VK_F16～VK_F24 およびその後ろの予約領域は対応なし
                return 0;
            }

            switch (key)
            {
                case 0x10: return 303;  // VK_SHIFT to RightShift
                case 0x11: return 305;  // VK_CONTROL to RightControl
                case 0x12: return 307;  // VK_MENU to RightAlt
                case 0x14: return 301;  // VK_CAPITAL to CapsLock
                case 0x21: return 280;  // VK_PRIOR to PageUp
                case 0x22: return 281;  // VK_NEXT to PageDown
                case 0x23: return 279;  // VK_END to End
                case 0x24: return 278;  // VK_HOME to Home
                case 0x25: return 276;  // LeftArrow
                case 0x26: return 273;  // UpArrow
                case 0x27: return 275;  // RightArrow
                case 0x28: return 274;  // DownArrow
                case 0x29: return 0;    // VK_SELECT
                case 0x2A: return 316;  // VK_PRINT to Print
                case 0x2B: return 0;    // VK_Execute
                case 0x2C: return 316;  // VK_SNAPSHOT (PrtScr) to Print
                case 0x2D: return 277;  // Insert
                case 0x2E: return 127;  // Delete
                case 0x2F: return 315;  // Help
                case 0x5B: return 311;  // LeftWindows
                case 0x5C: return 312;  // RightWindows
                case 0x5D: return 319;  // VK_APPS to Menu
                case 0x5F: return 0;    // VK_SLEEP
                case 0x6A: return 42;   // VK_MUTIPLY to Asterisk
                case 0x6B: return 43;   // VK_ADD to Plus
                case 0x6C: return 44;   // VK_SEPARATOR to Comma
                case 0x6D: return 45;   // VK_SUBTRACT to Minus
                case 0x6E: return 46;   // VK_DECIMAL to Period
                case 0x6F: return 47;   // VK_DIVIDE to Slash
                case 0x90: return 300;  // VK_NUMLOCK to Numlock
                case 0x91: return 302;  // VK_SCROLL to ScrollLock
                case 0xA0: return 304;  // VK_LSHIFT
                case 0xA1: return 303;  // VK_RSHIFT
                case 0xA2: return 306;  // VK_LCONTROL
                case 0xA3: return 305;  // VK_RCONTROL
                case 0xA4: return 308;  // VK_LMENU to LeftAlt
                case 0xA5: return 307;  // VK_RMENU to RightAlt
            }
            return key;
        }

        ///<summary>メッセージコードを表す。</summary>
        private enum KeyboardMessage
        {
            ///<summary>キーが押された。</summary>
            KeyDown     = 0x100,

            ///<summary>キーが放された。</summary>
            KeyUp       = 0x101,

            ///<summary>システムキーが押された。</summary>
            SysKeyDown  = 0x104,

            ///<summary>システムキーが放された。</summary>
            SysKeyUp    = 0x105,
        }

        ///<summary>キーボードの状態を表す。</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            ///<summary>仮想キーコード。1～254の範囲</summary>
            public int VkCode;
            ///<summary>スキャンコード。</summary>
            public int ScanCode;
            ///<summary>各種特殊フラグ。</summary>
            public KeyboardStateFlag Flag;
            ///<summary>このメッセージが送られたときの時間。</summary>
            public uint Time;
            ///<summary>メッセージに関連づけられた拡張情報。</summary>
            public IntPtr ExtraInfo;
        }

        ///<summary>
        ///キーボードの状態を補足する。
        ///</summary>
        private struct KeyboardStateFlag
        {
            private int flag;

            // デバッグ用
            public int RawValue { get => (int)flag; set => flag = (int)value; }

            private bool IsFlagging(int value)
                => (flag & value) != 0;

            private void Flag(bool value, int digit)
                => flag = value ? (flag | digit) : (flag & ~digit);

            ///<summary>キーがテンキー上のキーのような拡張キーかどうかを表す。</summary>
            public bool IsExtended
            {
                get => IsFlagging(0x01);
                set => Flag(value, 0x01);
            }

            ///<summary>イベントがインジェクトされたかどうかを表す。</summary>
            public bool IsInjected
            {
                get => IsFlagging(0x10);
                set => Flag(value, 0x10);
            }

            ///<summary>ALTキーが押されているかどうかを表す。</summary>
            public bool AltDown
            {
                get => IsFlagging(0x20);
                set => Flag(value, 0x20);
            }

            ///<summary>キーが放されたどうかを表す。</summary>
            public bool IsUp
            {
                get => IsFlagging(0x80);
                set => Flag(value, 0x80);
            }

            override public string ToString()
            {
                return $"[{(IsExtended ? "E" : "")}{(IsInjected ? "I" : "")}{(AltDown ? "A" : "")}{(IsUp ? "U" : "")}]";
            }
        }
#endif
    }
}
