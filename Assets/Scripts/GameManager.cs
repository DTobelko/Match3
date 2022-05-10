using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [SerializeField] private LevelSO levelSO;
    [SerializeField] private Visual visual;
    [SerializeField] private SoundManager soundManager;

    GameplayManager gameplayManager;
    [SerializeField] InputManager inputManager;


    public enum GameState
    {
        PREGAME,
        IDLE,
        SELECTED,
        PROCESSING
    }

    private int Score;

    public IntScoreEvent ScoreEvent;

    GameState currentGameState;

    private void Awake()
    {
        gameplayManager = new GameplayManager(visual, soundManager);
        inputManager.OnChipSelected += ChipSelected;

        gameplayManager.ScoreEvent.AddListener(ScoreChanged);

        gameplayManager.FieldReady.AddListener(FieldReady);
        gameplayManager.MoveStarted.AddListener(MoveStarted);
        gameplayManager.MoveFinished.AddListener(MoveFinished);

        if (ScoreEvent == null)
            ScoreEvent = new IntScoreEvent();


    }

    void Start()
    {
        currentGameState = GameState.PREGAME;
        Score = 0;

        // инициализация поля
        gameplayManager.CreateField(levelSO);
        gameplayManager.InstantiateChips(levelSO);
    }


    public int GetScore()
    {
        return Score;
    }

    private void SetScore(int score)
    {
        Score = score;
    }


    private void ChipSelected(object sender, int chipIndex)
    {
        if (currentGameState != GameState.PROCESSING)
        {
            currentGameState = GameState.SELECTED;
            gameplayManager.ChipSelected(chipIndex);
        }
    }

    private void ScoreChanged(int score)
    {
        SetScore(score);
        ScoreEvent.Invoke(Score);
    }

    private void FieldReady()
    {
        currentGameState = GameState.IDLE;
        Debug.Log("field ready IDLE");
    }

    private void MoveStarted()
    {
        currentGameState = GameState.PROCESSING;
        Debug.Log("PROCESSING");
    }

    private void MoveFinished()
    {
        currentGameState = GameState.IDLE;
        Debug.Log("Idle");
    }

}
