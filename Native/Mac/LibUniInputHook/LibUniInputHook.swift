//
//  LibUniInputHook.swift
//  LibUniInputHook
//
//  Created by Kirurobo on 2023/10/01.
//

import AppKit

@_cdecl("BeginHook")
public func BeginHook() {
    RawInputHook.shared.start()
}

@_cdecl("EndHook")
public func EndHook() {
    RawInputHook.shared.stop()
}

@_cdecl("ConfirmPrivilege")
public func ConfirmPrivilege() -> Int32 {
    return RawInputHook.shared.confirmPrivilege()
}
/// コールバックを登録
///     nil だと登録解除
@_cdecl("RegisterCallback")
public func RegisterCallback(callback: @escaping hookEventCallback) {
    RawInputHook.shared.onEventCallback = callback
}


/// イベント発生時のコールバック定義
public typealias hookEventCallback = (@convention(c) (_ eventType: Int32, _ keyCode: Int32, _ param1: Int32, _ param2: Int32) -> Void)


/// 実処理を担当するクラス
public class RawInputHook {
    public static let shared = RawInputHook()
    
    public var onEventCallback: hookEventCallback? = nil

    private var eventMonitor = [Any?]()
    
    /// 直前の修飾キー状態をここに保持する
    private var lastModifierFlags: NSEvent.ModifierFlags = NSEvent.ModifierFlags()

    /// イベント種類を伝える定数
    public enum EventType : Int32 {
        //   0ビット目：キーボードなら1
        //   1ビット目：押下発火なら1
        //   2ビット目：解放時発火なら1
        //   3ビット目：未使用
        //   4ビット目：マウスなら1
        //   5ビット目：押下発火なら1
        //   6ビット目：解放発火なら1
        //   7ビット目：ホイール発火なら1
        case None = 0
        case KeyDown = 3
        case KeyUp = 5
        case MouseDown = 48
        case MouseUp = 80
        case MouseWheel = 144
    }
    
    init() {
    }
    
    deinit {
        stop()
        onEventCallback = nil
    }

    /// 必要はアクセシビリティへの権限を確認
    ///   0:問題なし、1:権限なしでダイアログが開いた、2:権限取得（非同期のためこれにはならないはず）
    public func confirmPrivilege() -> Int32 {
        // アクセシビリティの権限をチェックし、なければシステム環境設定へ誘導
        //  実際に有効にするためにはおそらく設定後アプリの再起動が必要
        if (!AXIsProcessTrusted()) {
            let isTrusted = AXIsProcessTrustedWithOptions(
                [kAXTrustedCheckOptionPrompt.takeRetainedValue(): true] as CFDictionary)
            return isTrusted ? 2 : 1
        }
        return 0
    }

    /// フックを開始
    public func start() {
        stop()
        
        // キー押下時
        eventMonitor += [NSEvent.addGlobalMonitorForEvents(matching: .keyDown) {
            [weak self] (event: NSEvent) in
                self?.doKeyboardEvent(type: EventType.KeyDown, event: event)
        }]
        
        // キー解放時
        eventMonitor += [NSEvent.addGlobalMonitorForEvents(matching: .keyUp) {
            [weak self] (event: NSEvent) in
                self?.doKeyboardEvent(type: EventType.KeyUp, event: event)
        }]
        
        // [Shift]や[Ctrl]等修飾キーは別途処理
        eventMonitor += [NSEvent.addGlobalMonitorForEvents(matching: .flagsChanged) {
            [weak self] (event: NSEvent) in
                self?.doFlagsChangedEvent(event: event)
        }]
        
        // マウスボタン押下時
        eventMonitor += [NSEvent.addGlobalMonitorForEvents(
            matching: [.leftMouseDown, .rightMouseDown, .otherMouseDown]) {
            [weak self] (event: NSEvent) in
                self?.doMouseEvent(type: EventType.MouseDown, event: event)
        }]
        
        // マウスボタン解放時
        eventMonitor += [NSEvent.addGlobalMonitorForEvents(
            matching: [.leftMouseUp, .rightMouseUp, .otherMouseUp]) {
            [weak self] (event: NSEvent) in
                self?.doMouseEvent(type: EventType.MouseUp, event: event)
        }]
        
        // マウスホイール
        eventMonitor += [NSEvent.addGlobalMonitorForEvents(
            matching: .scrollWheel) {
            [weak self] (event: NSEvent) in
                self?.doMouseWheelEvent(type: EventType.MouseWheel, event: event)
        }]
    }
    
