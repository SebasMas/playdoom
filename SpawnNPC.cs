using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public GameObject crowPrefabOriginal;
    private GameObject crowPrefab;
    private BoxCollider2D spawnArea;
    public float respawnDelay = 5f;
    public int maxCrows = 5;
    public Tilemap groundTilemap;
    public float spawnHeightOffset = 0.5f;

    private List<CrowBehaviour> crows = new List<CrowBehaviour>();

    void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        if (spawnArea == null)
        {
            Debug.LogError("No se encontró BoxCollider2D en el SpawnManager");
        }

        if (crowPrefabOriginal != null)
        {
            crowPrefab = Instantiate(crowPrefabOriginal);
            crowPrefab.SetActive(false);
            DontDestroyOnLoad(crowPrefab);
        }
        else
        {
            Debug.LogError("Crow Prefab Original no está asignado en el Inspector");
        }

        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap no está asignado en el Inspector");
        }
    }

    void Start()
    {
        for (int i = 0; i < maxCrows; i++)
        {
            SpawnCrow();
        }
    }

    public void CrowDied(CrowBehaviour crow)
    {
        if (crow != null && crows.Contains(crow))
        {
            crows.Remove(crow);
            StartCoroutine(RespawnCrowAfterDelay(respawnDelay));
        }
    }

    IEnumerator RespawnCrowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            SpawnCrow();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al intentar hacer respawn de Crow: " + e.Message);
        }
    }

    void SpawnCrow()
    {
        if (crows.Count >= maxCrows)
        {
            Debug.Log("Número máximo de Crows alcanzado");
            return;
        }

        if (crowPrefab == null)
        {
            Debug.LogError("El prefab de Crow no está asignado o ha sido destruido");
            return;
        }

        Vector2 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition != Vector2.negativeInfinity)
        {
            GameObject newCrow = Instantiate(crowPrefab, spawnPosition, Quaternion.identity);
            newCrow.SetActive(true);
            CrowBehaviour crowBehaviour = newCrow.GetComponent<CrowBehaviour>();
            if (crowBehaviour != null)
            {
                crowBehaviour.spawnManager = this;
                crows.Add(crowBehaviour);
            }
            else
            {
                Debug.LogError("El prefab de Crow no tiene el componente CrowBehaviour");
                Destroy(newCrow);
            }
        }
        else
        {
            Debug.LogWarning("No se pudo encontrar una posición válida para spawnar el Crow");
        }
    }

    Vector2 GetValidSpawnPosition()
    {
        for (int i = 0; i < 30; i++) // Intenta 30 veces encontrar una posición válida
        {
            Vector2 randomPosition = GetRandomPositionInSpawnArea();
            Vector3Int cellPosition = groundTilemap.WorldToCell(randomPosition);
            
            // Verifica si hay un tile en la posición y si hay espacio libre arriba
            if (groundTilemap.HasTile(cellPosition) && !groundTilemap.HasTile(cellPosition + Vector3Int.up))
            {
                // Ajusta la posición para que esté justo encima del tile
                Vector3 worldPosition = groundTilemap.GetCellCenterWorld(cellPosition);
                return new Vector2(worldPosition.x, worldPosition.y + spawnHeightOffset);
            }
        }
        return Vector2.negativeInfinity; // Retorna esto si no se encuentra una posición válida
    }

    Vector2 GetRandomPositionInSpawnArea()
    {
        if (spawnArea == null)
        {
            Debug.LogError("SpawnArea no está asignada");
            return Vector2.zero;
        }

        Vector2 spawnSize = spawnArea.size;
        Vector2 spawnCenter = spawnArea.transform.position;

        float randomX = Random.Range(-spawnSize.x / 2, spawnSize.x / 2);
        float randomY = Random.Range(-spawnSize.y / 2, spawnSize.y / 2);

        return spawnCenter + new Vector2(randomX, randomY);
    }
}