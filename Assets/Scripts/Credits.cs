using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour
{

    public GameObject ByLine;
    public GameObject Thanks;
    TextMesh ByMesh;
    TextMesh ThanksMesh;
    AudioSource music;
    int numUpdates;

    // Use this for initialization
    void Start()
    {
        ByLine.SetActive(true);
        ByMesh = ByLine.GetComponent<TextMesh>();
        SetAlpha(ByMesh, 0f);
        Thanks.SetActive(true);
        ThanksMesh = Thanks.GetComponent<TextMesh>();
        SetAlpha(ThanksMesh, 0f);
        music = this.GetComponent<AudioSource>();
        music.Play();
        numUpdates = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (numUpdates <= 20)
        {
            numUpdates++;
        }
        else if (numUpdates <= 70)
        {
            SetAlpha(ByMesh, ByMesh.color.a + 0.02f);
            numUpdates++;
        }
        else if (numUpdates <= 120)
        {
            numUpdates++;
        }
        else if (numUpdates <= 170)
        {
            SetAlpha(ByMesh, ByMesh.color.a - 0.02f);
            numUpdates++;
        }
        else if (numUpdates <= 220)
        {
            numUpdates++;
        }
        else if (numUpdates <= 270)
        {
            SetAlpha(ThanksMesh, ThanksMesh.color.a + 0.02f);
            numUpdates++;
        }
        else if (numUpdates <= 320)
        {
            numUpdates++;
        }
        else if (numUpdates <= 370)
        {
            SetAlpha(ThanksMesh, ThanksMesh.color.a - 0.02f);
            numUpdates++;
        }
        else if (numUpdates <= 420)
        {
            numUpdates++;
        }
        else if (numUpdates <= 470)
        {
            music.volume = music.volume - 0.02f;
            numUpdates++;
        }
        else if (numUpdates == 471)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.Log("Should not reach");
        }
    }

    void SetAlpha(TextMesh mesh, float alpha)
    {
        Color color = mesh.color;
        color.a = alpha;
        mesh.color = color;
    }
}
