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
    private Takım currentTurn = Takım.Mavi; // Başlangıçta mavi takımın turunu başlatıyoruz.
    public int totalMovesPerTurn = 3; // Örnek olarak her tur için 3 hamle izni verdik.
    public GameObject textMeshProPrefab;


    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int j = 0; j < height; j++)
        {
            int currentWidth = width; // default değer
            if (j % 2 == 1)
            {
                currentWidth--; // Tek sayılı j değerleri için width 1 azaltılıyor.
            }

            for (int i = 0; i < currentWidth; i++) // Tek sayılı sıralarda width - 1 kadar dönüyor
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
            maviBaseHex.SetAsBase(Takım.Mavi);
            Instantiate(basePrefab, maviBaseHex.transform.position, Quaternion.identity);
        }

        Hex kırmızıBaseHex = hexes.FirstOrDefault(h => h.id.x == width - 1 && h.id.y == middleRow);
        if (kırmızıBaseHex != null)
        {
            kırmızıBaseHex.SetAsBase(Takım.Kırmızı);
            Instantiate(basePrefab, kırmızıBaseHex.transform.position, Quaternion.identity);
        }

        foreach (var hex in hexes)
        {
            hex.komşular = komşubul(hex);
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
            tıklanabilirlikleriSıfırla();
            currentTurnBaseHexKomşuları();
                if (Input.GetMouseButtonDown(0) && getselectedhex().tıklanabilirlik)
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
                    tıklanabilirlikleriGüncelle();
                    tıklananHexKomşuları(hex);
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
        if (currentTurn == Takım.Mavi)
        {
            currentTurn = Takım.Kırmızı;
            GameManager.instance.currentTurn = Takım.Kırmızı;
        }

        else if (currentTurn == Takım.Kırmızı)
        {
            currentTurn = Takım.Mavi;
            GameManager.instance.currentTurn = Takım.Mavi;
            GameManager.instance.roundcount++;
            if (GameManager.instance.roundcount >= 5)
            {
                GameManager.instance.roundcount = 3;
            }
        }

        // Tur sonu bilgilendirmesi
        Debug.Log($"{currentTurn} Takımının Turu Başlıyor!");
    }
    List<Hex> komşubul(Hex centerHex)
    {
        Vector2Int centerID = centerHex.id;
        List<Vector2Int> neighborOffsets;

        if (centerID.y % 2 == 0)
        {
            // Çift satırlar için komşu ofsetleri
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
            // Tek satırlar için komşu ofsetleri
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

    //tur hangi oyuncudaysa onun takımına ait hexleri tıklanabilir yap (base hariç), sağ tık ile tıklanabilirlikleri sıfırla
    public void tıklanabilirlikleriGüncelle()
    {
        foreach (var item in hexes)
        {
            if (item.takım == GameManager.instance.currentTurn && !item.isBase)
            {
                item.tıklanabilirlik = true;
            }
            else
            {
                item.tıklanabilirlik = false;
            }
        }
    }

    //tıklanan hex'in komşularını tıklanabilir yap kendisi ve komşuları dışındakileri tıklanabilirliklerini false yap
    public void tıklananHexKomşuları(Hex tıklananHex)
    {
        foreach (var item in hexes)
        {
            if (item == tıklananHex)
            {
                item.tıklanabilirlik = true;
            }
            else
            {
                item.tıklanabilirlik = false;
            }
        }
        foreach (var item in tıklananHex.komşular)
        {
            item.tıklanabilirlik = true;
        }
    }


    //mavi base hexin komşularını bulup tıklanabilirliklerini true yapar
    public void maviBaseHexKomşuları()
    {
        Hex maviBaseHex = hexes.FirstOrDefault(h => h.id.x == 0 && h.id.y == height / 2);
        if (maviBaseHex != null)
        {
            maviBaseHex.tıklanabilirlik = false;
            foreach (var item in maviBaseHex.komşular)
            {
                item.tıklanabilirlik = true;
            }
        }
    }

    //kırmızı base hexin komşularını bulup tıklanabilirliklerini true yapar
    public void kırmızıBaseHexKomşuları()
    {
        Hex kırmızıBaseHex = hexes.FirstOrDefault(h => h.id.x == width - 1 && h.id.y == height / 2);
        if (kırmızıBaseHex != null)
        {
            kırmızıBaseHex.tıklanabilirlik = false;
            foreach (var item in kırmızıBaseHex.komşular)
            {
                item.tıklanabilirlik = true;
            }
        }
    }

    //currentturn takımının base hexinin komşularını bulup tıklanabilirliklerini true yapar
    public void currentTurnBaseHexKomşuları()
    {
        if (GameManager.instance.currentTurn == Takım.Mavi)
        {
            maviBaseHexKomşuları();
        }
        else if (GameManager.instance.currentTurn == Takım.Kırmızı)
        {
            kırmızıBaseHexKomşuları();
        }
    }

    //tüm hexlerin tıklanabilirliklerini false yapar
    public void tıklanabilirlikleriSıfırla()
    {
        foreach (var item in hexes)
        {
            item.tıklanabilirlik = false;
        }
    }


}
