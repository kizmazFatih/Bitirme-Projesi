using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Clocks : MonoBehaviour
{
    public static Clocks instance;

    [SerializeField] private TextMeshProUGUI clock_txt;

    private int broken_clocks = 0;
    [SerializeField] private float time;
    private int minute;

    private bool gameOver = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        broken_clocks = 0;
        minute = 0;
    }


    void Update()
    {
        if (gameOver || broken_clocks >= 3) return;

        time -= Time.deltaTime;

        int seconds = (int)time % 60;
        minute = (int)time / 60;

        clock_txt.text = string.Format("{0}:{1:00}", minute, seconds);
    }

    public void AddBrokenClock()
    {
        broken_clocks += 1;
    }
}