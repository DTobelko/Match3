using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class IntScoreEvent : UnityEvent<int>
{
}


public class GameplayManager
{

    private int gridWidth;
    private int gridHeight;
    private int holesCount;
    private float cellSize;
    private int chipsCount;
    private Grid grid;  

    private int highlightedChip = -1;

    private Chip[] chips;

    Queue<Chip> newChips = new Queue<Chip>(); // очередь фишек, подлежащих восстановлению

    IVisual vis;
    SoundManager soundManager;
    int chipToSwapOne, chipToSwapTwo;
    bool[,] mark;
    LevelSO levelSO;
    bool allChipsHere = true;
    bool doSwap = true;
    bool fieldReady = false;

    private int Score;

    public IntScoreEvent ScoreEvent;
    public UnityEvent FieldReady, MoveStarted, MoveFinished;


    public GameplayManager(IVisual _vis, SoundManager _soundManager)
    {
        vis = _vis;
        vis.MoveEnded += OnMoveEnded;
        vis.DestroyEnded += OnDestroyEnded;

        soundManager = _soundManager;

        ScoreEvent = new IntScoreEvent();
        FieldReady = new UnityEvent();
        MoveStarted = new UnityEvent();
        MoveFinished = new UnityEvent();
        
    }

    public void CreateField(LevelSO levelSO)
    {
        gridWidth = levelSO.width;
        gridHeight = levelSO.height;
        holesCount = levelSO.holesCount;
        cellSize = levelSO.cellSize;

        grid = new Grid();

        grid.height = gridHeight;
        grid.width = gridWidth;
        grid.cellSize = cellSize;
        grid.gridCells = new Cell[gridWidth, gridHeight];

        // определяем дырки в поле случайным образом
        List<int> holes = new List<int>(holesCount);

        for (int i = 0; i < holesCount; i++)
        {
            bool filled = false;
            while (!filled)
            {
                int rand = UnityEngine.Random.Range(0, gridWidth * gridHeight);
                if (!holes.Contains(rand))
                {
                    holes.Add(rand);
                    filled = true;
                }
            }
        }
        holes.Sort();

        // инициализируем поле
        int counter = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid.gridCells[x, y] = new Cell(true, holes.Contains(counter));
                counter++;
            }
        }

        vis.DisplayField(grid);
    }

    public void InstantiateChips(LevelSO _levelSO)
    {
        // разместить на поле фишки заданных цветов случайным образом, без трёх в ряд и с возможностью хода, не занимая дырки
        // массив фишек - это массив, похожий на уже меющийся Grid, но вместо ячейки - подвижная фишка

        // одномерный массив фишек
        // создаём массив фишек
        levelSO = _levelSO;

        chipsCount = gridWidth * gridHeight - holesCount;
        chips = new Chip[chipsCount];

        for (int i = 0; i < chipsCount; i++)
        {
            int chipType = UnityEngine.Random.Range(0, levelSO.chipList.Count);
            chips[i] = new Chip(levelSO.chipList[chipType], chipType, 0, 0);
        }

        // проверка на матч-3 и на наличие ходов
        do
        {
            MixChips();
            LocateChips();
        } while (CheckMatch());

        fieldReady = true;

        vis.DisplayChips(chips, levelSO);

        /*  if (CanMove())
                Debug.Log("есть ход");
            else
                Debug.Log("нет хода");*/


        FieldReady?.Invoke();

    }

    bool CheckMatch()
    {
        int matches = 0;
        mark = new bool[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
            {
                matches += CheckLine(x, y, 1, 0);
                matches += CheckLine(x, y, 0, 1);
            }

        if (matches > 0)
            return true;

        return false;
    }

    void MarkChips()
    {
       
        for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    if (mark[x, y])
                    {
                        grid.gridCells[x, y].chipType = -1; // удаляем

                    }
                }

            for (int i = 0; i < chips.Length; i++)
            {

                if (grid.gridCells[chips[i].x, chips[i].y].chipType == -1)
                {
                    chips[i].chipType = -1;

                }
            }
            doSwap = false;

    }


    int CheckLine(int x0, int y0, int sx, int sy)
    {
        int chipType = grid.gridCells[x0, y0].chipType;

        if (chipType == -1) return 0;

        int countMatches = 0;

        for (int x = x0, y = y0; GetCell(x, y) == chipType; x += sx, y += sy) // считаем, сколько фишек того же типа стоят в ряд
        {
            countMatches++;
        } 

        if (countMatches < 3)
            return 0;

        if (fieldReady) AddScore(countMatches);

        // пометим совпавшиее фишки
        for (int x = x0, y = y0; GetCell(x, y) == chipType; x += sx, y += sy) 
        {
            mark[x, y] = true;
        }

        return countMatches;
    }

    private int GetCell(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return -1;

        if (grid.gridCells[x, y].isHole)
            return -1;

        return grid.gridCells[x, y].chipType;
    }


    void AddScore(int countMatches)
    {
        if (countMatches == 5)
        {
            Score += levelSO.Match5Score;
            Score -= levelSO.Match4Score;
            Score -= levelSO.Match3Score;
        }
        else if (countMatches == 4)
        {
            Score += levelSO.Match4Score;
            Score -= levelSO.Match3Score;
        }
        else if (countMatches == 3)
        {
            Score += levelSO.Match3Score;
        }

        ScoreEvent?.Invoke(Score);

    }


    private void MixChips()
    {
        for (int i = chips.Length - 1; i >= 1; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = chips[j];
            chips[j] = chips[i];
            chips[i] = temp;
        }
    }


    private void LocateChips()
    {
        int counter = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (!grid.gridCells[x, y].isHole)
                {
                    chips[counter].x = x;
                    chips[counter].y = y;
                    grid.gridCells[x, y].chipType = chips[counter].chipType;
                    grid.gridCells[x, y].chipInd = counter;
                    counter++;
                }
            }

        }
    }


    // есть ли ход
    private bool CanMove()
    {
        int abilities = 0;
        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
            {
                abilities += CheckLineHasMоve(x, y, 1, 0);
                abilities += CheckLineHasMоve(x, y, 0, 1);
            }

        if (abilities > 0)
            return true;

        return false;
    }

    int CheckLineHasMоve(int x0, int y0, int sx, int sy)
    {
        int chipType = GetCell(x0, y0);
        if (chipType == -1) return 0;

        int countMatches = 0;

        for (int x = x0, y = y0; GetCell(x, y) == chipType; x += sx, y += sy) // считаем, сколько фишек того же типа стоят в ряд
        {
            countMatches++;
        }

        if (countMatches == 2) // нашли две фишки одного типа друг за другом, далее проверить, есть ли фишка через одну до/ после них, либо на соседних строках/столбцах для возможного хода
        {
            if (GetCell(x0 + 3, y0) == chipType)
                return 1;
        }



        return 0;
    }



    public void ChipSelected(int chipIndex)
    {
        doSwap = true; 

        if (highlightedChip == chipIndex)
            return;

        if (highlightedChip == -1)
        {
            highlightedChip = chipIndex;
            vis.DisplayLightning(chips[chipIndex].x, chips[chipIndex].y);
            return;
        }

        if (highlightedChip != chipIndex)
        {
            vis.DestroyLightning();
        }

        // если тап по нужному месту - передвигаем фишки
        if ( ( chips[chipIndex].x == chips[highlightedChip].x && Math.Abs(chips[chipIndex].y - chips[highlightedChip].y) == 1 )
            ||
            (chips[chipIndex].y == chips[highlightedChip].y && Math.Abs(chips[chipIndex].x - chips[highlightedChip].x) == 1)
            )
        {
            MoveStarted?.Invoke();
            SwapChips(highlightedChip, chipIndex, true);
            soundManager.PlaySoundEffect(SoundEffect.ChipSwap);
            chipToSwapOne = highlightedChip;
            chipToSwapTwo = chipIndex;
            vis.DestroyLightning();
           
            highlightedChip = -1;
        }

        else 
        {
            // если тап не по нужному месту - подсветить новую фишку
            highlightedChip = chipIndex;
            vis.DisplayLightning(chips[chipIndex].x,  chips[chipIndex].y);
        }
    }


    void SwapChips(int highlightedChip, int chipIndex, bool doCheck)
    {
        int x = chips[highlightedChip].x;
        int y = chips[highlightedChip].y;
        chips[highlightedChip].x = chips[chipIndex].x;
        chips[highlightedChip].y = chips[chipIndex].y;
        chips[chipIndex].x = x;
        chips[chipIndex].y = y;

        grid.gridCells[chips[highlightedChip].x, chips[highlightedChip].y].chipType = chips[highlightedChip].chipType;
        grid.gridCells[chips[chipIndex].x, chips[chipIndex].y].chipType = chips[chipIndex].chipType;

        grid.gridCells[chips[highlightedChip].x, chips[highlightedChip].y].chipInd = highlightedChip;
        grid.gridCells[chips[chipIndex].x, chips[chipIndex].y].chipInd = chipIndex;

        vis.SwapChips(chips, highlightedChip, chipIndex, doCheck);
    }

    void OnMoveEnded(object sender, EventArgs e)        
    {
        if (allChipsHere && CheckMatch() )
        // удалить матч 3, начислить очки, вывести новые фишки 
        {
            MarkChips();
            soundManager.PlaySoundEffect(SoundEffect.ChipDisappear);

            vis.DestroyChips(chips);
        }
        else if (doSwap)
        {
            // здесь запустить свап без последующей проверки
            SwapChips(chipToSwapOne, chipToSwapTwo, false);
            MoveFinished?.Invoke();
        }
        else
        {
            MoveFinished?.Invoke();
        }
    }


    void OnDestroyEnded(object sender, EventArgs e)
    {
        allChipsHere = false;

        ReloadDisabledChips();

        MoveChipsUp();
    
    }



    void MoveChipsUp() // имещиеся фишки сдвигаются вверх
    {
        for (int x = 0; x < gridWidth; x++)
        {
            // первый цикл - обсчитываем новые позиции фишек
            for (int y = gridHeight - 1; y >= 0; y--)
            {

                if (grid.gridCells[x, y].chipType == -1 && !grid.gridCells[x, y].isHole) 
                {
                    // пробуем подвинуть фишки, которые ниже
                    for (int i = y - 1; i >= 0; i--)
                    {
                        // ищем, какую фишку можем подвинуть
                        if (grid.gridCells[x, i].chipType != -1 && !grid.gridCells[x, i].isHole)
                        {
                            int chipIndex = grid.gridCells[x, i].chipInd;

                            MoveChip(chipIndex, x, y);
                           
                            break;
                        }
                    }
                }
            }

            // второй цикл - сдвигаем все фишки визуально, в том числе неактивные - новые
            // как найти неактивные новые фишки
            for (int y = gridHeight - 1; y >= 0; y--)
            {
                // сдвигаем одновременно новые фишки на пустые места и затем все фишки визуально
                if (grid.gridCells[x, y].chipType != -1 && !grid.gridCells[x, y].isHole) // если она уже заполнена - визуализируем
                    vis.MoveChip(chips, grid.gridCells[x, y].chipInd, x, y, false);
                else
                {
                    if (!grid.gridCells[x, y].isHole)
                    {
                        // пока ещё пустую - заполняем из очереди
                        // взять из очереди
                        var tempChip = newChips.Dequeue();
                        int tempInd = GetChipIndex(tempChip);

                        bool thatIsAll = newChips.Count == 0 ? true : false;

                        MoveChip(tempInd, x, y);

                        // а затем визуализировать

                        vis.MoveChip(chips, grid.gridCells[x, y].chipInd, x, y, thatIsAll);
                    }
                }

                if (x == gridWidth - 1 && y == 0)
                {
                    allChipsHere = true;
                }
            }
        }
    }


    int GetChipIndex(Chip chip)
    {
        for (int i = 0; i < chips.Length; i++)
            if (chip.x == chips[i].x && chip.y==chips[i].y && chip.chipType == chips[i].chipType)
                return i;

        return -1;
    }

    void MoveChip(int chipIndex, int x, int y)   // двигаем фишку chipInd на координаты x, y
    {

        if (chips[chipIndex].y >= 0)
        {
            grid.gridCells[chips[chipIndex].x, chips[chipIndex].y].chipType = -1; // освобождаем ячейку, в которой фишка БЫЛА, если только это не новая фишка
        }

        chips[chipIndex].x = x;
        chips[chipIndex].y = y;

        grid.gridCells[x, y].chipType = chips[chipIndex].chipType;
        grid.gridCells[x, y].chipInd = chipIndex;

    }




    void AppearChip(int chipIndex, int x, int y) // появление 
    {
        soundManager.PlaySoundEffect(SoundEffect.ChipAppear);

        int chipType = UnityEngine.Random.Range(0, levelSO.chipList.Count);

        chips[chipIndex].chipType = chipType;

        vis.AppearChip(chips, chipIndex, x, y);

        chips[chipIndex].y = y;

        newChips.Enqueue(chips[chipIndex]);
    }


    void ReloadDisabledChips() // фишкам, кооторые удалены с поля, назначим новые координаты для дальнейшего респавна
    {

        for (int x = 0; x < gridWidth; x++)
        {
            int count = 0;
            for (int y = 0; y < gridHeight; y++)

            {
                if (grid.gridCells[x, y].chipType == -1)
                {
                    count--;
                    int chipIndex = grid.gridCells[x, y].chipInd;

                    // снести координаты фишки вниз
                    chips[chipIndex].y = count; // убранные фишки размещаем друг за другом внизу матрицы

                    AppearChip(chipIndex, x, chips[chipIndex].y);
                }
            }

        }
    }


}


