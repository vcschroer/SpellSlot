using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Configuracoes de Dano")]
    [SerializeField] private int danoBala = 5;

    [Header("Configuracoes de Ricochete")]
    [SerializeField] private int ricochetesRestantes = 0;
    [SerializeField] private float raioBuscaRicochete = 8f;

    [Header("Configuracoes de Rastro (Trail)")]
    [SerializeField] private TrailRenderer rastroBala;
    [SerializeField] private float larguraBaseTrail = 0.2f;

    private Rigidbody2D rb;
    private float velocidadeOriginal;

    public int RicochetesRestantes
    {
        get => ricochetesRestantes;
        set => ricochetesRestantes = Mathf.Max(0, value);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            velocidadeOriginal = rb.linearVelocity.magnitude;
        }

        ConfigurarTrail();
    }

    private void ConfigurarTrail()
    {
        if (rastroBala == null)
        {
            rastroBala = GetComponentInChildren<TrailRenderer>();
        }

        if (rastroBala != null)
        {
            rastroBala.widthMultiplier = larguraBaseTrail;
            rastroBala.Clear();
            rastroBala.emitting = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy inimigoAtingido = collision.GetComponent<Enemy>();

        if (inimigoAtingido != null)
        {
            inimigoAtingido.TomarDano(danoBala);

            if (ricochetesRestantes > 0)
            {
                ricochetesRestantes--;

                bool conseguiuRicochetear = TentarRicochetear(inimigoAtingido);

                if (!conseguiuRicochetear)
                {
                    DestruirComSeguranca();
                }
            }
            else
            {
                DestruirComSeguranca();
            }
        }
    }

    private bool TentarRicochetear(Enemy inimigoAtual)
    {
        Enemy[] todosOsInimigos = FindObjectsOfType<Enemy>();
        Enemy proximoInimigo = null;
        float menorDistancia = float.MaxValue;

        foreach (Enemy inimigo in todosOsInimigos)
        {
            if (inimigo == null || inimigo == inimigoAtual) continue;

            Collider2D col = inimigo.GetComponent<Collider2D>();
            if (col != null && !col.enabled) continue;

            float distancia = Vector2.Distance(transform.position, inimigo.transform.position);

            if (distancia < menorDistancia && distancia <= raioBuscaRicochete)
            {
                menorDistancia = distancia;
                proximoInimigo = inimigo;
            }
        }

        if (proximoInimigo != null && rb != null)
        {
            Vector2 direcao = ((Vector2)proximoInimigo.transform.position - (Vector2)transform.position).normalized;

            float vel = velocidadeOriginal > 0.1f ? velocidadeOriginal : 12f;

            rb.linearVelocity = direcao * vel;

            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);

            return true;
        }

        return false;
    }

    private void DestruirComSeguranca()
    {
        if (rastroBala != null)
        {
            rastroBala.transform.parent = null;
            rastroBala.emitting = false;
            Destroy(rastroBala.gameObject, rastroBala.time);
        }

        Destroy(gameObject);
    }
}