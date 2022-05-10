using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Text ScoreText;


    private void Awake()
    {
 
        gameManager.ScoreEvent.AddListener(OnScoreChanged);

    }

    private void OnScoreChanged(int score)
    {
        ScoreText.text = score.ToString();
    }


}
