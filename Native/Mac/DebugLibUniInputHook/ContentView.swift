//
//  ContentView.swift
//  DebugLibUniInputHook
//
//  Created by owner on 2023/10/01.
//

import SwiftUI

struct ContentView: View {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate
    @ObservedObject var viewModel = ContentViewModel.shared
    
    var body: some View {
        VStack {
            Image(systemName: "globe")
                .imageScale(.large)
                .foregroundStyle(.tint)
            Text("Key hook example")
            
            Button {
                viewModel.start()
            } label: {
                Text("Start")
            }
            
            Button {
                viewModel.stop()
            } label: {
                Text("Stop")
            }
            
            Text("\(viewModel.keyDownMessage)")
            Text("\(viewModel.keyUpMessage)")
            Text("\(viewModel.mouseDownMessage)")
            Text("\(viewModel.mouseUpMessage)")
        }
        .padding()
    }
}

#Preview {
    ContentView()
}
