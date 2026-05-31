using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configurações da Moeda")]
    [SerializeField] private int valorDinheiro = 10;

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
