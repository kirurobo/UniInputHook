using System;

namespace Kirurobo.UniInputHook
{
    ///<summary>キーボードの操作をフックし、任意のメソッドを挿入する。</summary>
    abstract public class IRawInput : IDisposable
    {
        private protected static IRawInput Instance;

        /// <summary>
        /// シングルトンのインスタンスを保持
        /// </summary>
        public static IRawInput Current
        {
            get { return Instance; }
        }


        /// <summary>
        /// キーボードが操作されたときに発生する
        /// </summary>
        public static Action<KeyboardActionArgs> OnKeyAction;

        /// <summary>
        /// マウスのボタンが操作されたときに発生する
        /// </summary>
        public static Action<MouseActionArgs> OnMouseAction;



        private protected IRawInput()
        {
        }

        ~IRawInput()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Stop();
            Instance = null;
        }

        /// <summary>
        /// 実行するための権限があるか確認。必要に応じOSによるダイアログも出す
        /// </summary>
        /// <returns>権限がなく開始できなければfalseを返す</returns>
        public virtual PrivilegeState GetPrivilege()
        {
            return PrivilegeState.Normal;
        }

        /// <summary>
        /// 入力監視を開始
        /// </summary>
        public void Start()
        {
            Current.StartHook();
        }

        /// <summary>
        /// 入力監視を終了しリソースを解放
        /// </summary>
        public void Stop()
        {
            Current.StopHook();
        }

        /// <summary>
        /// Start the hook
        /// </summary>
        private protected virtual void StartHook()
        {
            UnityEngine.Debug.Log("StartCapture on IRawInput");
        }

        /// <summary>
        /// Stop the hook
        /// </summary>
        private protected virtual void StopHook()
        {
        }

        /// <summary>
        /// キーボード操作検出時のコールバックを呼び出す
        /// </summary>
        /// <param name="args"></param>
        private protected void DoKeyAction(KeyboardActionArgs args)
        {
            OnKeyAction?.Invoke(args);
        }

        /// <summary>
        /// マウス操作検出時のコールバックを呼び出す
        /// </summary>
        /// <param name="args"></param>
        private protected void DoMouseAction(MouseActionArgs args)
        {
            OnMouseAction?.Invoke(args);
        }
    }
}
