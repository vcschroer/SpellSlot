using System.Collections.Generic;
using UnityEngine;

public class OrbitaProjetil : MonoBehaviour
{
    private int dano;
    private float raioAtaque;
    private LayerMask layerInimigos;
    private float tempoEntreDanos = 0.3f;
    private Dictionary<Enemy, float> ultimosAtaques = new Dictionary<Enemy, float>();

    public void Inicializar(int dano, float tamanho, float raioAtaque, LayerMask layerInimigos)
    {
        this.dano = dano;
        this.raioAtaque = raioAtaque;
        this.layerInimigos = layerInimigos;
        transform.localScale = Vector3.one * tamanho;
    }

    private void Update()
    {
        DetectarEDarDano();
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
    }
}