using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotMachine : MonoBehaviour
{
    public enum TipoRecompensa { Dinheiro, TamanhoEspada, VelocidadeAtaqueEspada, VelocidadeAtaquePistola, VelocidadePlayer, PistolBalas, PistolRicochete, Vazio }

    [Header("Referencias do Player")]
    [SerializeField] private PlayerController scriptPlayer;

    [Header("Conexao com a Parte Visual")]
    [SerializeField] private SlotMachineVisual scriptVisual;

    [Header("Configuracoes do Giro")]
    [SerializeField] private int custoVidaPorGiro = 10;
    [SerializeField] private float tempoCooldown = 2.0f;

    [Header("Sistema de Pity System")]
    [SerializeField] private int girosParaGarantirPremio = 5;
    private int contadorGirosPerdidos = 0;

    [Header("Configuracao de Sorte Acumulada")]
    [Range(0f, 100f)]
    [SerializeField] private float chanceJackpotMelhorada = 40f;

    [Header("Trava Anti-Jackpot Seguido")]
    [SerializeField] private int girosBloqueadosPosJackpot = 3;
    private int girosSemJackpotRestantes = 0;

    [Header("Valores dos Bonus - DINHEIRO")]
    [SerializeField] private int dinheiroPequeno = 50;
    [SerializeField] private int dinheiroGrande = 200;

    [Header("Valores dos Bonus - TAMANHO DA ESPADA")]
    [SerializeField] private int gomosPequeno = 1;
    [SerializeField] private int gomosGrande = 3;

    [Header("Valores dos Bonus - VELOCIDADE DE ATAQUE ESPADA")]
    [SerializeField] private float velAtaqueEspadaPequeno = 0.25f;
    [SerializeField] private float velAtaqueEspadaGrande = 0.75f;

    [Header("Valores dos Bonus - VELOCIDADE DE ATAQUE PISTOLA")]
    [SerializeField] private float velAtaquePistolaPequeno = 0.25f;
    [SerializeField] private float velAtaquePistolaGrande = 0.75f;

    [Header("Valores dos Bonus - VELOCIDADE DO PLAYER")]
    [SerializeField] private float velPlayerPequeno = 1.5f;
    [SerializeField] private float velPlayerGrande = 4f;

    [Header("Valores dos Bonus - UPGRADES DA PISTOLA")]
    [SerializeField] private int balasAdicionaisPequeno = 1;
    [SerializeField] private int balasAdicionaisGrande = 2;
    [SerializeField] private int ricochetesAdicionaisPequeno = 1;
    [SerializeField] private int ricochetesAdicionaisGrande = 2;

    [Header("Configuracoes do Jackpot")]
    [SerializeField] private Vector2 centroDoPlayerOffset = Vector2.zero;
    [SerializeField] private float raioDaOrbita = 2.0f;

    private float momentoProximoGiroPermitido = 0f;
    private bool menuAberto = false;
    private bool proximoGiroTemSorteGeral = false;
    private bool estaProcessandoGiro = false;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            AlternarCacaNiquel();
        }
    }

    private void AlternarCacaNiquel()
    {
        if (scriptPlayer == null) scriptPlayer = Object.FindFirstObjectByType<PlayerController>();

        if (scriptPlayer == null || estaProcessandoGiro) return;

        if (Time.time < momentoProximoGiroPermitido)
        {
            float tempoRestante = momentoProximoGiroPermitido - Time.time;
            Debug.Log($"[COOLDOWN]: Aguarde mais {tempoRestante:F1} segundos.");
            return;
        }

        if (scriptPlayer.dinheiroAtual - custoVidaPorGiro < 1)
        {
            Debug.LogWarning("Vida (Dinheiro) insuficiente para girar!");
            return;
        }

        menuAberto = !menuAberto;
        if (menuAberto)
        {
            momentoProximoGiroPermitido = Time.time + tempoCooldown;
            scriptPlayer.PerderDinheiro(custoVidaPorGiro);
            GirarCacaNiquel();
        }
    }

    private List<TipoRecompensa> GerarPoolDePremiosValidos()
    {
        List<TipoRecompensa> poolValido = new List<TipoRecompensa>
        {
            TipoRecompensa.Dinheiro,
            TipoRecompensa.VelocidadePlayer,
            TipoRecompensa.Vazio
        };

        if (scriptPlayer != null)
        {
            bool temEspada = scriptPlayer.ObterArmaPorTipo(TipoArma.Espada) != null;
            bool temPistola = scriptPlayer.ObterArmaPorTipo(TipoArma.Pistola) != null;

            if (temEspada)
            {
                poolValido.Add(TipoRecompensa.TamanhoEspada);
                poolValido.Add(TipoRecompensa.VelocidadeAtaqueEspada);
            }

            if (temPistola)
            {
                poolValido.Add(TipoRecompensa.PistolBalas);
                poolValido.Add(TipoRecompensa.PistolRicochete);
                poolValido.Add(TipoRecompensa.VelocidadeAtaquePistola);
            }
        }

        return poolValido;
    }

    private void GirarCacaNiquel()
    {
        estaProcessandoGiro = true;

        List<TipoRecompensa> poolValido = GerarPoolDePremiosValidos();

        TipoRecompensa slot1, slot2, slot3;
        bool travaAtiva = girosSemJackpotRestantes > 0;
        bool forcarVitoria = contadorGirosPerdidos >= girosParaGarantirPremio;

        if (forcarVitoria)
        {
            List<TipoRecompensa> premiosReais = poolValido.FindAll(x => x != TipoRecompensa.Vazio);
            TipoRecompensa premioGarantido = premiosReais.Count > 0 ? premiosReais[Random.Range(0, premiosReais.Count)] : TipoRecompensa.Dinheiro;

            slot1 = premioGarantido;
            slot2 = premioGarantido;
            slot3 = premioGarantido;
            Debug.Log($"[PITY SYSTEM]: Vitoria forcada com premio valido!");
        }
        else if (!travaAtiva && proximoGiroTemSorteGeral && Random.Range(0f, 100f) <= chanceJackpotMelhorada)
        {
            slot1 = poolValido[Random.Range(0, poolValido.Count)];
            slot2 = slot1;
            slot3 = slot1;
        }
        else
        {
            slot1 = poolValido[Random.Range(0, poolValido.Count)];
            slot2 = poolValido[Random.Range(0, poolValido.Count)];
            slot3 = poolValido[Random.Range(0, poolValido.Count)];

            if (travaAtiva && slot1 == slot2 && slot2 == slot3)
            {
                while (slot3 == slot1)
                {
                    slot3 = poolValido[Random.Range(0, poolValido.Count)];
                }
            }
        }

        if (scriptVisual != null) scriptVisual.AtualizarVisualDosSlots(slot1, slot2, slot3);

        StartCoroutine(RotinaProcessarResultado(slot1, slot2, slot3, travaAtiva));
    }

    private IEnumerator RotinaProcessarResultado(TipoRecompensa slot1, TipoRecompensa slot2, TipoRecompensa slot3, bool travaAtiva)
    {
        if (scriptVisual != null) yield return new WaitForSeconds(scriptVisual.ObterTempoTotalGiro());
        else yield return new WaitForSeconds(2.0f);

        proximoGiroTemSorteGeral = false;
        if (travaAtiva) girosSemJackpotRestantes--;

        int din = 0, tam = 0, velAEspada = 0, velAPistola = 0, velP = 0, pBalas = 0, pRico = 0;
        ContarSlot(slot1, ref din, ref tam, ref velAEspada, ref velAPistola, ref velP, ref pBalas, ref pRico);
        ContarSlot(slot2, ref din, ref tam, ref velAEspada, ref velAPistola, ref velP, ref pBalas, ref pRico);
        ContarSlot(slot3, ref din, ref tam, ref velAEspada, ref velAPistola, ref velP, ref pBalas, ref pRico);

        bool houveVitoria = (din >= 2 || tam >= 2 || velAEspada >= 2 || velAPistola >= 2 || velP >= 2 || pBalas >= 2 || pRico >= 2);
        contadorGirosPerdidos = houveVitoria ? 0 : contadorGirosPerdidos + 1;

        if (din == 3 || tam == 3 || velAEspada == 3 || velAPistola == 3 || velP == 3 || pBalas == 3 || pRico == 3)
        {
            girosSemJackpotRestantes = girosBloqueadosPosJackpot;

            if (scriptPlayer != null)
            {
                SpriteEffects playerEffects = scriptPlayer.GetComponent<SpriteEffects>();
                if (playerEffects == null) playerEffects = scriptPlayer.GetComponentInChildren<SpriteEffects>();

                if (playerEffects != null)
                {
                    playerEffects.DefinirRGB(true); 
                }

                if (scriptPlayer.armasEquipadas != null)
                {
                    foreach (BaseWeapon arma in scriptPlayer.armasEquipadas)
                    {
                        if (arma != null) arma.AtivarJackpot(centroDoPlayerOffset, raioDaOrbita);
                    }
                }
            }
        }
        else if (din == 2 || tam == 2 || velAEspada == 2 || velAPistola == 2 || velP == 2 || pBalas == 2 || pRico == 2)
        {
            if (girosSemJackpotRestantes <= 0) proximoGiroTemSorteGeral = true;
        }

        AplicarRecompensaDinheiro(din);
        AplicarRecompensaTamanho(tam);
        AplicarRecompensaVelAtaqueEspada(velAEspada);
        AplicarRecompensaVelAtaquePistola(velAPistola);
        AplicarRecompensaVelocidadePlayer(velP);
        AplicarRecompensaPistolBalas(pBalas);
        AplicarRecompensaPistolRicochete(pRico);

        menuAberto = false;
        estaProcessandoGiro = false;

    }

    private void ContarSlot(TipoRecompensa slot, ref int din, ref int tam, ref int velAE, ref int velAP, ref int velP, ref int pBalas, ref int pRico)
    {
        switch (slot)
        {
            case TipoRecompensa.Dinheiro: din++; break;
            case TipoRecompensa.TamanhoEspada: tam++; break;
            case TipoRecompensa.VelocidadeAtaqueEspada: velAE++; break;
            case TipoRecompensa.VelocidadeAtaquePistola: velAP++; break;
            case TipoRecompensa.VelocidadePlayer: velP++; break;
            case TipoRecompensa.PistolBalas: pBalas++; break;
            case TipoRecompensa.PistolRicochete: pRico++; break;
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
        if (quantidade < 2 || scriptPlayer == null) return;
        Sword espadaEquipada = scriptPlayer.ObterArmaPorTipo(TipoArma.Espada) as Sword;
        if (espadaEquipada != null)
        {
            int gomos = quantidade switch { 2 => gomosPequeno, 3 => gomosGrande, _ => 0 };
            espadaEquipada.MudarQuantidadeSegmentos(espadaEquipada.QuantidadeSegmentosMeio + gomos);
        }
    }

    private void AplicarRecompensaVelAtaqueEspada(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        float vel = quantidade switch { 2 => velAtaqueEspadaPequeno, 3 => velAtaqueEspadaGrande, _ => 0f };

        Sword espada = scriptPlayer.ObterArmaPorTipo(TipoArma.Espada) as Sword;
        if (espada != null)
        {
            espada.weaponAttackSpeed += vel; 
            Debug.Log($"[UPGRADE] Velocidade da Espada aumentada para {espada.weaponAttackSpeed}");
        }
    }

    private void AplicarRecompensaVelAtaquePistola(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        float vel = quantidade switch { 2 => velAtaquePistolaPequeno, 3 => velAtaquePistolaGrande, _ => 0f };

        Pistol pistola = scriptPlayer.ObterArmaPorTipo(TipoArma.Pistola) as Pistol;
        if (pistola != null)
        {
            pistola.weaponAttackSpeed += vel;
            Debug.Log($"[UPGRADE] Velocidade da Pistola aumentada para {pistola.weaponAttackSpeed}");
        }
    }

    private void AplicarRecompensaVelocidadePlayer(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        float vel = quantidade switch { 2 => velPlayerPequeno, 3 => velPlayerGrande, _ => 0f };
        scriptPlayer.AumentarVelocidade(vel); 
    }

    private void AplicarRecompensaPistolBalas(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        Pistol pistola = scriptPlayer.ObterArmaPorTipo(TipoArma.Pistola) as Pistol;
        if (pistola != null)
        {
            int adicionais = quantidade switch { 2 => balasAdicionaisPequeno, 3 => balasAdicionaisGrande, _ => 0 };
            pistola.QuantidadeBalas += adicionais; 
            Debug.Log($"[UPGRADE] Pistola agora dispara {pistola.QuantidadeBalas} balas!");
        }
    }

    private void AplicarRecompensaPistolRicochete(int quantidade)
    {
        if (quantidade < 2 || scriptPlayer == null) return;
        Pistol pistola = scriptPlayer.ObterArmaPorTipo(TipoArma.Pistola) as Pistol;
        if (pistola != null)
        {
            int adicionais = quantidade switch { 2 => ricochetesAdicionaisPequeno, 3 => ricochetesAdicionaisGrande, _ => 0 };
            pistola.QuantidadeRicochetes += adicionais; 
            Debug.Log($"[UPGRADE] Projetil da Pistola agora ricocheteia +{adicionais} vezes!");
        }
    }
}