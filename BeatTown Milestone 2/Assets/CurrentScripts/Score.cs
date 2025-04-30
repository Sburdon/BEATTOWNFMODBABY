using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [SerializeField]
    public int score = 0;

    [SerializeField]
    private Text scoreText;  // Reference to the UI Text component

    void Start()
    {
        UpdateScoreText();
    }

    void Update()
    {
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score.ToString();
        }
    }
}