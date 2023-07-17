using System;

namespace Starlite
{
#if WINDOWS || XBOX
    public static class Program
    {
        private static void Main()
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
}

