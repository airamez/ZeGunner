using UnityEngine;

public static class GameInstructions
{
    public static string Get()
    {
        return "- Aim turret with mouse\n" +
               "- Left click to fire\n" +
               "- W/S: Move turret up/down\n" +
               "- + -: Adjust mouse sensitivity\n" +
               "- , . or < >: Control volume\n" +
               "- ESC: Pause/Exit\n" +
               "- Enemies fire when close to base\n" +
               "- Red flash = off-screen enemy in range\n" +
               "- Base HP = 0 = Game Over\n" +
               "- Have fun!";
    }
}
