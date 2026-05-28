using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public BallPhysics BallPhysics;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        // Si venimos de la escena de selección, instanciar el prefab seleccionado
        if (BallSelector.SelectedBallPrefab != null)
        {
            var placeholders = FindObjectsByType<BallPhysics>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Transform spawnTrans = placeholders.Length > 0 ? placeholders[0].transform : null;
            Vector3 spawnPos = spawnTrans != null ? spawnTrans.position : Vector3.zero;
            Quaternion spawnRot = spawnTrans != null ? spawnTrans.rotation : Quaternion.identity;

            foreach (var p in placeholders) 
            {
                Destroy(p.gameObject);
            }

            var newBallObj = Instantiate(BallSelector.SelectedBallPrefab, spawnPos, spawnRot);
            BallPhysics = newBallObj.GetComponent<BallPhysics>();
        }

        if (BallPhysics == null)
        {
            BallPhysics = FindFirstObjectByType<BallPhysics>(FindObjectsInactive.Include);
        }

        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched += HandleBallLaunched;
        }

        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
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
                    frictionSlider.value = BallPhysics.Friction;
                }
                frictionSlider.label = $"Fricción: {frictionSlider.value:F3}";
                frictionSlider.RegisterValueChangedCallback(evt => {
                    frictionSlider.label = $"Fricción: {evt.newValue:F3}";
                    if (BallPhysics != null)
                    {
                        BallPhysics.SetFriction(evt.newValue);
                    }
                });
            }
        }

        if (GameManager.Instance != null)
        {
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    public void SetupNewBall(BallPhysics newBall)
    {
        // 1. Desuscribirse de la bola anterior si existe
        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched -= HandleBallLaunched;
        }

        // 2. Asignar la nueva bola
        BallPhysics = newBall;

        // 3. Suscribirse a los eventos de la nueva bola
        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched += HandleBallLaunched;
        }

        // 4. Actualizar el slider y etiquetas correspondientes en la interfaz
        var uiDocument = GetComponent<UIDocument>();
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
                // Configurar la fricción actual de la bola según el slider
                BallPhysics.SetFriction(frictionSlider.value);
            }
        }
    }

    private void HandleBallLaunched()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void HandleStateChanged(GameState newState)
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            var selectionContainer = root.Q<VisualElement>("SelectionContainer");
            var slidersContainer = root.Q<VisualElement>("SlidersContainer");

            if (newState == GameState.BallSelection)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                if (selectionContainer != null) selectionContainer.style.display = DisplayStyle.Flex;
                if (slidersContainer != null) slidersContainer.style.display = DisplayStyle.None;
            }
            else if (newState == GameState.WaitingThrow)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                if (selectionContainer != null) selectionContainer.style.display = DisplayStyle.None;
                if (slidersContainer != null) slidersContainer.style.display = DisplayStyle.Flex;
            }
            else if (newState == GameState.BowlSpinning)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }

        switch (newState)
        {
            case GameState.PauseMenu:
                Debug.Log("Pausa Activada");
                break;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        if (BallPhysics != null)
        {
            BallPhysics.OnBallLaunched -= HandleBallLaunched;
        }
    }
}
