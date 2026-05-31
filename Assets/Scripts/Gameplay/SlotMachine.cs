using UnityEngine;
using UnityEngine.InputSystem;

public class SlotMachine : MonoBehaviour
{
    public enum TipoRecompensa { Dinheiro, TamanhoEspada, VelocidadeAtaque, VelocidadePlayer, Vazio }

    [Header("Referências do Player")]
    [SerializeField] private PlayerController scriptPlayer;
    [SerializeField] private Sword scriptEspada;

    [Header("Conexão com a Parte Visual")]
    [SerializeField] private SlotMachineVisual scriptVisual;

    [Header("Configurações do Giro")]
    [SerializeField] private int custoVidaPorGiro = 10;
    [SerializeField] private float tempoCooldown = 2.0f;

    [Header("Sistema de Pity System")]
    [SerializeField] private int girosParaGarantirPremio = 5;
    private int contadorGirosPerdidos = 0;

    [Header("Configuração de Sorte Acumulada")]
    [Range(0f, 100f)]
    [SerializeField] private float chanceJackpotMelhorada = 40f;

    [Header("Trava Anti-Jackpot Seguido")]
    [SerializeField] private int girosBloqueadosPosJackpot = 3;
    private int girosSemJackpotRestantes = 0;

    [Header("Valores dos Bônus - DINHEIRO")]
    [SerializeField] private int dinheiroPequeno = 50;
    [SerializeField] private int dinheiroGrande = 200;

    [Header("Valores dos Bônus - TAMANHO DA ESPADA")]
    [SerializeField] private int gomosPequeno = 1;
    [SerializeField] private int gomosGrande = 3;

    [Header("Valores dos Bônus - VELOCIDADE DE ATAQUE")]
    [SerializeField] private float velAtaquePequeno = 0.25f;
    [SerializeField] private float velAtaqueGrande = 0.75f;

    [Header("Valores dos Bônus - VELOCIDADE DO PLAYER")]
    [SerializeField] private float velPlayerPequeno = 1.5f;
    [SerializeField] private float velPlayerGrande = 4f;

