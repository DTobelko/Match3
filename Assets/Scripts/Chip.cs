
using UnityEngine;

public class Chip
{
    public ChipSO chipSO;
    public int chipType; // тип фишки (от 1-го до кол-ва типов фишек в LevelSO)
    public int x; // координата фишки по горизонтали
    public int y; // по вертикали
    public GameObject GO;

    public Chip(ChipSO chipSO, int chipType, int x, int y)
    {
        this.chipSO = chipSO;
        this.chipType = chipType;
        this.x = x;
        this.y = y;
    }
}
