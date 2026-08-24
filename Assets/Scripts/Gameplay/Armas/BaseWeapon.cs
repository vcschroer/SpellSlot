using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Identificacao")]
    public TipoArma tipoArma;

    [Header("Configuracoes Globais da Arma")]
    [SerializeField] protected float tempoEntreAtaquesBase = 1.5f;

    [Header("Status Modificaveis pela Roleta")]
    public float weaponAttackSpeed = 1f;

    protected PlayerController player;
    private float timerAtaque;

    public bool EstaEmModoJackpot { get; protected set; }

    protected virtual void Start()
    {
        player = GetComponentInParent<PlayerController>();

        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        }
    }

    protected virtual void Update()
    {
        if (player == null || EstaEmModoJackpot) return;

        timerAtaque += UnityEngine.Time.deltaTime;

        float cooldownAtual = tempoEntreAtaquesBase / weaponAttackSpeed;

        if (timerAtaque >= cooldownAtual)
        {
            timerAtaque = 0f;
            DispararAtaqueAutomatico();
        }
    }

    protected abstract void DispararAtaqueAutomatico();
    public abstract void AtivarJackpot(Vector2 offset, float raio);
}