    private float momentoProximoGiroPermitido = 0f;
    private bool menuAberto = false;
    private bool proximoGiroTemSorteGeral = false;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            AlternarCaçaNiquel();
        }
    }

    private void AlternarCaçaNiquel()
    {
        if (scriptPlayer == null) return;

        if (Time.time < momentoProximoGiroPermitido)
        {
            float tempoRestante = momentoProximoGiroPermitido - Time.time;
            Debug.Log($"[COOLDOWN]: Aguarde mais {tempoRestante:F1} segundos.");
            return;
        }

        if (scriptPlayer.dinheiroAtual - custoVidaPorGiro < 1)
        {
            Debug.LogWarning("Vida insuficiente!");
            return;
        }

        menuAberto = !menuAberto;
        if (menuAberto)
        {
            momentoProximoGiroPermitido = Time.time + tempoCooldown;
            scriptPlayer.PerderDinheiro(custoVidaPorGiro);
            GirarCaçaNiquel();
        }
    }

    private void GirarCaçaNiquel()
    {
        TipoRecompensa slot1, slot2, slot3;
        bool travaAtiva = girosSemJackpotRestantes > 0;
        bool forcarVitoria = contadorGirosPerdidos >= girosParaGarantirPremio;

        if (forcarVitoria)
        {
            TipoRecompensa premioGarantido = (TipoRecompensa)Random.Range(0, 4);
            slot1 = premioGarantido;
            slot2 = premioGarantido;
            slot3 = premioGarantido;
            Debug.Log($"[PITY SYSTEM]: Vitória forçada após {contadorGirosPerdidos} giros perdidos!");
        }
        else if (!travaAtiva && proximoGiroTemSorteGeral && Random.Range(0f, 100f) <= chanceJackpotMelhorada)
        {
            slot1 = (TipoRecompensa)Random.Range(0, 5);
            slot2 = slot1;
            slot3 = slot1;
        }
        else
        {
            slot1 = (TipoRecompensa)Random.Range(0, 5);
            slot2 = (TipoRecompensa)Random.Range(0, 5);
            slot3 = (TipoRecompensa)Random.Range(0, 5);

            if (travaAtiva && slot1 == slot2 && slot2 == slot3)
            {
                while (slot3 == slot1) slot3 = (TipoRecompensa)Random.Range(0, 5);
            }
        }

        proximoGiroTemSorteGeral = false;
        if (travaAtiva) girosSemJackpotRestantes--;

        if (scriptVisual != null) scriptVisual.AtualizarVisualDosSlots(slot1, slot2, slot3);

        int qtdDinheiro = 0, qtdTamanho = 0, qtdVelAtaque = 0, qtdVelPlayer = 0;
        ContarSlot(slot1, ref qtdDinheiro, ref qtdTamanho, ref qtdVelAtaque, ref qtdVelPlayer);
        ContarSlot(slot2, ref qtdDinheiro, ref qtdTamanho, ref qtdVelAtaque, ref qtdVelPlayer);
        ContarSlot(slot3, ref qtdDinheiro, ref qtdTamanho, ref qtdVelAtaque, ref qtdVelPlayer);

        bool houveVitoria = (qtdDinheiro >= 2 || qtdTamanho >= 2 || qtdVelAtaque >= 2 || qtdVelPlayer >= 2);
        if (houveVitoria) contadorGirosPerdidos = 0;
        else contadorGirosPerdidos++;

        if (qtdDinheiro == 3 || qtdTamanho == 3 || qtdVelAtaque == 3 || qtdVelPlayer == 3)
        {
            girosSemJackpotRestantes = girosBloqueadosPosJackpot;

            Vector2 offset = (Vector2)typeof(PlayerController).GetField("centroDoPlayerOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scriptPlayer);
            float raio = (float)typeof(PlayerController).GetField("raioDaOrbita", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scriptPlayer);

            scriptEspada.AtivarJackpot(offset, raio);
        }
        else if (qtdDinheiro == 2 || qtdTamanho == 2 || qtdVelAtaque == 2 || qtdVelPlayer == 2)
        {
            if (girosSemJackpotRestantes <= 0) proximoGiroTemSorteGeral = true;
        }

        AplicarRecompensaDinheiro(qtdDinheiro);
        AplicarRecompensaTamanho(qtdTamanho);
        AplicarRecompensaVelocidadeAtaque(qtdVelAtaque);
        AplicarRecompensaVelocidadePlayer(qtdVelPlayer);

        menuAberto = false;
    }

    private void ContarSlot(TipoRecompensa slot, ref int din, ref int tam, ref int velA, ref int velP)
    {
        switch (slot)
        {
            case TipoRecompensa.Dinheiro: din++; break;
            case TipoRecompensa.TamanhoEspada: tam++; break;
            case TipoRecompensa.VelocidadeAtaque: velA++; break;
            case TipoRecompensa.VelocidadePlayer: velP++; break;
        }
    }

    private void AplicarRecompensaDinheiro(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        int bonus = quantidade switch { 2 => dinheiroPequeno, 3 => dinheiroGrande, _ => 0 };
        scriptPlayer.GanharDinheiro(bonus);
    }

    private void AplicarRecompensaTamanho(int quantidade)
    {
        if (quantidade < 2 || scriptEspada == null) return;
        int gomos = quantidade switch { 2 => gomosPequeno, 3 => gomosGrande, _ => 0 };
        scriptEspada.MudarQuantidadeSegmentos(scriptEspada.QuantidadeSegmentosMeio + gomos);
    }

    private void AplicarRecompensaVelocidadeAtaque(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        float vel = quantidade switch { 2 => velAtaquePequeno, 3 => velAtaqueGrande, _ => 0f };
        scriptPlayer.attackSpeed += vel;
    }

    private void AplicarRecompensaVelocidadePlayer(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        float vel = quantidade switch { 2 => velPlayerPequeno, 3 => velPlayerGrande, _ => 0f };
        scriptPlayer.AumentarVelocidade(vel);
    }
}
