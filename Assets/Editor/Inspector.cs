using UnityEngine;
using UnityEditor;

public class Inspector : Editor
{
    [CustomEditor(typeof(Wave))]
    public class WaveInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            Wave wave = (Wave)target;
            GUILayout.Label("1 = Robot\n2 = Assassin\n3 = Walker\n4 = Protector\n5 = Rollermine\n6 = Commander");
            base.OnInspectorGUI();
        }
    }


    [CustomEditor(typeof(RoomData))]
    public class RoomDataInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            GUILayout.Label("WEAPONS\n0 = Sniper\n1 = Deagle\n2 = StickyBomb Launcher\n3 = Shredder" +
                "\n4 = Laser\n5 = Charge Rifle\n6 = Rocker Launcher\n\nPOWERUPS\n0 = Turret\n1 = Infinite Ammo" +
                "\n2 = Deadeye\n3 = Timefield\n4 = Clone\n5 = Upgrade");
            base.OnInspectorGUI();
        }
    }
}