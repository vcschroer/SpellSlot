using UnityEngine;

public class TesteSC2 : MonoBehaviour
{
    // Aqui vemos Zac Zanetti Madrugada Gameplayz em ação, provando que com dedicação e aula de IA consegue coda >:D

    int SlotA = 1;
    int SlotB = 1;
    int V1 = 0; //V1 é de Vitoria 1 (Vitoria do Tipo 1)

    private void Awake()
    {
        switch (SlotA)
        {
            case 1:
                V1 += 1;
                break;
            case 2:
                break;
        }

        switch (SlotB)
        {
            case 1:
                V1 += 1;
                break;
            case 2:
                break;
        }

        if (V1 == 2)
        {
            MinhaFuncao();
        }
    }

    private void MinhaFuncao()
    {
        Debug.Log("Pai ta adaptado");
    }
}
