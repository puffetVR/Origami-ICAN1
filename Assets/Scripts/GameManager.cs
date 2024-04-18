using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);
    }
    #endregion

    public PlayerManager Player;
}
