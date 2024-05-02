using System.Runtime.InteropServices;
using AOT;


// 参考
// https://github.com/malaybaku/VMagicMirror
// http://hongliang.seesaa.net/article/7539988.html

namespace Kirurobo.UniInputHook
{

    ///<summary>キーボードの操作をフックし、任意のメソッドを挿入する。</summary>
    public class RawInputForMac : IRawInput
    {
        public static new IRawInput Current
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new RawInputForMac();
                }
                return Instance;
            }
        }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("LibUniInputHook", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BeginHook();

        [DllImport("LibUniInputHook", CallingConvention=CallingConvention.Cdecl)]
        private static extern void EndHook();

        [DllImport("LibUniInputHook", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint ConfirmPrivilege();

        [DllImport("LibUniInputHook", CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)]HookEventCallback callback);

        /// <summary>
        /// イベント種類を伝える定数
        /// </summary>
        private enum EventType : int
        {
            None = 0,
            KeyDown = 3,
            KeyUp = 5,
            MouseDown = 48,
            MouseUp = 80,
            MouseWheel = 144,
        }

        /// <summary>
        /// キー操作のコールバック
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="keyCode"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
        private delegate void HookEventCallback(
            EventType eventType,
            [MarshalAs(UnmanagedType.I4)] int keyCode,
            [MarshalAs(UnmanagedType.I4)] int param1,
            [MarshalAs(UnmanagedType.I4)] int param2
            );

        private GCHandle hookDelegate;


        /// <summary>
        /// 権限を確認し、必要ならばダイアログを開く
        /// </summary>
        /// <returns></returns>
        public override PrivilegeState GetPrivilege()
        {
            // アクセシビリティの確認をし、なければダイアログを開く
            var result = ConfirmPrivilege();
            
            // 問題ない場合は0が返ってくる
            if (result == 0)
            {
                return PrivilegeState.Normal;
            } else if (result == 2) {
                // その場で権限が取得された場合（これはおそらくないはず）
                return PrivilegeState.Permitted;
            }

            // ダイアログが開かれた場合
            return PrivilegeState.Confirmed;
        }

        private protected override void StartHook() {
            StopHook();

            var callback = new HookEventCallback(OnHooked);
            hookDelegate = GCHandle.Alloc(callback);

            RegisterCallback(callback);
            BeginHook();
        }

        private protected override void StopHook() {
            EndHook();
            RegisterCallback(null);

            if (hookDelegate.IsAllocated)
            {
                hookDelegate.Free();
            }
        }

        [MonoPInvokeCallback(typeof(HookEventCallback))]
        private void OnHooked(EventType eventType, int keyCode, int param1, int param2)
        {
            var args = new KeyboardActionArgs();
            args.VkCode = keyCode;

            switch (eventType)
            {
                case EventType.KeyDown:
                case EventType.KeyUp:
                    args.IsUp = (eventType == EventType.KeyUp);
                    args.ScanCode = param1;     // 今のところ未使用
                    args.Modifires = param2;    // 今のところ未使用
                    OnKeyAction?.Invoke(args);
                    break;

                case EventType.MouseDown:
                case EventType.MouseUp:
                    args.IsUp = (eventType == EventType.MouseUp);
                    OnKeyAction?.Invoke(args);
                    break;

                case EventType.MouseWheel:
                    args.Wheel = param1;    // 今のところ未使用
                    OnKeyAction?.Invoke(args);
                    break;
            }
        }
#endif
    }
}
