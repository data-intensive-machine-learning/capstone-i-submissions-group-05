using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public int Id;
    public int MaxLinks;

    public List<int> AttachedId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameHandler.Instance.SelectedMode.Processing(Id);
    }

    public void SetAttachedId(int id)
    {
        AttachedId.Add(id);

        if(!isAvailable)
        {
            gameObject.GetComponent<CircleCollider2D>().enabled = false;
        }
    }

      bool isAvailable => MaxLinks != AttachedId.Count;
}
