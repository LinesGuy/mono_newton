﻿using System;

namespace mono_newton_directx {
    public static class Program {
        [STAThread]
        static void Main() {
            using(var game = new GameRoot())
                game.Run();
        }
    }
}
