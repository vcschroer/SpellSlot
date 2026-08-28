using UnityEngine;

public class DustAreaSpawner : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PolygonCollider2D areaCollider;
    [SerializeField] private ParticleSystem sistemaPoeira;

    private Transform alvoPlayer;

    [Header("Configurações de Spawn")]
    [SerializeField] private float taxaEmissaoPorSegundo = 6f;
    [SerializeField] private int poeiraInicialCount = 40;
    [SerializeField] private float raioSpawnPlayer = 6f;

    private float contadorEmissao;

    private void Reset()
    {
        areaCollider = GetComponent<PolygonCollider2D>();
        sistemaPoeira = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (areaCollider == null) areaCollider = GetComponent<PolygonCollider2D>();
        if (sistemaPoeira == null) sistemaPoeira = GetComponent<ParticleSystem>();

        BuscarPlayer();

        for (int i = 0; i < poeiraInicialCount; i++)
        {
            EmiteParticulaEmPontoValido(true);
        }
    }

    private void Update()
    {
        contadorEmissao += UnityEngine.Time.deltaTime * taxaEmissaoPorSegundo;

        while (contadorEmissao >= 1f)
        {
            EmiteParticulaEmPontoValido(false);
            contadorEmissao -= 1f;
        }
    }

    private Transform BuscarPlayer()
    {
        if (alvoPlayer == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                alvoPlayer = playerObj.transform;
            }
        }
        return alvoPlayer;
    }

    private void EmiteParticulaEmPontoValido(bool randomizarTempoVida)
    {
        if (areaCollider == null || sistemaPoeira == null) return;

        Vector2 pontoValido;
        if (TentarObterPontoNoPoligono(out pontoValido))
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = new Vector3(pontoValido.x, pontoValido.y, transform.position.z);

            if (randomizarTempoVida)
            {
                float maxLifetime = sistemaPoeira.main.startLifetime.constantMax;
                emitParams.startLifetime = Random.Range(0.1f, maxLifetime);
            }

            sistemaPoeira.Emit(emitParams, 1);
        }
    }

    private bool TentarObterPontoNoPoligono(out Vector2 ponto)
    {
        int maxTentativas = 30;
        Transform player = BuscarPlayer();

        for (int i = 0; i < maxTentativas; i++)
        {
            Vector2 pontoTeste;

            if (player != null)
            {
                pontoTeste = (Vector2)player.position + (Random.insideUnitCircle * raioSpawnPlayer);
            }
            else
            {
                Bounds bounds = areaCollider.bounds;
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float y = Random.Range(bounds.min.y, bounds.max.y);
                pontoTeste = new Vector2(x, y);
            }

            if (areaCollider.OverlapPoint(pontoTeste))
            {
                ponto = pontoTeste;
                return true;
            }
        }

        ponto = Vector2.zero;
        return false;
    }
}