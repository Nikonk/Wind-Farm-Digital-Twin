using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake() 
    {
        DataHandler.LoadGameData();
    }
}
