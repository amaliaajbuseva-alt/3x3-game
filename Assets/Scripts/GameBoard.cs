using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject tilePrefab;
    public GameObject[] gemPrefabs;

    private GameObject[,] grid;
    private bool isProcessing = false;
    private Dictionary<GameObject, int> gemTypeDict = new Dictionary<GameObject, int>();
    private UIManager uiManager;

    void Start()
    {
        grid = new GameObject[width, height];
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager не найден!");
        }
        CreateBoard();
    }

    void CreateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = $"Tile_{x}_{y}";

                CreateGemWithoutMatches(x, y);
            }
        }
    }

    void CreateGemWithoutMatches(int x, int y)
    {
        List<int> availableTypes = new List<int>();
        for (int i = 0; i < gemPrefabs.Length; i++)
        {
            availableTypes.Add(i);
        }

        RemoveTypesThatCreateMatches(x, y, ref availableTypes);

        if (availableTypes.Count == 0)
        {
            availableTypes.Add(Random.Range(0, gemPrefabs.Length));
        }

        int gemIndex = availableTypes[Random.Range(0, availableTypes.Count)];
        Vector2 pos = new Vector2(x, y);
        GameObject gem = Instantiate(gemPrefabs[gemIndex], pos, Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = $"Gem_{x}_{y}";
        grid[x, y] = gem;

        gemTypeDict[gem] = gemIndex;
    }

    void RemoveTypesThatCreateMatches(int x, int y, ref List<int> availableTypes)
    {
        if (x >= 2)
        {
            GameObject left1 = grid[x - 1, y];
            GameObject left2 = grid[x - 2, y];

            if (left1 != null && left2 != null && AreSameType(left1, left2))
            {
                if (gemTypeDict.TryGetValue(left1, out int sameType))
                {
                    availableTypes.Remove(sameType);
                }
            }
        }

        if (y >= 2)
        {
            GameObject down1 = grid[x, y - 1];
            GameObject down2 = grid[x, y - 2];

            if (down1 != null && down2 != null && AreSameType(down1, down2))
            {
                if (gemTypeDict.TryGetValue(down1, out int sameType))
                {
                    availableTypes.Remove(sameType);
                }
            }
        }
    }

    void CreateGemAt(int x, int y)
    {
        Vector2 pos = new Vector2(x, y);
        int gemIndex = Random.Range(0, gemPrefabs.Length);
        GameObject gem = Instantiate(gemPrefabs[gemIndex], pos, Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = $"Gem_{x}_{y}";
        grid[x, y] = gem;

        gemTypeDict[gem] = gemIndex;
    }

    public void CheckMatches()
    {
        if (isProcessing) return;
        StartCoroutine(ProcessMatchesCoroutine());
    }

    IEnumerator ProcessMatchesCoroutine()
    {
        isProcessing = true;

        // 1. Находим совпадения
        List<GameObject> matches = FindAllMatches();

        if (matches.Count > 0)
        {
            Debug.Log($"Найдено {matches.Count} совпадений");

            // 2. Уничтожаем совпавшие фишки
            yield return StartCoroutine(DestroyMatches(matches));

            yield return new WaitForSeconds(0.3f);

            // 3. Применяем гравитацию
            yield return StartCoroutine(ApplyGravity());

            yield return new WaitForSeconds(0.3f);

            // 4. Заполняем пустоты
            yield return StartCoroutine(FillEmptySpaces());
        }

        isProcessing = false;
    }

    List<GameObject> FindAllMatches()
    {
        List<GameObject> allMatches = new List<GameObject>();
        HashSet<GameObject> uniqueMatches = new HashSet<GameObject>();

        // Горизонтальные совпадения
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                GameObject gem1 = grid[x, y];
                GameObject gem2 = grid[x + 1, y];
                GameObject gem3 = grid[x + 2, y];

                if (gem1 != null && gem2 != null && gem3 != null)
                {
                    if (AreSameType(gem1, gem2) && AreSameType(gem2, gem3))
                    {
                        AddUniqueMatch(uniqueMatches, gem1);
                        AddUniqueMatch(uniqueMatches, gem2);
                        AddUniqueMatch(uniqueMatches, gem3);

                        for (int i = x + 3; i < width; i++)
                        {
                            GameObject nextGem = grid[i, y];
                            if (nextGem != null && AreSameType(gem1, nextGem))
                            {
                                AddUniqueMatch(uniqueMatches, nextGem);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Вертикальные совпадения
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                GameObject gem1 = grid[x, y];
                GameObject gem2 = grid[x, y + 1];
                GameObject gem3 = grid[x, y + 2];

                if (gem1 != null && gem2 != null && gem3 != null)
                {
                    if (AreSameType(gem1, gem2) && AreSameType(gem2, gem3))
                    {
                        AddUniqueMatch(uniqueMatches, gem1);
                        AddUniqueMatch(uniqueMatches, gem2);
                        AddUniqueMatch(uniqueMatches, gem3);

                        for (int i = y + 3; i < height; i++)
                        {
                            GameObject nextGem = grid[x, i];
                            if (nextGem != null && AreSameType(gem1, nextGem))
                            {
                                AddUniqueMatch(uniqueMatches, nextGem);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        allMatches.AddRange(uniqueMatches);
        return allMatches;
    }

    bool AreSameType(GameObject gem1, GameObject gem2)
    {
        if (gem1 == null || gem2 == null) return false;

        if (gemTypeDict.TryGetValue(gem1, out int type1) &&
            gemTypeDict.TryGetValue(gem2, out int type2))
        {
            return type1 == type2;
        }

        return GetGemTypeFromName(gem1) == GetGemTypeFromName(gem2);
    }

    void AddUniqueMatch(HashSet<GameObject> set, GameObject gem)
    {
        if (gem != null)
        {
            set.Add(gem);
        }
    }

    string GetGemTypeFromName(GameObject gem)
    {
        string name = gem.name;

        if (name.Contains("(Clone)"))
        {
            name = name.Replace("(Clone)", "");
        }

        if (name.Contains("Gem"))
        {
            string[] parts = name.Split('_');
            if (parts.Length > 0)
            {
                string typeName = parts[0];
                if (typeName.StartsWith("Gem"))
                {
                    return typeName.Substring(3);
                }
                return typeName;
            }
        }

        return name;
    }

    public bool WillSwapCreateMatches(int x1, int y1, int x2, int y2)
    {
        // Временно свапаем
        GameObject temp = grid[x1, y1];
        grid[x1, y1] = grid[x2, y2];
        grid[x2, y2] = temp;

        // Проверяем совпадения вокруг обеих позиций
        bool hasMatches = false;

        // Проверяем вокруг первой позиции
        hasMatches = CheckMatchesAround(x1, y1);

        // Если вокруг первой нет, проверяем вокруг второй
        if (!hasMatches)
        {
            hasMatches = CheckMatchesAround(x2, y2);
        }

        // Возвращаем на место
        grid[x2, y2] = grid[x1, y1];
        grid[x1, y1] = temp;

        return hasMatches;
    }

    bool CheckMatchesAround(int x, int y)
    {
        if (grid[x, y] == null) return false;

        // Проверяем горизонтально
        int horizontalCount = 1;

        // Влево
        for (int i = x - 1; i >= 0; i--)
        {
            if (grid[i, y] != null && AreSameType(grid[x, y], grid[i, y]))
                horizontalCount++;
            else
                break;
        }

        // Вправо
        for (int i = x + 1; i < width; i++)
        {
            if (grid[i, y] != null && AreSameType(grid[x, y], grid[i, y]))
                horizontalCount++;
            else
                break;
        }

        if (horizontalCount >= 3) return true;

        // Проверяем вертикально
        int verticalCount = 1;

        // Вниз
        for (int j = y - 1; j >= 0; j--)
        {
            if (grid[x, j] != null && AreSameType(grid[x, y], grid[x, j]))
                verticalCount++;
            else
                break;
        }

        // Вверх
        for (int j = y + 1; j < height; j++)
        {
            if (grid[x, j] != null && AreSameType(grid[x, y], grid[x, j]))
                verticalCount++;
            else
                break;
        }

        return verticalCount >= 3;
    }

    IEnumerator DestroyMatches(List<GameObject> matches)
    {
        Debug.Log($"Уничтожаем {matches.Count} фишек");

        foreach (GameObject gem in matches)
        {
            if (gem != null)
            {
                bool found = false;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (grid[x, y] == gem)
                        {
                            grid[x, y] = null;

                            if (gemTypeDict.ContainsKey(gem))
                            {
                                gemTypeDict.Remove(gem);
                            }

                            Destroy(gem);
                            found = true;
                            Debug.Log($"Уничтожена фишка [{x},{y}]");
                            break;
                        }
                    }
                    if (found) break;
                }
            }
            yield return null;
        }


        if (AudioManager.instance != null && matches.Count > 0)
        {
            AudioManager.instance.PlayMatchSound();
        }

        // ⚠️ ТОЛЬКО ОЧКИ! Ходы списываются в Gem.cs
        if (uiManager != null && matches.Count > 0)
        {
            int baseScore = 100;
            int bonusScore = (matches.Count - 1) * 50;
            int totalScore = baseScore + bonusScore;

            uiManager.AddScore(totalScore);

            Debug.Log($"Начислено {totalScore} очков за {matches.Count} фишек");
        }

        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator ApplyGravity()
    {
        bool moved;
        int passes = 0;

        do
        {
            moved = false;
            passes++;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] == null)
                    {
                        for (int y2 = y + 1; y2 < height; y2++)
                        {
                            if (grid[x, y2] != null)
                            {
                                grid[x, y] = grid[x, y2];
                                grid[x, y2] = null;

                                if (grid[x, y] != null)
                                {
                                    StartCoroutine(AnimateFall(grid[x, y], new Vector2(x, y)));
                                }

                                moved = true;
                                break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);

        } while (moved && passes < 10);

        Debug.Log($"Гравитация завершена за {passes} проходов");
    }

    IEnumerator AnimateFall(GameObject gem, Vector2 targetPos)
    {
        if (gem == null) yield break;

        Vector2 startPos = gem.transform.position;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (gem == null) yield break;

            gem.transform.position = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (gem != null)
        {
            gem.transform.position = targetPos;
        }
    }

    IEnumerator FillEmptySpaces()
    {
        int created = 0;

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null)
                {
                    Vector2 spawnPos = new Vector2(x, height + 2);
                    int gemIndex = Random.Range(0, gemPrefabs.Length);
                    GameObject gem = Instantiate(gemPrefabs[gemIndex], spawnPos, Quaternion.identity);
                    gem.transform.parent = transform;
                    gem.name = $"Gem_{x}_{y}";
                    grid[x, y] = gem;

                    gemTypeDict[gem] = gemIndex;

                    StartCoroutine(AnimateFall(gem, new Vector2(x, y)));

                    created++;
                }
            }
        }

        if (created > 0)
        {
            Debug.Log($"Создано {created} новых фишек");
            yield return new WaitForSeconds(0.5f);

            // ⚠️ УПРОЩЕНИЕ: Проверяем совпадения сразу, без отдельного метода
            List<GameObject> newMatches = FindAllMatches();
            if (newMatches.Count > 0)
            {
                Debug.Log($"Новые совпадения после падения: {newMatches.Count}");
                StartCoroutine(ProcessMatchesCoroutine()); // Рекурсивный вызов
            }
        }
    }

    // Методы для взаимодействия с Gem.cs
    public bool IsValidSwap(int x1, int y1, int x2, int y2)
    {
        if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height) return false;
        if (x2 < 0 || x2 >= width || y2 < 0 || y2 >= height) return false;

        if (grid[x1, y1] == null || grid[x2, y2] == null) return false;

        int dx = Mathf.Abs(x1 - x2);
        int dy = Mathf.Abs(y1 - y2);

        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    public GameObject GetGemAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return grid[x, y];
        return null;
    }

    public void SetGemAt(int x, int y, GameObject gem)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            grid[x, y] = gem;

            if (gem != null && !gemTypeDict.ContainsKey(gem))
            {
                string typeName = GetGemTypeFromName(gem);
                int typeIndex = GetTypeIndexFromName(typeName);
                gemTypeDict[gem] = typeIndex;
            }
        }
    }

    int GetTypeIndexFromName(string typeName)
    {
        for (int i = 0; i < gemPrefabs.Length; i++)
        {
            if (gemPrefabs[i].name.Contains(typeName))
            {
                return i;
            }
        }
        return 0;
    }

    public bool GetGemCoordinates(GameObject gem, out int x, out int y)
    {
        x = -1;
        y = -1;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j] == gem)
                {
                    x = i;
                    y = j;
                    return true;
                }
            }
        }

        return false;
    }
}