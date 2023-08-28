using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


public class HexGrid : MonoBehaviour
{
    private List<Hex> hexes = new List<Hex>();
    public GameObject hexPrefab;
    public GameObject basePrefab;
    public int width = 10;
    public int height = 10;

    private float hexOffsetX = 2.0f;
    private float hexOffsetZ = 1.73f;
    private Takim currentTurn = Takim.Mavi; // Baslangicta mavi takimin turunu baslatiyoruz.
    public int totalMovesPerTurn = 3; // ornek olarak her tur icin 3 hamle izni verdik.
    public GameObject textMeshProPrefab;


    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int j = 0; j < height; j++)
        {
            int currentWidth = width; // default deger
            if (j % 2 == 1)
            {
                currentWidth--; // Tek sayili j degerleri icin width 1 azaltiliyor.
            }

            for (int i = 0; i < currentWidth; i++) // Tek sayili siralarda width - 1 kadar donuyor
            {
                GameObject hexObject = Instantiate(hexPrefab, PositionForIJ(i, j), Quaternion.identity);
                hexObject.transform.parent = this.transform;

                Hex newHex = hexObject.GetComponent<Hex>();
                newHex.id = new Vector2Int(i, j);

                Vector3 prefabScale = textMeshProPrefab.transform.localScale;
                Vector3 prefabPosition = textMeshProPrefab.transform.localPosition;
                Quaternion prefabRotation = textMeshProPrefab.transform.localRotation;

                GameObject tmpObject = Instantiate(textMeshProPrefab);
                tmpObject.transform.SetParent(hexObject.transform);

                tmpObject.transform.localPosition = prefabPosition;
                tmpObject.transform.localRotation = prefabRotation;
                tmpObject.transform.localScale = prefabScale;

                TextMeshPro tmp = tmpObject.GetComponent<TextMeshPro>();
                tmp.text = "0";
                newHex.askerSayisiText = tmp;

                hexes.Add(newHex);
            }
        }

        int middleRow = height / 2;

        Hex maviBaseHex = hexes.FirstOrDefault(h => h.id.x == 0 && h.id.y == middleRow);
        if (maviBaseHex != null)
        {
            maviBaseHex.SetAsBase(Takim.Mavi);
            Instantiate(basePrefab, maviBaseHex.transform.position, Quaternion.identity);
        }

        Hex kirmiziBaseHex = hexes.FirstOrDefault(h => h.id.x == width - 1 && h.id.y == middleRow);
        if (kirmiziBaseHex != null)
        {
            kirmiziBaseHex.SetAsBase(Takim.Kirmizi);
            Instantiate(basePrefab, kirmiziBaseHex.transform.position, Quaternion.identity);
        }

        foreach (var hex in hexes)
        {
            hex.komsular = komsubul(hex);
        }
    }


    Vector3 PositionForIJ(int i, int j)
    {
        float x = i * hexOffsetX;
        if (j % 2 == 1) x += hexOffsetX / 2;
        float z = j * hexOffsetZ;
        return new Vector3(x, 0, z);
    }

    void Update()
    {
        switch (GameManager.instance.roundcount)
        {
            case 1:
            tiklanabilirlikleriSifirla();
            currentTurnBaseHexKomsulari();
                if (Input.GetMouseButtonDown(0) && getselectedhex().tiklanabilirlik)
                {
                    Hex hex = getselectedhex();
                    hex.askersayisi += 5;
                    hex.UpdateAskerSayisiText();
                    hex.AssignToTeam(currentTurn);
                    EndTurn();
                }
                break; // This break is required to exit the case.

            case 2:
                if (Input.GetMouseButtonDown(0))
                {
                    Hex hex = getselectedhex();
                    tiklanabilirlikleriGuncelle();
                    tiklananHexKomsulari(hex);
                }
                break; // This break is required to exit the case.
        }
    }

    Hex getselectedhex()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        Hex selectedHex = null; // Declare selectedHex here

        if (Physics.Raycast(ray, out hitInfo))
        {
            selectedHex = hitInfo.collider.GetComponent<Hex>();
        }

        return selectedHex;
    }


    void EndTurn()
    {
        if (currentTurn == Takim.Mavi)
        {
            currentTurn = Takim.Kirmizi;
            GameManager.instance.currentTurn = Takim.Kirmizi;
        }

        else if (currentTurn == Takim.Kirmizi)
        {
            currentTurn = Takim.Mavi;
            GameManager.instance.currentTurn = Takim.Mavi;
            GameManager.instance.roundcount++;
            if (GameManager.instance.roundcount >= 5)
            {
                GameManager.instance.roundcount = 3;
            }
        }

        // Tur sonu bilgilendirmesi
        Debug.Log($"{currentTurn} Takiminin Turu Basliyor!");
    }
    List<Hex> komsubul(Hex centerHex)
    {
        Vector2Int centerID = centerHex.id;
        List<Vector2Int> neighborOffsets;

        if (centerID.y % 2 == 0)
        {
            // cift satirlar icin komsu ofsetleri
            neighborOffsets = new List<Vector2Int>
            {
                new Vector2Int(-1, 0),
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 1)
            };
        }
        else
        {
            // Tek satirlar icin komsu ofsetleri
            neighborOffsets = new List<Vector2Int>
            {
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(1, -1),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(0, 1)
            };
        }

        List<Hex> neighbors = new List<Hex>();

        foreach (var offset in neighborOffsets)
        {
            Vector2Int neighborID = centerID + offset;
            Hex neighbor = hexes.FirstOrDefault(h => h.id == neighborID);
            if (neighbor != null) neighbors.Add(neighbor);
        }

        return neighbors;
    }

    //tur hangi oyuncudaysa onun takimina ait hexleri tiklanabilir yap (base haric), sag tik ile tiklanabilirlikleri sifirla
    public void tiklanabilirlikleriGuncelle()
    {
        foreach (var item in hexes)
        {
            if (item.takim == GameManager.instance.currentTurn && !item.isBase)
            {
                item.tiklanabilirlik = true;
            }
            else
            {
                item.tiklanabilirlik = false;
            }
        }
    }

    //tiklanan hex'in komsularini tiklanabilir yap kendisi ve komsulari disindakileri tiklanabilirliklerini false yap
    public void tiklananHexKomsulari(Hex tiklananHex)
    {
        foreach (var item in hexes)
        {
            if (item == tiklananHex)
            {
                item.tiklanabilirlik = true;
            }
            else
            {
                item.tiklanabilirlik = false;
            }
        }
        foreach (var item in tiklananHex.komsular)
        {
            item.tiklanabilirlik = true;
        }
    }


    //mavi base hexin komsularini bulup tiklanabilirliklerini true yapar
    public void maviBaseHexKomsulari()
    {
        Hex maviBaseHex = hexes.FirstOrDefault(h => h.id.x == 0 && h.id.y == height / 2);
        if (maviBaseHex != null)
        {
            maviBaseHex.tiklanabilirlik = false;
            foreach (var item in maviBaseHex.komsular)
            {
                item.tiklanabilirlik = true;
            }
        }
    }

    //kirmizi base hexin komsularini bulup tiklanabilirliklerini true yapar
    public void kirmiziBaseHexKomsulari()
    {
        Hex kirmiziBaseHex = hexes.FirstOrDefault(h => h.id.x == width - 1 && h.id.y == height / 2);
        if (kirmiziBaseHex != null)
        {
            kirmiziBaseHex.tiklanabilirlik = false;
            foreach (var item in kirmiziBaseHex.komsular)
            {
                item.tiklanabilirlik = true;
            }
        }
    }

    //currentturn takiminin base hexinin komsularini bulup tiklanabilirliklerini true yapar
    public void currentTurnBaseHexKomsulari()
    {
        if (GameManager.instance.currentTurn == Takim.Mavi)
        {
            maviBaseHexKomsulari();
        }
        else if (GameManager.instance.currentTurn == Takim.Kirmizi)
        {
            kirmiziBaseHexKomsulari();
        }
    }

    //tum hexlerin tiklanabilirliklerini false yapar
    public void tiklanabilirlikleriSifirla()
    {
        foreach (var item in hexes)
        {
            item.tiklanabilirlik = false;
        }
    }


}
