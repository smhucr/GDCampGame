using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public enum Takim
{
    Mavi,
    Kirmizi,
    Neutral
}
public class Hex : MonoBehaviour
{
    public Vector2Int id;
    public int askersayisi;
    public bool tiklanabilirlik = false;
    public List<Hex> komsular = new List<Hex>();
    public Takim takim = Takim.Neutral; // Baslangicta tum hex'ler notr
    public bool isBase = false; // Bir hex'in base olup olmadigini kontrol etmek icin
    public int baseCan = 100;  // Eger bir hex base ise bu degeri kullanabiliriz
    //textmeshpro soldiertext;
    public TextMeshPro askerSayisiText;
    public Material maviMaterial;
    public Material kirmiziMaterial;
    public Material neutralMaterial;

    public void UpdateAskerSayisiText()
    {
        if (askerSayisiText != null)
            askerSayisiText.text = askersayisi.ToString();
    }
    public void SetAsBase(Takim takim)
    {
        this.takim = takim;
        this.isBase = true;
    }

    public void AssignToTeam(Takim takim)
    {
        this.takim = takim;
        UpdateHexColor();
    }

    public void AddSoldier(Takim playerTeam)
    {
        print(playerTeam);
        if (takim == playerTeam || takim == Takim.Neutral)
        {
            askersayisi++;
            AssignToTeam(playerTeam);
            print(takim);
        }

        UpdateAskerSayisiText();
    }


    void UpdateHexColor()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        switch (takim)
        {
            case Takim.Mavi:
                renderer.sharedMaterial = maviMaterial;
                break;
            case Takim.Kirmizi:
                renderer.sharedMaterial = kirmiziMaterial;
                break;
            default:
                renderer.sharedMaterial = neutralMaterial;
                break;
        }
    }
}

