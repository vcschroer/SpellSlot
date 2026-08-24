using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonaWeapon : BaseWeapon
{
    [Header("Configurações da Zona")]
    [SerializeField] private int dano = 5;
    [SerializeField] private float raioZona = 2.5f;
    [SerializeField] private float tempoEntreDanos = 0.5f;

    [Header("Visual por Sprite")]
    [SerializeField] private SpriteRenderer spriteZona;
    [SerializeField] private float multiplicadorEscalaVisual = 1f;

    [Header("Filtro de Inimigos")]
    [SerializeField] private LayerMask layerInimigos;

    [Header("Posicionamento")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);

    [Header("Configurações de Jackpot")]
    [SerializeField] private float duracaoJackpot = 5f;
    [SerializeField] private float multiplicadorRaioJackpot = 2f;

    private Dictionary<Enemy, float> ultimosAtaques = new Dictionary<Enemy, float>();

    public float RaioZona
    {
        get => raioZona;
        set
        {
            raioZona = Mathf.Max(0.1f, value);
            AtualizarTamanhoVisual();
        }
    }

    protected override void Start()
    {
        tipoArma = TipoArma.Zona;
        base.Start();
        AtualizarTamanhoVisual();
    }

    private void OnValidate()
    {
        AtualizarTamanhoVisual();
    }

    protected override void Update()
    {
        base.Update();
        AcompanharPlayer();
        ProcessarDanoEmArea();
    }

    protected override void DispararAtaqueAutomatico()
    {
    }

    private void AcompanharPlayer()
    {
        if (player != null)
        {
            transform.position = (Vector2)player.transform.position + centroDoPlayerOffset;
        }
    }

    private void ProcessarDanoEmArea()
    {
        Collider2D[] acertados = Physics2D.OverlapCircleAll(transform.position, raioZona, layerInimigos);

        foreach (Collider2D col in acertados)
        {
            if (col == null) continue;

            Enemy inimigo = col.GetComponent<Enemy>();
            if (inimigo != null)
            {
                float cooldownReal = tempoEntreDanos / weaponAttackSpeed;

                if (!ultimosAtaques.ContainsKey(inimigo) || UnityEngine.Time.time >= ultimosAtaques[inimigo] + cooldownReal)
                {
                    inimigo.TomarDano(dano);
                    ultimosAtaques[inimigo] = UnityEngine.Time.time;
                }
            }
        }
    }

    private void AtualizarTamanhoVisual()
    {
        if (spriteZona != null)
        {
            float escalaCalculada = (raioZona * 2f) * multiplicadorEscalaVisual;
            spriteZona.transform.localScale = new Vector3(escalaCalculada, escalaCalculada, 1f);
        }
    }

    public override void AtivarJackpot(Vector2 offset, float raio)
    {
        EstaEmModoJackpot = true;
        StartCoroutine(RotinaJackpot(raio));
    }

    private IEnumerator RotinaJackpot(float raio)
    {
        float raioOriginal = raioZona;
        raioZona = raio > 0 ? raio : raioOriginal * multiplicadorRaioJackpot;
        AtualizarTamanhoVisual();

        yield return new WaitForSeconds(duracaoJackpot);

        raioZona = raioOriginal;
        AtualizarTamanhoVisual();
        EstaEmModoJackpot = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 centro = player != null ? player.transform.position + (Vector3)centroDoPlayerOffset : transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(centro, raioZona);

        Gizmos.color = new Color(1f, 0f, 1f, 0.15f);
        Gizmos.DrawSphere(centro, raioZona);
    }
}