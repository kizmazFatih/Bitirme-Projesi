using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clocks : MonoBehaviour
{

    private int broken_clocks = 0;
    private float time = 0;
    private int hour;

    private bool gameOver = false;

    void Start()
    {
        broken_clocks = 0;
        time = 0;
        hour = 0;
    }

    void Update()
    {
        if (gameOver && broken_clocks >= 5) return;

        time += Time.deltaTime * 1;

        if (time >= 60)
        {
            hour++;
            time = 0;
        }
        if (hour >= 10) { gameOver = true; }

    }
}
