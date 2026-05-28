using UnityEngine;
using System;
public enum GameState
{
    BallSelection,
    PauseMenu,
    WaitingThrow,
    BowlSpinning,
    Scoreboard
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get;private set;}
    public GameState CurrentState {get;private set;}
    public event Action<GameState>OnStateChanged;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "BallSelectionScreen")
        {
            ChangeState(GameState.BallSelection);
        }
        else
        {
            ChangeState(GameState.WaitingThrow);
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

}
