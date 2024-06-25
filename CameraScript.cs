using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Creamos una variable pública de tipo GameObject que hace referencia a Atrides
    // El GameObject de Atrides es Knight3 dentro del editor
    public GameObject Knight3;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Obtenemos la posición de la cámara usando la función transform
        Vector3 position = transform.position;
        // Establecemos que el valor del eje X de position será igual al valor del eje X de Atrides
        position.x = Knight3.transform.position.x;
        // Obtenemos los valores modificados de position y los actualizamos manualmente
        transform.position = position;
    }
}
