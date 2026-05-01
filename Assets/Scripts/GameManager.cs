using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────
//  GAME MANAGER  –  Versión actualizada con sistema de guardado
// ─────────────────────────────────────────────────────────────────
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
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount  = 0;
    }

    void Start()
    {
        // ── Aplica datos de un guardado si venimos de "Continuar" ──
        if (SaveSystem.instance != null && SaveSystem.instance.isLoadingGame)
        {
            SaveData data = SaveSystem.instance.pendingLoad;
            if (data != null)
            {
                // Restaura el tiempo guardado
                elapsedTime = data.elapsedTime;

                // ─── LÓGICA PARA ELIMINAR ENEMIGOS MUERTOS ───
                // AHORA BUSCA EL TAG "damage"
                GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Damage");
                foreach (GameObject enemy in allEnemies)
                {
                    if (data.aliveEnemyIDs != null && !data.aliveEnemyIDs.Contains(enemy.name))
                    {
                        Destroy(enemy);
                    }
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
            if (isPaused) Resume();
            else Pause();
        }
    }

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
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale       = 0f;
        isPaused             = true;
        Cursor.visible       = true;
        Cursor.lockState     = CursorLockMode.None;
    }

    // ─────────────────────────────────────────────────────────────
    //  GUARDAR Y VOLVER AL MENÚ
    // ─────────────────────────────────────────────────────────────
    public void SaveAndQuit()
    {
        if (SaveSystem.instance == null)
        {
            Debug.LogError("❌ No se encontró SaveSystem en la escena. Recuerda iniciar desde el Menú Principal.");
            return;
        }

        PlayerControllerComplete player = FindAnyObjectByType<PlayerControllerComplete>();

        // AHORA BUSCA EL TAG "Damage" PARA GUARDARLOS
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Damage");
        List<string> aliveEnemies = new List<string>();

        foreach (GameObject enemy in enemiesInScene)
        {
            aliveEnemies.Add(enemy.name); 
        }

        if (player != null)
        {
            SaveSystem.instance.Save(
                sceneIndex      : SceneManager.GetActiveScene().buildIndex,
                playerPosition  : player.transform.position,
                health          : player.CurrentHealth, 
                time            : elapsedTime,
                enemies         : aliveEnemies 
            );
        }
        else
        {
            SaveSystem.instance.Save(
                sceneIndex      : SceneManager.GetActiveScene().buildIndex,
                playerPosition  : Vector3.zero,
                health          : 100,
                time            : elapsedTime,
                enemies         : aliveEnemies 
            );
            Debug.LogWarning("⚠️ No se encontró el jugador al guardar. Se guardó con valores por defecto.");
        }

        GoToMainMenu();
    }

    // ─────────────────────────────────────────────────────────────
    //  VOLVER AL MENÚ SIN GUARDAR
    // ─────────────────────────────────────────────────────────────
    public void GoToMainMenu()
    {
        Time.timeScale   = 1f;
        isPaused         = false;
        gameEnded        = false;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    // ─────────────────────────────────────────────────────────────
    //  VICTORIA
    // ─────────────────────────────────────────────────────────────
    public void WinLevel()
    {
        if (gameEnded) return;

        gameEnded    = true;
        isRunning    = false;
        isPaused     = true;
        Time.timeScale = 0f;

        foreach (GameObject hud in hudElements)
            if (hud != null) hud.SetActive(false);

        if (LvlComplete != null)
        {
            LvlComplete.SetActive(true);
            LvlComplete.transform.SetAsLastSibling();
        }

        if (finalTimeText != null)
            finalTimeText.text = "Tiempo: " + GetTimeString();

        SaveSystem.instance?.DeleteSave();

        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // ─────────────────────────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────────────────────────
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

        // Esto detiene el modo Play en el editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}