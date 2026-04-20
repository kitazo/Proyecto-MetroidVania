using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuración de Sistema")]
    public int targetFPS = 60;

    [Header("UI y Menús")]
    public GameObject pauseMenuUI; 
    public GameObject LvlComplete; 
    public TextMeshProUGUI timerText; // El texto que corre durante el juego
    public TextMeshProUGUI finalTimeText; // El texto que muestra el tiempo final al ganar
    public GameObject[] hudElements; // Lista de cosas que se quiere ocultar al ganar, Barra de vida, etc.

    [Header("Estado del Juego")]
    private float elapsedTime = 0f;
    private bool isRunning = true;
    public static bool isPaused = false;
    private bool gameEnded = false; // Nueva variable para evitar el bug de Escape

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        // Si el juego ya terminó, no permitimos pausar ni reanudar con Escape
        if (gameEnded) return; 

        // Lógica del Timer Antes en Timer.cs
        if (isRunning && !isPaused)
        {
            elapsedTime += Time.deltaTime;
            if (timerText != null) 
                timerText.text = GetTimeString();
        }

        // Presiona Escape para pausar/reanudar Antes en PauseMenu.cs
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;   // Reanuda el tiempo del juego
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;   // Congela el tiempo del juego
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void WinLevel()
    {
        if (gameEnded) return; // Evita que se ejecute dos veces
        
        gameEnded = true; // Bloquea el Update para que Escape no funcione
        isRunning = false;
        isPaused = true; 
        Time.timeScale = 0f; 

        // 1. Oculta el HUD
        foreach (GameObject hud in hudElements)
        {
            if (hud != null) hud.SetActive(false);
        }

        // 2. Muestra la victoria y traer el panel al frente
        if (LvlComplete != null)
        {
            LvlComplete.SetActive(true);
            LvlComplete.transform.SetAsLastSibling(); 
        }

        if (finalTimeText != null)
        {
            finalTimeText.text = "Tiempo: " + GetTimeString();
        }

        // 3. Activa el cursor para poder clickear Reset o Salir
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}