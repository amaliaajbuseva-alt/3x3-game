using UnityEngine;
using System.Collections;

public class Gem : MonoBehaviour
{
    private Vector3 startMousePos;
    private GameBoard board;
    private bool isDragging = false;
    private bool isSwapping = false;
    private UIManager uiManager;

    void Start()
    {
        board = FindObjectOfType<GameBoard>();
        uiManager = FindObjectOfType<UIManager>();
    }

    void OnMouseDown()
    {
        if (isSwapping) return;

        startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    void OnMouseUp()
    {
        if (!isDragging || isSwapping) return;
        isDragging = false;

        Vector3 endMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dragDirection = endMousePos - startMousePos;

        // Минимальная дистанция для свапа
        if (dragDirection.magnitude < 0.3f) return;

        // Получаем наши координаты
        if (!board.GetGemCoordinates(gameObject, out int myX, out int myY))
        {
            Debug.LogError("Не удалось найти координаты фишки!");
            return;
        }

        // Определяем направление свапа
        int targetX = myX;
        int targetY = myY;

        if (Mathf.Abs(dragDirection.x) > Mathf.Abs(dragDirection.y))
        {
            // Горизонтальный свап
            targetX += (dragDirection.x > 0) ? 1 : -1;
        }
        else
        {
            // Вертикальный свап
            targetY += (dragDirection.y > 0) ? 1 : -1;
        }

        // Проверяем валидность свапа
        if (board.IsValidSwap(myX, myY, targetX, targetY))
        {
            GameObject targetGem = board.GetGemAt(targetX, targetY);
            if (targetGem != null)
            {
                StartCoroutine(SwapAnimation(targetGem, myX, myY, targetX, targetY));
            }
        }
    }

    IEnumerator SwapAnimation(GameObject otherGem, int x1, int y1, int x2, int y2)
    {
        isSwapping = true;

        // Получаем компонент Gem у другой фишки
        Gem otherGemScript = otherGem.GetComponent<Gem>();
        if (otherGemScript != null)
        {
            otherGemScript.isSwapping = true;
        }

        // Сохраняем позиции
        Vector3 myPos = transform.position;
        Vector3 otherPos = otherGem.transform.position;

        // Анимация свапа
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(myPos, otherPos, elapsed / duration);
            otherGem.transform.position = Vector3.Lerp(otherPos, myPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Финальные позиции
        transform.position = otherPos;
        otherGem.transform.position = myPos;

        // Обновляем grid
        board.SetGemAt(x1, y1, otherGem);
        board.SetGemAt(x2, y2, gameObject);

        // Ждём немного перед проверкой
        yield return new WaitForSeconds(0.1f);

        // Проверяем совпадения
        board.CheckMatches();

        // ⚠️ ВАЖНО: Ждём пока вся цепная реакция завершится
        yield return new WaitForSeconds(0.5f);

        // ⚠️ ТОЛЬКО ТЕПЕРЬ списываем ход (после всей обработки)
        if (uiManager != null)
        {
            uiManager.UseMove();
            Debug.Log("Ход использован! (после проверки совпадений)");
        }


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySwapSound();
        }

        // Сбрасываем флаги
        isSwapping = false;
        if (otherGemScript != null)
        {
            otherGemScript.isSwapping = false;
        }
    }
}