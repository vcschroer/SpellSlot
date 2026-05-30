using UnityEngine;

public class SwordCheck : MonoBehaviour
{
    private Sword scriptPai;

    public void Configurar(Sword pai)
    {
        scriptPai = pai;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy inimigo = collision.GetComponent<Enemy>();

        if (inimigo != null)
        {
            inimigo.TomarDano();
        }
    }
}
