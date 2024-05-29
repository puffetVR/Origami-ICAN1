using UnityEngine;

public class Shaker : MonoBehaviour
{
    public void CamShake(string tstr)
    {
        var sStrings = tstr.Split(","[0]);

        float t = float.Parse(sStrings[0]);
        float str = float.Parse(sStrings[1]);

        GameManager.instance.Player.cam.ShakeCamera(t, str);
    }
}
