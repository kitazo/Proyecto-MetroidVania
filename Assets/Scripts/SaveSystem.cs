using UnityEngine;
using System.IO;
<<<<<<< Updated upstream
using System.Collections.Generic; // Obligatorio para usar List<>

// ─────────────────────────────────────────────────────────────────
//  DATOS QUE SE GUARDAN EN EL ARCHIVO
// ─────────────────────────────────────────────────────────────────
[System.Serializable]
public class SaveData
{
    public int   sceneIndex;   // Qué nivel estaba jugando
    public float playerX;      // Posición X del jugador
    public float playerY;      // Posición Y del jugador
    public int   playerHealth; // Vida actual del jugador
    public float elapsedTime;  // Tiempo acumulado en el nivel
    public List<string> aliveEnemyIDs; // NUEVO: Nombres de los enemigos vivos
}

// ─────────────────────────────────────────────────────────────────
//  SAVE SYSTEM  –  Singleton persistente entre escenas
//  Arrastra este script a un GameObject vacío llamado "SaveSystem"
//  en la escena del Menú Principal.
// ─────────────────────────────────────────────────────────────────
=======
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int   sceneIndex;   //Qué nivel estaba jugando
    public float playerX;      //Posición X del jugador
    public float playerY;      //Posición Y del jugador
    public int   playerHealth; //Vida actual del jugador
    public float elapsedTime;  //Tiempo acumulado en el nivel
    public List<string> aliveEnemyIDs; 
}

>>>>>>> Stashed changes
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;

<<<<<<< Updated upstream
    // Datos pendientes de aplicar cuando cargue la escena del juego
=======
    //Datos pendientes de aplicar cuando cargue la escena del juego
>>>>>>> Stashed changes
    [HideInInspector] public SaveData pendingLoad = null;
    [HideInInspector] public bool     isLoadingGame = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    void Awake()
    {
<<<<<<< Updated upstream
        // Patrón Singleton – no se destruye al cambiar de escena
=======
>>>>>>> Stashed changes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

<<<<<<< Updated upstream
    // ── ¿Existe un archivo de guardado? ──────────────────────────
=======
    //Comprueba si existe un archivo guardado
>>>>>>> Stashed changes
    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

<<<<<<< Updated upstream
    // ── Guardar partida (ACTUALIZADO CON ENEMIGOS) ───────────────
=======
    //Guarda la partida
>>>>>>> Stashed changes
    public void Save(int sceneIndex, Vector3 playerPosition, int health, float time, List<string> enemies)
    {
        SaveData data = new SaveData
        {
            sceneIndex    = sceneIndex,
            playerX       = playerPosition.x,
            playerY       = playerPosition.y,
            playerHealth  = health,
            elapsedTime   = time,
<<<<<<< Updated upstream
            aliveEnemyIDs = enemies // Se guarda la lista de enemigos
=======
            aliveEnemyIDs = enemies //Se guarda la lista de enemigos
>>>>>>> Stashed changes
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("✅ Partida y enemigos guardados en: " + SavePath);
    }

<<<<<<< Updated upstream
    // ── Cargar partida ──────────────────────────────────────────
=======
    //Carga la partida guardada
>>>>>>> Stashed changes
    public SaveData Load()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("⚠️ No hay archivo de guardado.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("✅ Partida cargada desde escena: " + data.sceneIndex);
        return data;
    }

<<<<<<< Updated upstream
    // ── Borrar guardado (Nueva Partida) ─────────────────────────
=======
    //Borra la partida guardada
>>>>>>> Stashed changes
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("🗑️ Archivo de guardado eliminado.");
        }
    }
}