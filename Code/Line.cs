using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{

    public DirectionalID data = new();

    public List<int> LeftLinkList = new();
    public List<int> RightLinkList = new();

    public bool IsSelected;

    public enum Direction { Horizontal, Vertical }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IsSelected = true;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;

        GameHandler.Instance.SelectedMode.Processing(data.id, data.direction);

        gameObject.GetComponent<BoxCollider2D>().enabled = false;

    }
    public class DirectionalID
    {
        public int id;
        public Direction direction;

        public DirectionalID() { }

        public DirectionalID(int id, Direction direction)
        {
            this.id = id;
            this.direction = direction;
        }
        public void SetData(int id, Direction direction)
        {
            this.id = id;
            this.direction = direction;
        }
    }
}
