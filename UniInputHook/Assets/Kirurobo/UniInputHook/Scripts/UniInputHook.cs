using System;
using System.Collections.Concurrent;
using UnityEngine;


namespace Kirurobo.UniInputHook
{
    /// <summary>
    /// キーボード、マウスの操作をフォーカス外でも取得する
    /// </summary>
    public class UniInputHook : MonoBehaviour
    {
        /// <summary>
        /// キー押下にのみ反応するイベントハンドラー
        /// </summary>
        public Action<KeyCode> OnKeyDown;

        /// <summary>
        /// キー解放にのみ反応するイベントハンドラー
        /// </summary>
        public Action<KeyCode> OnKeyUp;

        /// <summary>
        /// キー押下、解放の両方に反応するイベントハンドラー
        /// 引数としてイベント種類を含む情報が渡される
        /// </summary>
        public Action<KeyboardActionArgs> OnKeyEvent;

        /// <summary>
        /// 権限チェックに失敗した場合のイベントハンドラー
        /// </summary>
        /// <param name="isDialogOpened">権限設定のダイアログが開かれた場合はtrue</param>
        public Action<bool> OnPrivilegeCheckFailed;

        // マウスイベントは未実装
        // public Action<KeyCode> OnMouseDown;
        // public Action<KeyCode> OnMouseUp;

        private IRawInput _rawInput;
        private ConcurrentQueue<KeyboardActionArgs> _keyActions;


        private void Awake()
        {
            // キーイベントキューの初期化
            _keyActions = new ConcurrentQueue<KeyboardActionArgs>();

            // RawInputのインスタンスを取得。なければ生成される
            _rawInput = RawInput.Current;
        }

        private void Start()
        {
            // ネイティブで処理されるハンドラーを登録
            RawInput.OnKeyAction += RawKeyActionHandler;
        }

        /// <summary>
        /// 毎フレームでの処理
        /// </summary>
        private void Update()
        {
            // キューに溜まったイベントがあれば処理する
            while (_keyActions.TryDequeue(out KeyboardActionArgs args))
            {
                KeyCode key;
                try
                {
                    key = (KeyCode)args.VkCode;
                } catch (Exception e)
                {
                    Debug.Log(e);
                    continue;
                }
                var keyCode = args.VkCode;
                if (keyCode <= 0) continue;

                if (args.IsUp)
                {
                    OnKeyUp?.Invoke(key);
                }
                else
                {
                    OnKeyDown?.Invoke(key);
                }
                OnKeyEvent?.Invoke(args);
            }
        }

        /// <summary>
        /// 監視を開始
        /// </summary>
        private void OnEnable()
        {
            if (_rawInput == null)
            {
                return;
            }

            // 有効化時には権限チェックも行う
            var privilege = _rawInput.GetPrivilege();
            if (privilege == PrivilegeState.Normal)
            {
                _rawInput?.Start();
            }
            else
            {
                // 権限チェックに失敗した場合のイベントを発行
                //  権限設定のダイアログが開かれた場合はtrueを引数に渡す
                OnPrivilegeCheckFailed?.Invoke(privilege == PrivilegeState.Confirmed);
            }
        }

        /// <summary>
        /// 監視を終了
        /// </summary>
        private void OnDisable()
        {
            _rawInput?.Stop();
        }

        /// <summary>
        /// 破棄時にはネイティブ部分も解放
        /// </summary>
        public void OnDestroy()
        {
            _rawInput?.Dispose();
        }

        /// <summary>
        /// ネイティブでの関数は短時間で終了するよう、キューに追加するのみ
        /// </summary>
        /// <param name="args"></param>
        private void RawKeyActionHandler(KeyboardActionArgs args)
        {
            _keyActions.Enqueue(args);
        }
    }
}