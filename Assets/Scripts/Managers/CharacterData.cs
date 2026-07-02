using UnityEngine;

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "Jogo/Personagem")]
public class CharacterData : ScriptableObject
{
    public string nomeDoPersonagem;
    public GameObject prefabDoPersonagem; 
    public GameObject prefabDaArmaInicial;
    public int vidaMaxima = 100;
    public float velocidadeMovimento = 5f;
}