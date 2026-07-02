using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Identificacao (Para a Roleta)")]
    public TipoArma tipoArma;

    [Header("Configuracoes Globais da Arma")]
    [Tooltip("Tempo base em segundos entre cada ataque automatico")]
    [SerializeField] protected float tempoEntreAtaquesBase = 1.5f;

    [Header("Status Modificaveis pela Roleta")]
    [Tooltip("A velocidade de ataque PROPRIA desta arma (Modificada pela roleta)")]
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

        timerAtaque += Time.deltaTime;

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