using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MonoPersistency;

public class CharacterSelector : MonoBehaviour {

    public Button startButton;
    public MarinesType selectedMarine;

    public InputField charName;

	// Use this for initialization
	void Start () {
	   
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LaunchGame()
    {
        SaveSystem.Registery.m_snapshot.m_partyName = charName.text;
        DataContainer playerContainer = new DataContainer("player");
        playerContainer.Addvalue("marine", selectedMarine);
        //...
        SaveSystem.RegisterPlayer(playerContainer);
        SaveSystem.LoadScene("SampleLab");
    }
}
