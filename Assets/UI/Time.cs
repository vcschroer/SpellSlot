using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Time : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private TextMeshProUGUI textoRelogio;

    [Header("Configurações do Timer")]
    [SerializeField] private bool comecarAoIniciar = true;

    private float tempoDecorrido = 0f;
    private bool estaRodando = false;

    private void Start()
    {
        if (comecarAoIniciar)
        {
            IniciarRelogio();
        }
    }

    private void Update()
    {
        if (!estaRodando) return;

        tempoDecorrido += UnityEngine.Time.deltaTime;
        AtualizarTextoUI();
    }

    private void AtualizarTextoUI()
    {
        if (textoRelogio == null) return;

        int minutos = Mathf.FloorToInt(tempoDecorrido / 60f);
        int segundos = Mathf.FloorToInt(tempoDecorrido % 60f);

        textoRelogio.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    public void IniciarRelogio()
    {
        estaRodando = true;
    }

    public void PausarRelogio()
    {
        estaRodando = false;
    }

    public void ResetarRelogio()
    {
        tempoDecorrido = 0f;
        AtualizarTextoUI();
    }

    public float ObterTempoEmSegundos()
    {
        return tempoDecorrido;
    }
}