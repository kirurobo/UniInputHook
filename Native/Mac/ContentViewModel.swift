//
//  ContentViewModel.swift
//  DebugLibUniInputHook
//
//  Created by owner on 2023/10/01.
//

import Foundation

class ContentViewModel: ObservableObject {
    static var shared = ContentViewModel()
    
    static var count: Int = 0
    
    @Published var keyDownMessage: String = "no event"
    @Published var keyUpMessage: String = "no event"
    @Published var mouseDownMessage: String = "no event"
    @Published var mouseUpMessage: String = "no event"

    init() {
        let confirm = RawInputHook.shared.confirmPrivilege()
        if (confirm != 0) {
            //exit(0)
        }
    }
    
    func start() {
        let confirm = RawInputHook.shared.confirmPrivilege()
        if (confirm != 0) {
            return
        }
        
        RawInputHook.shared.start()
        
        RawInputHook.shared.onEventCallback = {
            (type: Int32, key: Int32, param1: Int32, param2: Int32) in
            ContentViewModel.count += 1
            let eventType = RawInputHook.EventType(rawValue: type)
            let count = ContentViewModel.count
            
            switch eventType {
            case .KeyDown:
                ContentViewModel.shared.keyDownMessage =
                    "Key up: \(key) (\(param1))  Modifieres: \(param2) - \(count)"
                break
            case .KeyUp:
                ContentViewModel.shared.keyUpMessage =
                    "Key up: \(key) (\(param1))  Modifieres: \(param2) - \(count)"
                break
            case .MouseDown:
                ContentViewModel.shared.mouseDownMessage =
                    "Mouse down: \(key), (\(param1), \(param2)) - \(count)"
                break
            case .MouseUp:
                ContentViewModel.shared.mouseUpMessage =
                    "Mouse up: \(key), (\(param1), \(param2)) - \(count)"
                break
            default:
                break
            }
        }
    }

    func stop() {
        RawInputHook.shared.stop()
    }
}
