using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{

    public List<ChipSO> chipList;
    public int width;
    public int height;

    public int holesCount;

    public float cellSize;

    public int Match3Score; // сколько очков начисляем за 3 фишки
    public int Match4Score; // за 4
    public int Match5Score; // за 5

}