using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
<<<<<<< Updated upstream

// ─────────────────────────────────────────────────────────────────
//  GAME MANAGER  –  Versión actualizada con sistema de guardado
// ─────────────────────────────────────────────────────────────────
=======
>>>>>>> Stashed changes
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuración de Sistema")]
    public int targetFPS = 60;

    [Header("UI y Menús")]
    public GameObject pauseMenuUI;
    public GameObject LvlComplete;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI finalTimeText;
    public GameObject[] hudElements;

<<<<<<< Updated upstream
    [Header("Estado del Juego")]
    private float elapsedTime = 0f;
    private bool isRunning    = true;
    public static bool isPaused  = false;
    private bool gameEnded       = false;

    // ─── Índice del menú principal en Build Settings ─────────────
    [Header("Navegación")]
    [Tooltip("Índice de la escena del Menú Principal en Build Settings (normalmente 0)")]
    public int mainMenuSceneIndex = 0;

    // ─────────────────────────────────────────────────────────────
=======
    public OptionsManager panelOpciones;    

    [Header("Estado del Juego")]
    private float elapsedTime = 0f;
    private bool isRunning    = true;
    public bool isPaused      = false;
    private bool gameEnded    = false;

    [Header("Navegación")]
    public int mainMenuSceneIndex = 0;

>>>>>>> Stashed changes
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount  = 0;
    }

    void Start()
    {
<<<<<<< Updated upstream
        // ── Aplica datos de un guardado si venimos de "Continuar" ──
=======
        if (panelOpciones != null) panelOpciones.gameObject.SetActive(false);

>>>>>>> Stashed changes
        if (SaveSystem.instance != null && SaveSystem.instance.isLoadingGame)
        {
            SaveData data = SaveSystem.instance.pendingLoad;
            if (data != null)
            {
<<<<<<< Updated upstream
                // Restaura el tiempo guardado
                elapsedTime = data.elapsedTime;

                // ─── LÓGICA PARA ELIMINAR ENEMIGOS MUERTOS ───
                // AHORA BUSCA EL TAG "damage"
=======
                elapsedTime = data.elapsedTime;

>>>>>>> Stashed changes
                GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Damage");
                foreach (GameObject enemy in allEnemies)
                {
                    if (data.aliveEnemyIDs != null && !data.aliveEnemyIDs.Contains(enemy.name))
<<<<<<< Updated upstream
                    {
                        Destroy(enemy);
                    }
=======
                        Destroy(enemy);
>>>>>>> Stashed changes
                }
            }

            SaveSystem.instance.isLoadingGame = false;
        }
    }

    void Update()
    {
        if (gameEnded) return;

        if (isRunning && !isPaused)
        {
            elapsedTime += Time.deltaTime;
            if (timerText != null)
                timerText.text = GetTimeString();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
<<<<<<< Updated upstream
=======
            if (panelOpciones != null && panelOpciones.gameObject.activeSelf)
            {
                CerrarOpciones();
                return;
            }

>>>>>>> Stashed changes
            if (isPaused) Resume();
            else Pause();
        }
    }

<<<<<<< Updated upstream
    // ─────────────────────────────────────────────────────────────
    //  PAUSA / REANUDA
    // ─────────────────────────────────────────────────────────────
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale       = 1f;
        isPaused             = false;
        Cursor.visible       = false;
        Cursor.lockState     = CursorLockMode.Locked;
