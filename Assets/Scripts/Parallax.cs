using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Sensibilidad")]
    [Tooltip("0.01 es muy lento (fondo), 0.1 es rápido (cerca)")]
    public float parallaxMultiplier = 0.02f; 

    private Material parallaxMaterial;
    private Transform camTransform;
    private Vector3 lastCamPosition;

    void Start()
    {
        // 1. Buscamos el componente Renderer del Quad
        parallaxMaterial = GetComponent<Renderer>().material;
        
        // 2. Buscamos la cámara principal automáticamente
        camTransform = Camera.main.transform;
        lastCamPosition = camTransform.position;
    }

    void LateUpdate()
    {
        // 3. Calculamos cuánto se desplazó la CÁMARA en este frame
        float deltaX = camTransform.position.x - lastCamPosition.x;

        // 4. Movemos la textura una fracción de ese movimiento
        // Si sigue muy rápido, baja el multiplicador en el Inspector a 0.005
        parallaxMaterial.mainTextureOffset += new Vector2(deltaX * parallaxMultiplier, 0);

        // 5. Actualizamos la posición para el siguiente cálculo
        lastCamPosition = camTransform.position;
    }
}