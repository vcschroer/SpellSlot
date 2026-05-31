using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Configurações Gerais")]
    [SerializeField] private List<DadosInimigo> listaInimigos = new List<DadosInimigo>();
    [Tooltip("Tempo em segundos para encerrar o spawn (ex: 300 = 5 minutos)")]
    [SerializeField] private float tempoLimiteSpawnSegundos = 300f;
    [SerializeField] private string nomeCenaVitoria = "Win";

    [Header("Ritmo de Spawn (Intervalo)")]
    [SerializeField] private float intervaloSpawnInicial = 3f;
    [SerializeField] private float intervaloSpawnMinimo = 0.3f;
    [SerializeField] private float reducaoIntervaloPorMinuto = 0.25f;

    [Header("Ritmo de Spawn (Quantidade)")]
    [SerializeField] private int quantidadeInicial = 1;
    [SerializeField] private int quantidadeMaxima = 10;
    [SerializeField] private float aumentoQuantidadePorMinuto = 0.5f;

    [Header("Área de Spawn")]
    [SerializeField] private PolygonCollider2D areaDeSpawn;
    [SerializeField] private float raioMinimoSpawn = 12f;
    [SerializeField] private float raioMaximoSpawn = 18f;

    private Transform alvoPlayer;
    private float tempoDecorrido = 0f;
    private float cronometroSpawn = 0f;
    private bool spawnEncerrado = false;
    private float proximaChecagemVitoria = 0f;

    private bool vitoriaDisparada = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) alvoPlayer = playerObj.transform;
    }

    void Update()
    {
        if (alvoPlayer == null || vitoriaDisparada) return;

        tempoDecorrido += Time.deltaTime;

        if (tempoDecorrido >= tempoLimiteSpawnSegundos)
        {
            spawnEncerrado = true;

            if (Time.time >= proximaChecagemVitoria)
            {
                VerificarVitoria();
                proximaChecagemVitoria = Time.time + 1f;
            }
            return;
        }

        cronometroSpawn += Time.deltaTime;
        if (cronometroSpawn >= CalcularIntervaloAjustado())
        {
            int qtdParaSpawnar = CalcularQuantidadeAtual();
            for (int i = 0; i < qtdParaSpawnar; i++)
            {
                SpawnarInimigoAleatorio();
            }
            cronometroSpawn = 0f;
        }
    }

    private void VerificarVitoria()
    {
        if (vitoriaDisparada) return;

        GameObject[] inimigosRestantes = GameObject.FindGameObjectsWithTag("Enemy");

        if (inimigosRestantes.Length == 0)
        {
            vitoriaDisparada = true; 

            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.CarregarCena(nomeCenaVitoria);
            }
            else
            {
                SceneManager.LoadScene(nomeCenaVitoria);
            }
        }
    }

    private float CalcularIntervaloAjustado()
    {
        float minutosPassados = tempoDecorrido / 60f;
        float intervaloAtual = intervaloSpawnInicial - (minutosPassados * reducaoIntervaloPorMinuto);
        return Mathf.Max(intervaloAtual, intervaloSpawnMinimo);
    }

    private int CalcularQuantidadeAtual()
    {
        float minutosPassados = tempoDecorrido / 60f;
        int quantidadeCalculada = quantidadeInicial + Mathf.FloorToInt(minutosPassados * aumentoQuantidadePorMinuto);
        return Mathf.Clamp(quantidadeCalculada, quantidadeInicial, quantidadeMaxima);
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
                pesoCalculado += minutosPassados * listaInimigos[i].fatorCrescimentoPorMinuto;

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
            Vector3 posicaoSpawn = ObterPosicaoSpawn();
            Instantiate(prefabEscolhido, posicaoSpawn, Quaternion.identity);
        }
    }

    private Vector3 ObterPosicaoSpawn()
    {
        if (areaDeSpawn != null)
        {
            Bounds bounds = areaDeSpawn.bounds;
            Vector2 pontoAleatorio;
            int tentativas = 0;

            do
            {
                pontoAleatorio = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
                tentativas++;
            }
            while (!areaDeSpawn.OverlapPoint(pontoAleatorio) && tentativas < 50);

            return new Vector3(pontoAleatorio.x, pontoAleatorio.y, 0f);
        }

        Vector2 direcaoAleatoria = Random.insideUnitCircle.normalized;
        float distanciaAleatoria = Random.Range(raioMinimoSpawn, raioMaximoSpawn);
        return alvoPlayer.position + (Vector3)(direcaoAleatoria * distanciaAleatoria);
    }
}