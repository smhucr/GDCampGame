using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public enum Takım
{
    Mavi,
    Kırmızı,
    Neutral
}
public class Hex : MonoBehaviour
{
    public Vector2Int id;
    public int askersayisi;
    public bool tıklanabilirlik = false;
    public List<Hex> komşular = new List<Hex>();
    public Takım takım = Takım.Neutral; // Başlangıçta tüm hex'ler nötr
    public bool isBase = false; // Bir hex'in base olup olmadığını kontrol etmek için
    public int baseCan = 100;  // Eğer bir hex base ise bu değeri kullanabiliriz
    //textmeshpro soldiertext;
    public TextMeshPro askerSayisiText;
    public Material maviMaterial;
    public Material kırmızıMaterial;
    public Material neutralMaterial;

    public void UpdateAskerSayisiText()
    {
        if (askerSayisiText != null)
            askerSayisiText.text = askersayisi.ToString();
    }
    public void SetAsBase(Takım takim)
    {
        this.takım = takim;
        this.isBase = true;
    }

    public void AssignToTeam(Takım takim)
    {
        this.takım = takim;
        UpdateHexColor();
    }

    public void AddSoldier(Takım playerTeam)
    {
        print(playerTeam);
        if (takım == playerTeam || takım == Takım.Neutral)
        {
            askersayisi++;
            AssignToTeam(playerTeam);
            print(takım);
        }

        UpdateAskerSayisiText();
    }


    void UpdateHexColor()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        switch (takım)
        {
            case Takım.Mavi:
                renderer.sharedMaterial = maviMaterial;
                break;
            case Takım.Kırmızı:
                renderer.sharedMaterial = kırmızıMaterial;
                break;
            default:
                renderer.sharedMaterial = neutralMaterial;
                break;
        }
    }
}

