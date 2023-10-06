using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;



public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;


    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    public int deaths;
    public int maxDeaths;
    public AudioClip jump;
    public AudioClip owch;




    private bool flashingDamage;


    [Header("Components")]
    public Rigidbody rig;
    public MeshRenderer mr;
    public Player photonPlayer;
    public PlayerWeapon weapon;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;

        // is this not our local player?
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    void Update()
    {
        if (!photonView.IsMine || dead)
        {
            return;
        }

        Move();
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        rig.velocity = dir;

    }

    void TryJump()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = jump;
        audio.Play();
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = owch;
        audio.Play();
        photonView.RPC("DamageFlash", RpcTarget.Others);

        GameUI.instance.UpdateHealthBar();

        if (curHp <= 0)
        {
            if(deaths == maxDeaths)
                photonView.RPC("Die", RpcTarget.All);

            deaths += 1;
            GameUI.instance.UpdateOrbs();
            photonView.RPC("Respawn", RpcTarget.All);
        }
        //print player and who killed them for BR++
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;

        }
    }

    [PunRPC]
    void Die()
    {
        curHp = 0;
        dead = true;
        GameUI.instance.UpdateOrbs();

        GameManager.instance.alivePlayers--;

        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        if (photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            GetComponentInChildren<CameraController>().SetAsSpectator();
            mr.gameObject.SetActive(false);
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }

    }

    [PunRPC]
    public void AddKill()
    {
        kills++;
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        GameUI.instance.UpdateHealthBar();
    }

    [PunRPC]
    public void Respawn()
    {
        curHp = maxHp;
        GameUI.instance.UpdateHealthBar();
        if (photonView.IsMine)
            transform.position = new Vector3(-14, 1 , 79);
    }
}
