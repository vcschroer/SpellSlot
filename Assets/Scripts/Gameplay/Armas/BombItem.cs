using System.Collections;
using UnityEngine;

public class BombItem : MonoBehaviour
{
    [Header("Efeito Visual")]
    [SerializeField] private GameObject prefabEfeitoExplosao;

    [Header("Configurações da Bomba")]
    [SerializeField] private float tempoParaExplodir = 3f;
    [SerializeField] private float raioExplosao = 2.5f;

    [Header("Gizmos e Debug")]
    [SerializeField] private Color corDoGizmo = Color.red;

    private int dano;
    private LayerMask layerInimigos;

    public void Inicializar(int dano, float raioExplosao, float tempoParaExplodir, float escalaBomba, LayerMask layerInimigos)
    {
        this.dano = dano;
        this.raioExplosao = raioExplosao;
        this.tempoParaExplodir = tempoParaExplodir;
        this.layerInimigos = layerInimigos;

        transform.localScale = Vector3.one * escalaBomba;

        StartCoroutine(RotinaExplosao());
    }

    private IEnumerator RotinaExplosao()
    {
        yield return new WaitForSeconds(tempoParaExplodir);
        Explodir();
    }

    private void Explodir()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("Explosao");
        }

        if (prefabEfeitoExplosao != null)
        {
            GameObject fx = Instantiate(prefabEfeitoExplosao, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * (raioExplosao * 2f);
            Destroy(fx, 2f);
        }

        Collider2D[] acertados = Physics2D.OverlapCircleAll(transform.position, raioExplosao, layerInimigos);

        foreach (Collider2D col in acertados)
        {
            if (col == null) continue;

            Enemy inimigoAtingido = col.GetComponent<Enemy>();

            if (inimigoAtingido != null)
            {
                inimigoAtingido.TomarDano(dano);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = corDoGizmo;
        Gizmos.DrawWireSphere(transform.position, raioExplosao);

        Gizmos.color = new Color(corDoGizmo.r, corDoGizmo.g, corDoGizmo.b, 0.2f);
        Gizmos.DrawSphere(transform.position, raioExplosao);
    }
}