using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [System.Serializable]
    public class DadosInimigo
    {
        public string nome;
        public GameObject prefabInimigo;

        [Tooltip("O peso base de spawn desse inimigo no início do jogo")]
        public float pesoBase = 10f;

        [Tooltip("Se marcado, este inimigo vai aparecer MAIS vezes conforme o tempo passa")]
        public bool escalarComOTempo = false;

        [Tooltip("O quão rápido a presença desse inimigo aumenta por minuto de jogo")]
        public float fatorCrescimentoPorMinuto = 5f;
    }

    [Header("Configurações dos Inimigos")]
    [SerializeField] private List<DadosInimigo> listaInimigos = new List<DadosInimigo>();

    [Header("Ritmo de Spawn (Quantidade)")]
    [SerializeField] private float intervaloSpawnInicial = 3f; 
    [SerializeField] private float intervaloSpawnMinimo = 0.3f; 
    [SerializeField] private float reducaoIntervaloPorMinuto = 0.25f;

    [Header("Distância de Spawn (Fora da Tela)")]
    [SerializeField] private float raioMinimoSpawn = 12f;
    [SerializeField] private float raioMaximoSpawn = 18f;

    private Transform alvoPlayer;
    private float tempoDecorrido = 0f;
    private float cronometroSpawn = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) alvoPlayer = playerObj.transform;
    }

    void Update()
    {
        if (alvoPlayer == null) return;

        tempoDecorrido += Time.deltaTime;
        cronometroSpawn += Time.deltaTime;

        if (cronometroSpawn >= CalcularIntervaloAjustado())
        {
            SpawnarInimigoAleatorio();
            cronometroSpawn = 0f;
        }
    }

    private float CalcularIntervaloAjustado()
    {
        float minutosPassados = tempoDecorrido / 60f;
        float intervaloAtual = intervaloSpawnInicial - (minutosPassados * reducaoIntervaloPorMinuto);
        return Mathf.Max(intervaloAtual, intervaloSpawnMinimo);
    }

    private void SpawnarInimigoAleatorio()
    {
        if (listaInimigos.Count == 0) return;

        float minutosPassados = tempoDecorrido / 60f;
        List<float> pesosAtuais = new List<float>();
        float somaTotalPesos = 0f;

        for (int i = 0; i < listaInimigos.Count; i++)
        {
            float pesoCalculado = listaInimigos[i].pesoBase;

            if (listaInimigos[i].escalarComOTempo)
            {
                pesoCalculado += minutosPassados * listaInimigos[i].fatorCrescimentoPorMinuto;
            }

            pesosAtuais.Add(pesoCalculado);
            somaTotalPesos += pesoCalculado;
        }

        float valorAleatorio = Random.Range(0f, somaTotalPesos);
        float somaAcumulada = 0f;
        GameObject prefabEscolhido = listaInimigos[0].prefabInimigo;

        for (int i = 0; i < pesosAtuais.Count; i++)
        {
            somaAcumulada += pesosAtuais[i];
            if (valorAleatorio <= somaAcumulada)
            {
                prefabEscolhido = listaInimigos[i].prefabInimigo;
                break;
            }
        }

        if (prefabEscolhido != null)
        {
            Vector3 posicaoSpawn = CalcularPosicaoSpawnForaDaTela();
            Instantiate(prefabEscolhido, posicaoSpawn, Quaternion.identity);
        }
    }

    private Vector3 CalcularPosicaoSpawnForaDaTela()
    {
        Vector2 direcaoAleatoria = Random.insideUnitCircle.normalized;
        float distanciaAleatoria = Random.Range(raioMinimoSpawn, raioMaximoSpawn);
        Vector3 deslocamento = new Vector3(direcaoAleatoria.x, direcaoAleatoria.y, 0f) * distanciaAleatoria;
        return alvoPlayer.position + deslocamento;
    }
}