=======
    public void ModificarTiempo(float cantidad)
    {
        elapsedTime += cantidad;
        
        // Evitamos que el tiempo sea negativo
        if (elapsedTime < 0f)
        {
            elapsedTime = 0f;
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale   = 1f;
        isPaused         = false;
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;
>>>>>>> Stashed changes
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
<<<<<<< Updated upstream
        Time.timeScale       = 0f;
        isPaused             = true;
        Cursor.visible       = true;
        Cursor.lockState     = CursorLockMode.None;
    }

    // ─────────────────────────────────────────────────────────────
    //  GUARDAR Y VOLVER AL MENÚ
    // ─────────────────────────────────────────────────────────────
=======
        Time.timeScale   = 0f;
        isPaused         = true;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void AbrirOpciones()
    {
        if (pauseMenuUI    != null) pauseMenuUI.SetActive(false);
        if (panelOpciones  != null) panelOpciones.OpenOptions();
    }

    public void CerrarOpciones()
    {
        if (panelOpciones  != null) panelOpciones.CloseOptions();
        if (pauseMenuUI    != null) pauseMenuUI.SetActive(true);
    }

>>>>>>> Stashed changes
    public void SaveAndQuit()
    {
        if (SaveSystem.instance == null)
        {
<<<<<<< Updated upstream
            Debug.LogError("❌ No se encontró SaveSystem en la escena. Recuerda iniciar desde el Menú Principal.");
=======
            Debug.LogError("❌ No se encontró SaveSystem en la escena.");
>>>>>>> Stashed changes
            return;
        }

        PlayerControllerComplete player = FindAnyObjectByType<PlayerControllerComplete>();

<<<<<<< Updated upstream
        // AHORA BUSCA EL TAG "Damage" PARA GUARDARLOS
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Damage");
        List<string> aliveEnemies = new List<string>();

        foreach (GameObject enemy in enemiesInScene)
        {
            aliveEnemies.Add(enemy.name); 
        }
=======
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Damage");
        List<string> aliveEnemies   = new List<string>();

        foreach (GameObject enemy in enemiesInScene)
            aliveEnemies.Add(enemy.name);
>>>>>>> Stashed changes

        if (player != null)
        {
            SaveSystem.instance.Save(
<<<<<<< Updated upstream
                sceneIndex      : SceneManager.GetActiveScene().buildIndex,
                playerPosition  : player.transform.position,
                health          : player.CurrentHealth, 
                time            : elapsedTime,
                enemies         : aliveEnemies 
=======
                sceneIndex     : SceneManager.GetActiveScene().buildIndex,
                playerPosition : player.transform.position,
                health         : player.CurrentHealth,
                time           : elapsedTime,
                enemies        : aliveEnemies
>>>>>>> Stashed changes
            );
        }
        else
        {
            SaveSystem.instance.Save(
<<<<<<< Updated upstream
                sceneIndex      : SceneManager.GetActiveScene().buildIndex,
                playerPosition  : Vector3.zero,
                health          : 100,
                time            : elapsedTime,
                enemies         : aliveEnemies 
=======
                sceneIndex     : SceneManager.GetActiveScene().buildIndex,
                playerPosition : Vector3.zero,
                health         : 100,
                time           : elapsedTime,
                enemies        : aliveEnemies
>>>>>>> Stashed changes
            );
            Debug.LogWarning("⚠️ No se encontró el jugador al guardar. Se guardó con valores por defecto.");
        }

        GoToMainMenu();
    }

<<<<<<< Updated upstream
    // ─────────────────────────────────────────────────────────────
    //  VOLVER AL MENÚ SIN GUARDAR
    // ─────────────────────────────────────────────────────────────
=======
>>>>>>> Stashed changes
    public void GoToMainMenu()
    {
        Time.timeScale   = 1f;
        isPaused         = false;
        gameEnded        = false;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

<<<<<<< Updated upstream
    // ─────────────────────────────────────────────────────────────
    //  VICTORIA
    // ─────────────────────────────────────────────────────────────
=======
>>>>>>> Stashed changes
    public void WinLevel()
    {
        if (gameEnded) return;

<<<<<<< Updated upstream
        gameEnded    = true;
        isRunning    = false;
        isPaused     = true;
=======
        gameEnded      = true;
        isRunning      = false;
        isPaused       = true;
>>>>>>> Stashed changes
        Time.timeScale = 0f;

        foreach (GameObject hud in hudElements)
            if (hud != null) hud.SetActive(false);

        if (LvlComplete != null)
        {
            LvlComplete.SetActive(true);
            LvlComplete.transform.SetAsLastSibling();
        }

<<<<<<< Updated upstream
=======
        //Como usamos elapsedTime, el finalTimeText se actualizará con los segundos sumados/restados correctamente
>>>>>>> Stashed changes
        if (finalTimeText != null)
            finalTimeText.text = "Tiempo: " + GetTimeString();

        SaveSystem.instance?.DeleteSave();

        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

<<<<<<< Updated upstream
    // ─────────────────────────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────────────────────────
=======
>>>>>>> Stashed changes
    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float ElapsedTime => elapsedTime;

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused       = false;
        gameEnded      = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Saliendo del juego...");
        Application.Quit();

<<<<<<< Updated upstream
        // Esto detiene el modo Play en el editor de Unity
=======
>>>>>>> Stashed changes
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}