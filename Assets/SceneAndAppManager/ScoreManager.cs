using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Hub")
        {
            if(!PlayerPrefs.HasKey("HighScore")) PlayerPrefs.SetInt("HighScore", 0);
            if(!PlayerPrefs.HasKey("LastScore")) PlayerPrefs.SetInt("LastScore", 0);
            GameObject.FindWithTag("HighScore").GetComponent<TextMeshProUGUI>().text =
                "High Score: " + PlayerPrefs.GetInt("HighRecord");
            GameObject.FindWithTag("LastScore").GetComponent<TextMeshProUGUI>().text =
                "Last Score: " + PlayerPrefs.GetInt("LastRecord");
        }
    }

    public void SaveRecord()
    {
        Debug.Log("SaveRecord");
        PointsManager manager = GameObject.FindWithTag("PointsManager").GetComponent<PointsManager>();
        if (PlayerPrefs.HasKey("HighRecord"))
        {
            if(PlayerPrefs.GetInt("HighRecord") < manager.darkEnergyPoints) 
                PlayerPrefs.SetInt("HighRecord", manager.darkEnergyPoints);
        }
        else
        {
            PlayerPrefs.SetInt("HighRecord", manager.darkEnergyPoints);
        }
        PlayerPrefs.SetInt("LastRecord", manager.darkEnergyPoints);
    }

    public void DeleteRecords()
    {
        PlayerPrefs.SetInt("HighRecord", 0);
        PlayerPrefs.SetInt("LastRecord", 0);
    }
}
