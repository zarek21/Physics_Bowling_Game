using UnityEngine;

public class UIManager : MonoBehaviour
{
  private void HandleStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.PauseMenu:
            Debug.Log("Pausa Activada");
            break;
        }

        
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

     private void OnDisable()
    {
        GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }
}
