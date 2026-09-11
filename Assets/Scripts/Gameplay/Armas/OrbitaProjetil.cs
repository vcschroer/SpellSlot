using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitaProjetil : MonoBehaviour
{
    [Header("Sprites do Carro")]
    [SerializeField] private Sprite spriteFrente;
    [SerializeField] private Sprite spriteCostas;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Configurações de Corte e Flip")]
    [Tooltip("Altura Y em relação ao player onde o carro troca de Frente para Costas.")]
    [SerializeField] private float offsetLinhaCorteY = 0f;

    [Tooltip("Inverte o comportamento do Flip da parte de baixo se necessário.")]
    [SerializeField] private bool inverterFlipParteBaixo = false;

    [Header("Suavização da Rotação (Drift)")]
    [Tooltip("Ângulo máximo de inclinação em Z quando o carro está nas pontas esquerdo/direito.")]
    [SerializeField] private float maxInclinacaoGraus = 30f;

    [Tooltip("Velocidade de suavização da rotação. Valores entre 10 e 20 removem qualquer tremor.")]
    [SerializeField] private float velocidadeSuavizacao = 15f;

    [Tooltip("Inverte a direção da inclinação Z caso precise ajustar o alinhamento.")]
    [SerializeField] private bool inverterInclinacaoZ = false;

    private int dano;
    private float raioAtaque;
    private LayerMask layerInimigos;
    private float tempoEntreDanos = 0.3f;
    private Dictionary<Enemy, float> ultimosAtaques = new Dictionary<Enemy, float>();
    private Transform playerTransform;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void Inicializar(int dano, float tamanho, float raioAtaque, LayerMask layerInimigos, Transform player)
    {
        this.dano = dano;
        this.raioAtaque = raioAtaque;
        this.layerInimigos = layerInimigos;
        this.playerTransform = player;
        transform.localScale = Vector3.one * tamanho;
    }

    private void Update()
    {
        AtualizarVisualDriftSuave();
        DetectarEDarDano();
    }

    private void AtualizarVisualDriftSuave()
    {
        if (playerTransform == null || spriteRenderer == null) return;

        Vector3 posRelativa = transform.position - playerTransform.position;
        float yAjustado = posRelativa.y - offsetLinhaCorteY;

        bool estaAcima = yAjustado > 0f;
        bool estaNaEsquerda = posRelativa.x < 0f;

        if (estaAcima)
        {
            if (spriteFrente != null) spriteRenderer.sprite = spriteFrente;
        }
        else
        {
            if (spriteCostas != null) spriteRenderer.sprite = spriteCostas;
        }

        bool deveFlipar;
        if (estaAcima)
        {
            deveFlipar = estaNaEsquerda;
        }
        else
        {
            deveFlipar = inverterFlipParteBaixo ? estaNaEsquerda : !estaNaEsquerda;
        }

        spriteRenderer.flipX = deveFlipar;

        float distancia = Vector2.Distance(transform.position, playerTransform.position);
        float xNormalizado = distancia > 0.001f ? (posRelativa.x / distancia) : 0f;

        float anguloAlvoZ = xNormalizado * maxInclinacaoGraus;

        if (estaAcima)
        {
            anguloAlvoZ = -anguloAlvoZ;
        }

        if (inverterInclinacaoZ)
        {
            anguloAlvoZ = -anguloAlvoZ;
        }

        Quaternion rotAlvo = Quaternion.Euler(0f, 0f, anguloAlvoZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotAlvo, UnityEngine.Time.deltaTime * velocidadeSuavizacao);
    }

    private void DetectarEDarDano()
    {
        Collider2D[] acertados = Physics2D.OverlapCircleAll(transform.position, raioAtaque, layerInimigos);

        foreach (Collider2D col in acertados)
        {
            if (col == null) continue;

            Enemy inimigoAtingido = col.GetComponent<Enemy>();

            if (inimigoAtingido != null)
            {
                if (!ultimosAtaques.ContainsKey(inimigoAtingido) || UnityEngine.Time.time >= ultimosAtaques[inimigoAtingido] + tempoEntreDanos)
                {
                    inimigoAtingido.TomarDano(dano);
                    ultimosAtaques[inimigoAtingido] = UnityEngine.Time.time;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioAtaque);

        if (playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 centroLinha = playerTransform.position + new Vector3(0f, offsetLinhaCorteY, 0f);
            Gizmos.DrawLine(centroLinha + Vector3.left * 5f, centroLinha + Vector3.right * 5f);
        }
    }
}