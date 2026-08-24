using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitaWeapon : BaseWeapon
{
    [Header("Configurações do Prefab")]
    [SerializeField] private GameObject prefabProjetil;

    [Header("Filtro de Inimigos")]
    [SerializeField] private LayerMask layerInimigos;

    [Header("Status Modificáveis da Órbita")]
    [SerializeField] private int dano = 10;
    [SerializeField] private float raioProjetil = 0.5f;
    [SerializeField] private int quantidadeProjeteis = 3;
    [SerializeField] private float velocidadeGiro = 180f;
    [SerializeField] private float raioOrbita = 2.5f;
    [SerializeField] private float tamanhoProjetil = 1f;

    [Header("Posicionamento")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);

    [Header("Configurações de Jackpot")]
    [SerializeField] private float duracaoJackpot = 5f;
    [SerializeField] private float multiplicadorVelocidadeJackpot = 3f;

    private List<GameObject> projeteisAtivos = new List<GameObject>();
    private float anguloAtual = 0f;

    public int QuantidadeProjeteis
    {
        get => quantidadeProjeteis;
        set
        {
            quantidadeProjeteis = Mathf.Max(1, value);
            AtualizarProjeteis();
        }
    }

    public float VelocidadeGiro
    {
        get => velocidadeGiro;
        set => velocidadeGiro = value;
    }

    public float TamanhoProjetil
    {
        get => tamanhoProjetil;
        set
        {
            tamanhoProjetil = value;
            AtualizarProjeteis();
        }
    }

    protected override void Start()
    {
        tipoArma = TipoArma.Orbita;
        base.Start();
        AtualizarProjeteis();
    }

    protected override void Update()
    {
        base.Update();

        float velAtual = velocidadeGiro * weaponAttackSpeed;
        anguloAtual += velAtual * UnityEngine.Time.deltaTime;
        if (anguloAtual >= 360f) anguloAtual -= 360f;

        AtualizarPosicaoProjeteis();
    }

    protected override void DispararAtaqueAutomatico()
    {
        if (projeteisAtivos.Count != quantidadeProjeteis)
        {
            AtualizarProjeteis();
        }
    }

    private void AtualizarProjeteis()
    {
        foreach (GameObject p in projeteisAtivos)
        {
            if (p != null) Destroy(p);
        }
        projeteisAtivos.Clear();

        if (prefabProjetil == null) return;

        for (int i = 0; i < quantidadeProjeteis; i++)
        {
            GameObject novoProjetil = Instantiate(prefabProjetil, transform);
            OrbitaProjetil scriptProj = novoProjetil.GetComponent<OrbitaProjetil>();

            if (scriptProj != null)
            {
                scriptProj.Inicializar(dano, tamanhoProjetil, raioProjetil, layerInimigos);
            }

            projeteisAtivos.Add(novoProjetil);
        }

        AtualizarPosicaoProjeteis();
    }

    private void AtualizarPosicaoProjeteis()
    {
        if (player == null || projeteisAtivos.Count == 0) return;

        Vector2 posCentro = (Vector2)player.transform.position + centroDoPlayerOffset;
        float passoAngulo = 360f / projeteisAtivos.Count;

        for (int i = 0; i < projeteisAtivos.Count; i++)
        {
            if (projeteisAtivos[i] == null) continue;

            float anguloCalculado = anguloAtual + (passoAngulo * i);
            float radiano = anguloCalculado * Mathf.Deg2Rad;

            Vector3 posOrbita = new Vector3(
                posCentro.x + Mathf.Cos(radiano) * raioOrbita,
                posCentro.y + Mathf.Sin(radiano) * raioOrbita,
                0f
            );

            projeteisAtivos[i].transform.position = posOrbita;
        }
    }

    public override void AtivarJackpot(Vector2 offset, float raio)
    {
        EstaEmModoJackpot = true;
        StartCoroutine(RotinaJackpot(raio));
    }

    private IEnumerator RotinaJackpot(float raio)
    {
        float velOriginal = velocidadeGiro;
        float raioOriginal = raioOrbita;

        velocidadeGiro *= multiplicadorVelocidadeJackpot;
        raioOrbita = raio;

        yield return new WaitForSeconds(duracaoJackpot);

        velocidadeGiro = velOriginal;
        raioOrbita = raioOriginal;
        EstaEmModoJackpot = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 centro = player != null ? player.transform.position + (Vector3)centroDoPlayerOffset : transform.position;
        Gizmos.DrawWireSphere(centro, raioOrbita);
    }
}