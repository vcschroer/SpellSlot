using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pistol : BaseWeapon
{
    [Header("Configuracoes da Pistola")]
    [SerializeField] private GameObject prefabProjetil;
    [SerializeField] private Transform pontoDeDisparo;
    [SerializeField] private float velocidadeProjetil = 12f;

    [Header("Configuracoes de Jackpot")]
    [SerializeField] private float duracaoJackpot = 5f;
    [SerializeField] private float velocidadeGiroJackpot = 720f;
    [SerializeField] private float tempoEntreTirosJackpot = 0.08f;

    private float anguloMira;

    protected override void Start()
    {
        tipoArma = TipoArma.Pistola;
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (!EstaEmModoJackpot && player != null && Camera.main != null && Mouse.current != null)
        {
            Vector3 posMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            posMouse.z = 0f;

            Vector2 direcao = (posMouse - transform.position).normalized;
            anguloMira = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, anguloMira);
        }
    }

    protected override void DispararAtaqueAutomatico()
    {
        Atirar();
    }

    private void Atirar()
    {
        if (prefabProjetil == null || pontoDeDisparo == null) return;

        if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("Tiro pistola");

        GameObject bala = Instantiate(prefabProjetil, pontoDeDisparo.position, transform.rotation);
        Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();

        if (rbBala != null)
        {
            rbBala.linearVelocity = transform.right * velocidadeProjetil;
        }

        Destroy(bala, 3f);
    }

    public override void AtivarJackpot(Vector2 offset, float raio)
    {
        EstaEmModoJackpot = true;
        StartCoroutine(RotinaJackpotPistola());
    }

    private IEnumerator RotinaJackpotPistola()
    {
        float timerJackpot = duracaoJackpot;
        float timerTiro = 0f;
        float anguloGiro = 0f;

        while (timerJackpot > 0)
        {
            timerJackpot -= Time.deltaTime;
            timerTiro += Time.deltaTime;

            anguloGiro += velocidadeGiroJackpot * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, 0, anguloGiro);

            if (timerTiro >= tempoEntreTirosJackpot)
            {
                timerTiro = 0f;
                Atirar();
            }

            yield return null;
        }

        EstaEmModoJackpot = false;
    }
}