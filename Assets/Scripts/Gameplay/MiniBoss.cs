using UnityEngine;
using static EnemySpawn;

public class MiniBoss : Enemy
{
    [Header("Configurações do Mini Boss")]
    [Tooltip("Multiplicador do tamanho visual do Boss (Ex: 1.8 para ser 80% maior)")]
    [SerializeField] private float multiplicadorTamanho = 1.8f;

    [Tooltip("Quantidade de vida total do Mini Boss")]
    [SerializeField] private int vidaBoss;

    [Header("Drop Especial de Arma")]
    [Tooltip("Prefab do item coletável de arma (WeaponDrop)")]
    [SerializeField] private GameObject prefabWeaponDrop;

    protected override void Start()
    {
        base.Start();

        transform.localScale *= multiplicadorTamanho;

        vidaAtual = vidaBoss;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TomarDano(danoNoPlayer);
            }
        }
        else
        {
            base.OnCollisionEnter2D(collision);
        }
    }

    protected override void IniciarProcessoMorte(bool deveDroparMoeda)
    {
        base.IniciarProcessoMorte(deveDroparMoeda);

        if (prefabWeaponDrop != null)
        {
            Instantiate(prefabWeaponDrop, transform.position, Quaternion.identity);
        }
    }
}