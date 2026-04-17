using UnityEngine;
public class Crosshair : MonoBehaviour
{

    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(Screen.width / 2, Screen.height / 2, 4, 4), Texture2D.whiteTexture);
    }
}
