using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public BallPhysics BallPhysics;

    [Header("UI Document")]
    [Tooltip("El UIDocument principal que contiene el HUD y las pantallas de Victoria/Derrota")]
    public UIDocument HudUIDocument;

    private Button _winRetryButton;
    private Button _retryButton;
    private Button _resetThrowButton;
    private Button _winMainMenuButton;

    
    private VisualElement _selectionContainer;
    private VisualElement _slidersContainer;
    private VisualElement _sliderContainer;
    private VisualElement _winContainer;
    private VisualElement _loseContainer;

    private Label _winTitleLabel;
    private Label _levelLabel;

    private VisualElement _victoryProgressContainer;
    private Label _victoryProgressLabel;
    private VisualElement _victoryProgressBarFill;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.ApplyLevelConfig();
        }

        if (BallPhysics == null)
        {
            BallPhysics = FindFirstObjectByType<BallPhysics>(FindObjectsInactive.Include);
        }

        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched += HandleBallLaunched;
            BallPhysics.OnBallStopped += HandleBallStopped;
        }

        
        if (HudUIDocument == null)
        {
            HudUIDocument = GetComponent<UIDocument>();
        }

        if (HudUIDocument != null)
        {
            var root = HudUIDocument.rootVisualElement;
            
            _selectionContainer = root.Q<VisualElement>("SelectionContainer");
            _slidersContainer = root.Q<VisualElement>("SlidersContainer");
            _sliderContainer = root.Q<VisualElement>("SliderContainer");
            _winContainer = root.Q<VisualElement>("WinContainer");
            _loseContainer = root.Q<VisualElement>("LoseContainer");

            _victoryProgressContainer = root.Q<VisualElement>("VictoryProgressContainer");
            _victoryProgressLabel = root.Q<Label>("VictoryProgressLabel");
            _victoryProgressBarFill = root.Q<VisualElement>("VictoryProgressBarFill");

            _winTitleLabel = root.Q<Label>("WinTitle");
            _levelLabel = root.Q<Label>("LevelLabel");

            UpdateLevelLabel();

            _winRetryButton = root.Q<Button>("WinRetryButton");
            _retryButton = root.Q<Button>("RetryButton");
            _resetThrowButton = root.Q<Button>("ResetThrowButton");
            _winMainMenuButton = root.Q<Button>("WinMainMenuButton");

            if (_winRetryButton != null) _winRetryButton.clicked += LoadNextLevelOrReset;
            if (_retryButton != null) _retryButton.clicked += ReloadLevel;
            if (_resetThrowButton != null) _resetThrowButton.clicked += ReloadLevel;
            if (_winMainMenuButton != null) _winMainMenuButton.clicked += GoToMainMenu;

            var frictionSlider = root.Q<Slider>("FrictionSlider");
            var levelForceLabel = root.Q<Label>("LevelForceLabel");

            if (levelForceLabel != null && BallPhysics != null)
            {
                levelForceLabel.text = $"Fuerza del Nivel: {BallPhysics.Force.magnitude:F1} N";
            }

            if (frictionSlider != null)
            {
                if (BallPhysics != null)
                {
                    if (BallPhysics.Friction <= 0.001f)
                    {
                        BallPhysics.SetFriction(frictionSlider.value);
                    }
                    else
                    {
                        frictionSlider.value = BallPhysics.Friction;
                    }
                }
                
                var frictionLabel = root.Q<Label>("FrictionLabel");
                if (frictionLabel != null)
                {
                    frictionLabel.text = $"FRICCIÓN: {frictionSlider.value:F3}";
                }

                frictionSlider.RegisterValueChangedCallback(evt => {
                    if (frictionLabel != null)
                    {
                        frictionLabel.text = $"FRICCIÓN: {evt.newValue:F3}";
                    }
                    if (BallPhysics != null)
                    {
                        BallPhysics.SetFriction(evt.newValue);
                    }
                });
            }
        }

        Debug.Log($"[UIManager] Setup complete. HUD Doc: {HudUIDocument != null}, WinContainer: {_winContainer != null}, LoseContainer: {_loseContainer != null}");

        if (GameManager.Instance != null)
        {
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    public void SetupNewBall(BallPhysics newBall)
    {
        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched -= HandleBallLaunched;
            BallPhysics.OnBallStopped -= HandleBallStopped;
        }

        BallPhysics = newBall;

        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched += HandleBallLaunched;
            BallPhysics.OnBallStopped += HandleBallStopped;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyLevelConfig();
        }

        UpdateLevelLabel();

        var uiDocument = HudUIDocument != null ? HudUIDocument : GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            var frictionSlider = root.Q<Slider>("FrictionSlider");
            var levelForceLabel = root.Q<Label>("LevelForceLabel");

            if (levelForceLabel != null && BallPhysics != null)
            {
                levelForceLabel.text = $"Fuerza del Nivel: {BallPhysics.Force.magnitude:F1} N";
            }

            if (frictionSlider != null && BallPhysics != null)
            {
                BallPhysics.SetFriction(frictionSlider.value);
            }
        }
    }

    private void UpdateLevelLabel()
    {
        if (_levelLabel != null)
        {
            int lvlNum = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex + 1 : 1;
            float forceMag = BallPhysics != null ? BallPhysics.Force.magnitude : 0f;
            _levelLabel.text = $"NIVEL {lvlNum} (FUERZA: {forceMag:F1} N)";
        }
    }

    private void HandleBallLaunched()
    {
        if (_slidersContainer != null) _slidersContainer.style.display = DisplayStyle.None;
        if (_sliderContainer != null) _sliderContainer.style.display = DisplayStyle.None;
    }

    private void HandleBallStopped()
    {
        bool isInside = (WinZone.Instance != null && WinZone.Instance.IsBallInside);
        string currentSt = (GameManager.Instance != null) ? GameManager.Instance.CurrentState.ToString() : "null";
        Debug.Log($"[UIManager] HandleBallStopped. IsBallInside: {isInside}, CurrentState: {currentSt}");

        if (isInside)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.BowlSpinning)
        {
            Debug.Log("[UIManager] Cambiando estado de juego a LOSE (Derrota).");
            GameManager.Instance.ChangeState(GameState.Lose);
        }
    }

    private void ReloadLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.WaitingThrow);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetLevels();
            GameManager.Instance.ChangeState(GameState.BallSelection);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("BallSelectionScreen");
    }

    private void LoadNextLevelOrReset()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.HasNextLevel())
            {
                GameManager.Instance.AdvanceLevel();
                GameManager.Instance.ChangeState(GameState.WaitingThrow);
            }
            else
            {
                GameManager.Instance.ResetLevels();
                GameManager.Instance.ChangeState(GameState.WaitingThrow);
            }
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void HandleStateChanged(GameState newState)
    {
        Debug.Log($"[UIManager] HandleStateChanged: {newState}");

        if (_selectionContainer != null) _selectionContainer.style.display = DisplayStyle.None;
        if (_slidersContainer != null) _slidersContainer.style.display = DisplayStyle.None;
        if (_sliderContainer != null) _sliderContainer.style.display = DisplayStyle.None;
        if (_winContainer != null) _winContainer.style.display = DisplayStyle.None;
        if (_loseContainer != null) _loseContainer.style.display = DisplayStyle.None;
        if (_resetThrowButton != null) _resetThrowButton.style.display = DisplayStyle.None;
        if (_victoryProgressContainer != null) _victoryProgressContainer.style.display = DisplayStyle.None;
        if (_winMainMenuButton != null) _winMainMenuButton.style.display = DisplayStyle.None;

        if (newState == GameState.Win || newState == GameState.Lose)
        {
            if (_winContainer != null) _winContainer.style.display = (newState == GameState.Win) ? DisplayStyle.Flex : DisplayStyle.None;
            if (_loseContainer != null) _loseContainer.style.display = (newState == GameState.Lose) ? DisplayStyle.Flex : DisplayStyle.None;

            if (newState == GameState.Win)
            {
                if (GameManager.Instance != null)
                {
                    bool hasNext = GameManager.Instance.HasNextLevel();
                    if (!hasNext)
                    {
                        if (_winTitleLabel != null)
                        {
                            _winTitleLabel.text = "Eres el rey de los bolos y la física";
                        }
                        if (_winRetryButton != null)
                        {
                            _winRetryButton.text = "Jugar de nuevo";
                        }
                        if (_winMainMenuButton != null)
                        {
                            _winMainMenuButton.style.display = DisplayStyle.Flex;
                            _winMainMenuButton.text = "Volver al menú principal";
                        }
                    }
                    else
                    {
                        if (_winTitleLabel != null)
                        {
                            int lvlNum = GameManager.Instance.CurrentLevelIndex + 1;
                            _winTitleLabel.text = $"NIVEL {lvlNum} COMPLETADO";
                        }
                        if (_winRetryButton != null)
                        {
                            _winRetryButton.text = "SIGUIENTE NIVEL";
                        }
                        if (_winMainMenuButton != null)
                        {
                            _winMainMenuButton.style.display = DisplayStyle.None;
                        }
                    }
                }
            }

            Debug.Log($"[UIManager] Modal mostrado para estado: {newState}. WinContainer visible: {_winContainer?.style.display}, LoseContainer visible: {_loseContainer?.style.display}");
        }
        else if (newState == GameState.BallSelection)
        {
            if (_selectionContainer != null) _selectionContainer.style.display = DisplayStyle.Flex;
        }
        else if (newState == GameState.WaitingThrow || newState == GameState.BowlSpinning)
        {
            if (_slidersContainer != null && newState == GameState.WaitingThrow) _slidersContainer.style.display = DisplayStyle.Flex;
            if (_sliderContainer != null && newState == GameState.WaitingThrow) _sliderContainer.style.display = DisplayStyle.Flex;
            if (_resetThrowButton != null) _resetThrowButton.style.display = DisplayStyle.Flex;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null)
            {
                while (GameManager.Instance.HasNextLevel())
                {
                    GameManager.Instance.AdvanceLevel();
                }
                GameManager.Instance.ChangeState(GameState.Win);
            }
        }
