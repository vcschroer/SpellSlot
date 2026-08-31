using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configurações da Moeda")]
    [SerializeField] private int valorDinheiro = 10;

    [Header("Efeito Flutuante Ocioso")]
    [SerializeField] private float amplitudeFloating = 0.25f;
    [SerializeField] private float velocidadeFloating = 3f;

    [Header("Efeito de Atração / Coleta")]
    [SerializeField] private float velocidadeAtraçãoInicial = 5f;
    [SerializeField] private float aceleracaoAtração = 15f;
    [SerializeField] private float distanciaColeta = 0.2f;

    private Vector3 posicaoInicial;
    private Transform alvoPlayer;
    private bool sendoColetada = false;
    private float velocidadeAtualAtração;

    private void Start()
    {
        posicaoInicial = transform.position;
    }

    private void Update()
    {
        if (sendoColetada)
        {
            MoverParaOPlayer();
        }
        else
        {
            AplicarFloating();
        }
    }

    private void AplicarFloating()
    {
        float novoY = posicaoInicial.y + (Mathf.Sin(UnityEngine.Time.time * velocidadeFloating) * amplitudeFloating);
        transform.position = new Vector3(posicaoInicial.x, novoY, posicaoInicial.z);
    }

    private void MoverParaOPlayer()
    {
        if (alvoPlayer == null)
        {
            Destroy(gameObject);
            return;
        }

        velocidadeAtualAtração += aceleracaoAtração * UnityEngine.Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            alvoPlayer.position,
            velocidadeAtualAtração * UnityEngine.Time.deltaTime
        );

        if (Vector3.Distance(transform.position, alvoPlayer.position) <= distanciaColeta)
        {
            FinalizarColeta();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (sendoColetada) return;

        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            alvoPlayer = player.transform;
            sendoColetada = true;
            velocidadeAtualAtração = velocidadeAtraçãoInicial;

            Collider2D colisor = GetComponent<Collider2D>();
            if (colisor != null) colisor.enabled = false;
        }
    }

    private void FinalizarColeta()
    {
        if (alvoPlayer != null)
        {
            PlayerController player = alvoPlayer.GetComponent<PlayerController>();
            if (player != null)
            {
                player.GanharDinheiro(valorDinheiro);
            }
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("coin");
        }

        Destroy(gameObject);
    }
}