    /// マウスイベント発生時の処理
    private func doMouseEvent(type: EventType, event: NSEvent) {
        onEventCallback?(
            type.rawValue,
            Int32(event.buttonNumber),
            Int32(event.absoluteX),
            Int32(event.absoluteY)
        )
    }
    
    /// マウスほいrーうイベント発生時の処理
    private func doMouseWheelEvent(type: EventType, event: NSEvent) {
        onEventCallback?(
            type.rawValue,
            Int32(event.buttonNumber),
            Int32(event.deltaX),
            Int32(event.deltaY)
        )
    }

    /// キーボードイベント発生時の処理。charactersを元に押されたキーを判断する
    private func doKeyboardEvent(type: EventType, event: NSEvent) {
        if !event.isARepeat {
            // リピートは反応しないものとする
            if let unicodeChars = event.characters?.unicodeScalars {
                for code in unicodeChars {
                    onEventCallback?(
                        type.rawValue,
                        ConvertKeyCode(code.value),
                        Int32(event.keyCode),    // スキャンコードに該当
                        Int32(event.modifierFlags.rawValue)  // 修飾キー。今のところ未使用だが参考までに値を渡してみる
                    )
                }
            }
        }
    }
    
    /// 修飾キー単独でイベントを発生させるための処理
    private func doFlagsChangedEvent(event: NSEvent)
    {
        let dict: Dictionary<Int32, NSEvent.ModifierFlags> = [
//            // JavaScriptと同様の場合
//            16: .shift,
//            17: .control,
//            18: .option,
//            20: .capsLock,
//            91: .command,   // 左[command]
//            //93: .command, // 右[command]
            
            // UnityのKeyCode準拠
            //303: .shift,  // RightShift
            304: .shift,    // LeftShift
            //305: .control,// RightControl
            306: .control,  // LeftControl
            //307: .option, // RightAlt
            308: .option,   // LeftAlt
            //309: .command,// RightCommand
            310: .command,  // LeftCommand
            //93: .command, // 右[command]
            301: .capsLock,
        ]
        
        // 各キー毎の判定
        for item in dict {
            if event.modifierFlags.contains(item.value) {
                // 修飾キーが追加されていればKeyDownとみなす
                if (!lastModifierFlags.contains(item.value)) {
                    onEventCallback?(
                        EventType.KeyDown.rawValue,
                        item.key,
                        Int32(event.keyCode),
                        Int32(event.modifierFlags.rawValue)
                    )
                }
            } else {
                // 修飾キーがなくなったらKeyUpとみなす
                if (lastModifierFlags.contains(item.value)) {
                    onEventCallback?(
                        EventType.KeyUp.rawValue,
                        item.key,
                        Int32(event.keyCode),
                        Int32(event.modifierFlags.rawValue)
                    )
                }
            }
        }
        
        // 前回の状態として記憶
        lastModifierFlags = event.modifierFlags
    }

    /// フックを終了
    public func stop() {
        for monitor in eventMonitor {
            if (monitor != nil) {
                NSEvent.removeMonitor(monitor!)
            }
        }
        eventMonitor.removeAll()
    }
    
    /// UnityのKeyCodeに合わせてキーコードを加工
    private func ConvertKeyCode(_ unicodeChar: UInt32) -> Int32 {
        let code = Int32(unicodeChar)
        
        switch code {
        case 65...90: // 大文字アルファベットは小文字として返す
            return code + 32
        case 63232: // UpArrow
            return 273
        case 63233: // DownArrow
            return 274
        case 63234: // LeftArrow
            return 276  // 左右の順が異なることに注意
        case 63235: // RightArrow
            return 275
        case 63276: // PageUp
            return 280
        case 63277: // PageDown
            return 281
        case 63236...63247: // F1 - F12
            return code - 63236 + 282
        default:
            return code
        }
    }
}
