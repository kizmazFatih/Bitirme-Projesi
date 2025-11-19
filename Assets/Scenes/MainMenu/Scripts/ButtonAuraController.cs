using UnityEngine;
using System.Collections;

public class ButtonAuraController : MonoBehaviour
{
    public ParticleSystem aura;

    void Awake()
    {
        if (!aura) aura = GetComponent<ParticleSystem>();
        if (aura) aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // Belirtilen gecikmeden sonra sürekli çalıştırır (Stop çağrısı yok).
    public void PlayAfterDelay(float delaySeconds = 0f)
    {
        StartCoroutine(PlayCo(delaySeconds));
    }

    IEnumerator PlayCo(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (aura)
        {
            // Sonsuz çalışması için: Looping = ON, Emission Rate açık olmalı
            aura.Play();
        }
    }

    // İstersen manuel durdurmak için çağırabileceğin opsiyonel metod:
    public void StopSoft()
    {
        if (aura) aura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}