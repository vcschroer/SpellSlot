using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configurações da Moeda")]
    [SerializeField] private int valorDinheiro = 10;

    [Header("Efeito Flutuante")]
    [SerializeField] private float amplitudeFloating = 0.25f;
    [SerializeField] private float velocidadeFloating = 3f;

    private Vector3 posicaoInicial;

    private void Start()
    {
        posicaoInicial = transform.position;
    }

    private void Update()
    {
        AplicarFloating();
    }

    private void AplicarFloating()
    {
        float novoY = posicaoInicial.y + (Mathf.Sin(UnityEngine.Time.time * velocidadeFloating) * amplitudeFloating);
        transform.position = new Vector3(posicaoInicial.x, novoY, posicaoInicial.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            player.GanharDinheiro(valorDinheiro);

            Destroy(gameObject);
        }
    }
}