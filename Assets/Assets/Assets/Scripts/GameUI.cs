using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Unity.VisualScripting;


public class GameUI : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI playerInfoText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI winText;
    public Image winBackground;
    public Image deadOrb1;
    public Image deadOrb2;
    public Image deadOrb3;
    public AudioClip wingame;
    public AudioClip loseOrb;


    private PlayerController player;

    public static GameUI instance;

    void Awake()
    {
        instance = this;
    }

    public void Initialize(PlayerController localPlayer)
    {
        player = localPlayer;
        healthBar.maxValue = player.maxHp;
        healthBar.value = player.curHp;

        UpdatePlayerInfoText();
        UpdateAmmoText();
        UpdateOrbs();

    }

    public void UpdateHealthBar()
    {
        healthBar.value = player.curHp;
    }

    public void UpdatePlayerInfoText()
    {
        playerInfoText.text = "<b>Alive:</b> " + GameManager.instance.alivePlayers + "\n <b>Kills:</b> " + player.kills;
    }

    public void UpdateAmmoText()
    {
        ammoText.text = player.weapon.curAmmo + " / " + player.weapon.maxAmmo;
    }

    public void UpdateOrbs()
    {
        if(player.deaths == 1)
        {
             deadOrb1.gameObject.SetActive(true);
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = loseOrb;
            audio.Play();
        }
        else if (player.deaths == 2)
        {
        deadOrb2.gameObject.SetActive(true);
        }
       else if (player.deaths == 3)
       {
           deadOrb3.gameObject.SetActive(true);
       }
        
    }

    public void SetWinText(string winnerName)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = wingame;
        audio.Play();
        winBackground.gameObject.SetActive(true);
        winText.text = winnerName + " wins!";
    }
}
