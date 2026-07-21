using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Configuracoes de Dano")]
    [SerializeField] private int danoBala = 5;

    [Header("Configuracoes de Ricochete")]
    [Tooltip("Quantas vezes a bala pode ricochetear antes de sumir.")]
    [SerializeField] private int ricochetesRestantes = 0;

    [Tooltip("Distancia maxima que a bala consegue 'enxergar' outro inimigo para ricochetear.")]
    [SerializeField] private float raioBuscaRicochete = 8f;

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
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
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
}