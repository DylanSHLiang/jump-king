using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Goal : MonoBehaviour
{
    [SerializeReference] GameObject winScreen;
    [SerializeReference] GameObject player;
    private AudioSource audioData;
    private void Start()
    {
        audioData = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("YOU WIN!");
            winScreen.SetActive(true);
            audioData.Play();
            player.GetComponent<PlayerInput>().DeactivateInput();
        }
    }
}
