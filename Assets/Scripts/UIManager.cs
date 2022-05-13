using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Text ScoreText;

    [SerializeField] private GameObject NotificationPanel;


    private void Start()
    {
         gameManager.ScoreEvent.AddListener(OnScoreChanged);
        gameManager.NoPossibleMove.AddListener(OnNoPossibleMove);
        gameManager.MixForMoveDone.AddListener(OnMixForMoveDone);
    }

    private void OnScoreChanged(int score)
    {
        ScoreText.text = score.ToString();
    }

    private void OnNoPossibleMove()
    {
        NotificationPanel.SetActive(true);
    }


    private void OnMixForMoveDone()
    {
        StartCoroutine(ShowNotification());
    }

    private IEnumerator ShowNotification()
    {
        yield return new WaitForSeconds(1);
        NotificationPanel.SetActive(false);
    }

}
