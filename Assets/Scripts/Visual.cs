using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Visual : MonoBehaviour, IVisual
{
    private int gridWidth;
    private int gridHeight;
    [SerializeField] private GameObject gridBackgroudPrefab;

    [SerializeField] private GameObject gridBackgroudText;
    private GameObject GO;

    private GameObject lightning;

    private Grid VisualGrid;
    public float duration = 0.5f;

    public float durationLong = 1f;

    int emptyCount, destroyedCount;
    float xx, yy, zz;

    ChipSO chipSO;
    [SerializeField] private GameObject chipPrefab;
    [SerializeField] private GameObject lightPrefab;

    Chip[] chipsTest;
    LevelSO levelSO;

    public event EventHandler MoveEnded;
    public event EventHandler DestroyEnded;

    public void DisplayField(Grid grid)
    {
        gridWidth = grid.width;
        gridHeight = grid.height;

        VisualGrid = grid;

        Vector3 position = new Vector3();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (!VisualGrid.gridCells[x, y].isHole)
                {
                    position = new Vector3(x * VisualGrid.cellSize, y * VisualGrid.cellSize);

                    Instantiate(gridBackgroudPrefab, position, Quaternion.identity);

                }
            }

        }
    }

    public void DisplayChips(Chip[] chips, LevelSO _levelSO)
    {
        levelSO = _levelSO;
        chipsTest = chips;

        int counter = 0;
        Vector3 position = new Vector3();

        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
            {
                if (!VisualGrid.gridCells[x, y].isHole)
                {
                    position = new Vector3(x * VisualGrid.cellSize, y * VisualGrid.cellSize);

                    chipSO = levelSO.chipList[chips[counter].chipType];

                    chips[counter].GO = Instantiate(chipPrefab, position, Quaternion.identity);

                    chips[counter].GO.name = "Chip" + counter;

                    chips[counter].GO.GetComponent<SpriteRenderer>().sprite = chipSO.sprite;

                    counter++;
                }
            }
    }

    public void DisplayLightning(int x, int y)
    {
        Vector3 position = new Vector3(x * VisualGrid.cellSize, y * VisualGrid.cellSize);
        lightning = Instantiate(lightPrefab, position, Quaternion.identity);
    }

    public void DestroyLightning()
    {
        Destroy(lightning);
    }



    public void SwapChips(Chip[] chips, int highlightedChip, int chipIndex, bool doCheck)
    {
        StopAllCoroutines();
        Vector3 target1Position = chips[highlightedChip].GO.GetComponent<Transform>().position;
        Vector3 target2Position = chips[chipIndex].GO.GetComponent<Transform>().position;
        StartCoroutine(MoveChip(chips[highlightedChip].GO.GetComponent<Transform>(), target2Position, false));
        StartCoroutine(MoveChip(chips[chipIndex].GO.GetComponent<Transform>(), target1Position, doCheck));

        chipsTest = chips;
    }

    /*
    public void Update()
    {
        for (int i = 0; i < chipsTest.Length; i++)
        {
            if (chipsTest[i].GO)
            {
                string s = i.ToString() + "*" + chipsTest[i].chipType.ToString();
                chipsTest[i].GO.GetComponentInChildren<TextMesh>().text = s;


            }
        }
    }*/

    public void MoveChip(Chip[] chips, int chipIndex, int x, int y, bool invoke)  // двигаем фишку chips[chipIndex].y на позицию x, y
    {
        Vector3 targetPosition = new Vector3(x * VisualGrid.cellSize, y * VisualGrid.cellSize);

        StartCoroutine(MoveChip(chips[chipIndex].GO.GetComponent<Transform>(), targetPosition, invoke));      // true нужен только на последей фишке

        chipsTest = chips;
    }


    IEnumerator MoveChip(Transform transform, Vector3 targetPosition, bool invoke)
    {
        float timeElapsed = 0;
        Vector3 startPosition = transform.position;
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        if (invoke) OnMoveEnded(this, EventArgs.Empty);
    }

    protected virtual void OnMoveEnded(Visual v, EventArgs e)
    {
        MoveEnded?.Invoke(this, e);
    }

    protected virtual void OnDestroyEnded(Visual v, EventArgs e)
    {
        DestroyEnded?.Invoke(this, e);
    }


    public void DestroyChips(Chip[] chips)
    {
        emptyCount = 0;
        destroyedCount = 0;

        for (int i = 0; i < chips.Length; i++)
            if (chips[i].chipType == -1)
            {
                emptyCount++;
            }


        for (int i = 0; i < chips.Length; i++)
        {
            if (chips[i].chipType == -1)
            {
                StartCoroutine(HideChip(chips, i));
            }
        }

        chipsTest = chips;
    }


    IEnumerator HideChip(Chip[] chips, int i)
    {
        xx = chips[i].GO.GetComponent<Transform>().localScale.x;
        yy = chips[i].GO.GetComponent<Transform>().localScale.y;
        zz = chips[i].GO.GetComponent<Transform>().localScale.z;

        float timeElapsed = 0;
        while (timeElapsed < durationLong)
        {
            chips[i].GO.GetComponent<Transform>().localScale = Vector3.Lerp(new Vector3(xx, yy, zz), Vector3.zero, timeElapsed / durationLong);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
          chips[i].GO.GetComponent<SpriteRenderer>().enabled = false;
          chips[i].GO.GetComponent<BoxCollider2D>().enabled = false;
        destroyedCount++;

        if (destroyedCount == emptyCount)
            OnDestroyEnded(this, EventArgs.Empty);
    }


    public void AppearChip(Chip[] chips, int chipIndex, int x, int y)
    {
        Vector3 position = new Vector3(x * VisualGrid.cellSize, y * VisualGrid.cellSize);

        chipSO = levelSO.chipList[chips[chipIndex].chipType];

        chips[chipIndex].GO.GetComponent<SpriteRenderer>().enabled = true;
        chips[chipIndex].GO.GetComponent<BoxCollider2D>().enabled = true;

        chips[chipIndex].GO.GetComponent<SpriteRenderer>().sprite = chipSO.sprite;

        chips[chipIndex].GO.GetComponent<Transform>().localScale = new Vector3(xx, yy, zz);

        chips[chipIndex].GO.GetComponent<Transform>().position = position;
    }

}
