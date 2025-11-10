using System.Collections;
using UnityEngine;

public class teleporter : MonoBehaviour
{
    public GameObject teleporter1;
    public GameObject teleporter2;
    private int count = 3;

    private IEnumerator teleporterCoroutine()
    {
        count = count - 3;
        yield return new WaitForSeconds(3);
        count = 3;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == teleporter1 && count == 3)
        {
            this.gameObject.transform.position = teleporter2.transform.position;
            StartCoroutine(teleporterCoroutine());
        }
        else if(other.gameObject == teleporter2 && count == 3)
        {
            this.gameObject.transform.position = teleporter1.transform.position;
            StartCoroutine (teleporterCoroutine());
        }
    }
}
