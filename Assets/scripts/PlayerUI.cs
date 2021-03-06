using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Components")]
    public Image image;

    [Header("Child Text Objects")]
    public Text playerNameText;
    public Text playerDataText;

    Player player;

    bool isWin = false;

    /// <summary>
    /// Caches the controlling Player object, subscribes to its events
    /// </summary>
    /// <param name="player">Player object that controls this UI</param>
    /// <param name="isLocalPlayer">true if the Player object is the Local Player</param>
    public void SetPlayer(Player player, bool isLocalPlayer)
    {
        // cache reference to the player that controls this UI object
        this.player = player;

        isWin = false;

        // subscribe to the events raised by SyncVar Hooks on the Player object
        player.OnPlayerNumberChanged += OnPlayerNumberChanged;
        player.OnPlayerColorChanged += OnPlayerColorChanged;
        player.OnPlayerDataChanged += OnPlayerDataChanged;

        // add a visual background for the local player in the UI
        if (isLocalPlayer)
            image.color = new Color(1f, 1f, 1f, 0.1f);
    }

    void OnDisable()
    {
        player.OnPlayerNumberChanged -= OnPlayerNumberChanged;
        player.OnPlayerColorChanged -= OnPlayerColorChanged;
        player.OnPlayerDataChanged -= OnPlayerDataChanged;

        isWin = false; 
    }

    // This value can change as clients leave and join
    void OnPlayerNumberChanged(int newPlayerNumber)
    {
        playerNameText.text = string.Format(player.Name + " {0:00}", newPlayerNumber);
    }

    // Random color set by Player::OnStartServer
    void OnPlayerColorChanged(Color32 newPlayerColor)
    {
        playerNameText.color = newPlayerColor;
    }

    // This updates from Player::UpdateData via InvokeRepeating on server
    void OnPlayerDataChanged(int newPlayerData)
    { 
        if(newPlayerData == 1)
            player.isWin = true;

        // Show the data in the UI
        if(!player.isWin)
            playerDataText.text = string.Format("To target {0:00}", newPlayerData);
         else 
            playerDataText.text = "FINISHED !";            
    }

}

