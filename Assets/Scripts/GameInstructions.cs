using UnityEngine;

public static class GameInstructions
{
    public static string Get()
    {
        return "- Move the turret (Aiming) using the mouse\n" +
               "- Fire with the left mouse button\n" +
               "- Move the Turret up and down using W and S keys\n" +
               "- Adjust mouse sensitivity with + / - keys\n" +
               "- Control volume with , / . keys (or < / > keys)\n" +
               "- Press ESC to pause game and allow to exit\n" +
               "- If enemies get close to the base they will fire at it\n" +
               "- If the Base HP reach zero you lose\n" +
               "- Have fun!";
    }
}