#endif

        if (WinZone.Instance != null && _victoryProgressContainer != null)
        {
            float curTime = WinZone.Instance.CurrentTime;
            float targetTime = WinZone.Instance.TargetTime;
            bool isInside = WinZone.Instance.IsBallInside;

            if (curTime > 0.01f || (isInside && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.BowlSpinning))
            {
                if (_victoryProgressContainer.style.display != DisplayStyle.Flex)
                {
                    _victoryProgressContainer.style.display = DisplayStyle.Flex;
                }

                if (_victoryProgressLabel != null)
                {
                    _victoryProgressLabel.text = "Mantente en la zona";
                }

                if (_victoryProgressBarFill != null)
                {
                    float percent = targetTime > 0f ? (curTime / targetTime) * 100f : 0f;
                    _victoryProgressBarFill.style.width = new StyleLength(new Length(percent, LengthUnit.Percent));
                }
            }
            else
            {
                if (_victoryProgressContainer.style.display != DisplayStyle.None)
                {
                    _victoryProgressContainer.style.display = DisplayStyle.None;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched -= HandleBallLaunched;
            BallPhysics.OnBallStopped -= HandleBallStopped;
        }

        if (_winRetryButton != null) _winRetryButton.clicked -= LoadNextLevelOrReset;
        if (_retryButton != null) _retryButton.clicked -= ReloadLevel;
        if (_resetThrowButton != null) _resetThrowButton.clicked -= ReloadLevel;
        if (_winMainMenuButton != null) _winMainMenuButton.clicked -= GoToMainMenu;
    }
}
