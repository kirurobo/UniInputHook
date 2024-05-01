using System;
using System.Collections.Concurrent;
using UnityEngine;


namespace Kirurobo.UniInputHook
{
    /// <summary>
    /// キーボード、マウスの操作をフォーカス外でも取得する
    /// </summary>
    public class UniInputHook : MonoBehaviour, IDisposable
    {
        /// <summary>
        /// イベントハンドラー
        /// </summary>
        public Action<KeyCode> OnKeyDown;
        public Action<KeyCode> OnKeyUp;
        public Action<KeyCode> OnMouseDown;
        public Action<KeyCode> OnMouseUp;
        public Action<uint> OnPrivilegeCheckFailed;

        public Action<KeyboardActionArgs> OnKeyDownArgs;

        private IRawInput _rawInput;
        private ConcurrentQueue<KeyboardActionArgs> _keyActions;


        private void Awake()
        {
            _keyActions = new ConcurrentQueue<KeyboardActionArgs>();

            _rawInput = RawInput.Current;
        }

        private void Start()
        {

            RawInput.OnKeyAction += OnKeyAction;
        }

        private void Update()
        {

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
                OnKeyDownArgs?.Invoke(args);
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
            if (_rawInput.GetPrivilege())
            {
                _rawInput?.Start();
            }
            else
            {
                OnPrivilegeCheckFailed?.Invoke(1);
            }
        }

        /// <summary>
        /// 監視を終了
        /// </summary>
        private void OnDisable()
        {
            _rawInput?.Stop();
        }
        public void Dispose()
        {
            _rawInput?.Dispose();
        }

        /// <summary>
        /// ネイティブでの関数は短時間で終了するよう、キューに追加するのみ
        /// </summary>
        /// <param name="args"></param>
        private void OnKeyAction(KeyboardActionArgs args)
        {
            _keyActions.Enqueue(args);
        }
    }
}