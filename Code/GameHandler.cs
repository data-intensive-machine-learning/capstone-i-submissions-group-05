using System;
using System.Collections.Generic;
using UnityEngine;
using static Line;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;

    public Mode SelectedMode;

    public Camera MainCamera;
    public Transform Pointer, PointHolder, WallHolder, StampHolder, HorizontalLines, VerticalLines;
    public LineRenderer WallMaker;

    int MaxRow = 4;
    int MaxColumn = 4;
    int space = 10;

    private void Awake()
    {
        Instance = this;
        SelectedMode = new Mode.Point();
    }
    private void Update()
    {
        SelectedMode.Update();
    }


    public abstract class Mode
    {
        public abstract void Update();
        public abstract void Processing(int id);
        public abstract void Processing(int id, Direction direction);

        public class Point : Mode//******************************************This is Point Class******************************************
        {

            List<global::Point> PointList = new();
            List<Transform> BoxList = new();

            int selectedId = -1;

            public Point()
            {
                PointCreation();
            }
            void PointCreation() 
            {
                int posY = Instance.MaxRow * (Instance.space / 2), i = 0;
                for (int row = 0; row < (Instance.MaxRow + 1); row++)
                {
                    int posX = -(Instance.MaxColumn * (Instance.space / 2));

                    for (int column = 0; column < (Instance.MaxColumn + 1); column++)
                    {
                        Transform point = Instantiate(GameAssets.Instance.Point, Instance.PointHolder).transform;
                        point.position = new Vector2(posX, posY);
                        global::Point p = point.GetComponent<global::Point>();
                        p.Id = i++;
                        PointList.Add(p); //Add Point script in the List


                        if ((row == 0 || row == Instance.MaxRow) && (column == 0 || column == Instance.MaxColumn))
                        {
                            p.MaxLinks = 2;
                        }
                        else if (row == 0 || row == Instance.MaxRow || column == 0 || column == Instance.MaxColumn)
                        {
                            p.MaxLinks = 3;
                        }
                        else
                        {
                            p.MaxLinks = 4;
                        }


                        posX += Instance.space;
                    }
                    posY -= Instance.space;
                }
            }
            public override void Update()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Instance.Pointer.gameObject.SetActive(true);
                    Instance.Pointer.position = Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition); // get current touch position at the screen

                    if (selectedId != -1)
                    {
                        Instance.WallMaker.SetPosition(1, Instance.Pointer.position);
                        Instance.WallMaker.gameObject.SetActive(true);
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    Instance.Pointer.position = Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
                    if (Instance.WallMaker.gameObject.activeInHierarchy)
                    {
                        Instance.WallMaker.SetPosition(1, Instance.Pointer.position);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    Instance.Pointer.gameObject.SetActive(false);
                    if (selectedId != -1)
                    {
                        Instance.WallMaker.gameObject.SetActive(false);
                        selectedId = -1;
                    }
                }
            }
            public override void Processing(int id)
            {

                if (selectedId == id) return;

                if (selectedId == -1)
                {
                    selectedId = id;
                    Instance.WallMaker.SetPosition(0, PointList[id].transform.position);
                    Instance.WallMaker.SetPosition(1, PointList[id].transform.position);
                    Instance.WallMaker.gameObject.SetActive(true);
                }
                else if (InRange(id) && CanAttach(id))
                {
                    PointList[id].SetAttachedId(selectedId);
                    PointList[selectedId].SetAttachedId(id);

                    LineDraw(selectedId, id);

                    Instance.WallMaker.gameObject.SetActive(false);
                    selectedId = -1;
                    StampDetection(id);
                }
            }
            bool InRange(int id) => (Mathf.Abs(selectedId - id) == 1 && (selectedId / (Instance.MaxColumn + 1) == id / (Instance.MaxColumn + 1))) || Mathf.Abs(selectedId - id) == (Instance.MaxColumn + 1);
            bool CanAttach(int id)
            {
                foreach (int value in PointList[id].AttachedId) if (value == selectedId) return false;
                
                return true;
            }
            void LineDraw(int id_1, int id_2)
            {
                LineRenderer wall = Instantiate(GameAssets.Instance.WallMaker, Instance.WallHolder).GetComponent<LineRenderer>();
                wall.SetPosition(0, PointList[id_1].transform.position);
                wall.SetPosition(1, PointList[id_2].transform.position);
            }
            void StampDetection(int id)
            {
                int upperId = id - (Instance.MaxColumn + 1), rightId = id + 1, downId = id + Instance.MaxColumn + 1, leftId = id - 1;
                bool Up = false, Right = false, Down = false, Left = false;

                foreach (int value in PointList[id].AttachedId)
                {
                    if (value == upperId) Up = true;
                    else if (value == rightId) Right = true;
                    else if (value == downId) Down = true;
                    else if (value == leftId) Left = true;
                }

                if (Up && Right) StampDetection(upperId, rightId, 1);

                if (Down && Right) StampDetection(downId, rightId, 1);

                if (Down && Left) StampDetection(downId, leftId, -1);

                if (Up && Left) StampDetection(upperId, leftId, -1);
            }
            void StampDetection(int pointA, int pointB, int direction)
            {
                bool sideA = false, sideB = false;

                foreach (int value in PointList[pointA + direction].AttachedId)
                {
                    if (value == pointA) sideA = true;
                    else if (value == pointB) sideB = true;
                }
                if (sideA && sideB)
                {
                    bool stampPermission = true;
                    Vector3 midPosition = (PointList[pointA].transform.position + PointList[pointB].transform.position) / 2;
                    foreach (Transform value in BoxList)
                    {
                        if (value.position == midPosition)
                        {
                            stampPermission = false;
                            break;
                        }
                    }
                    if (stampPermission)
                    {
                        Transform stamp = Instantiate(GameAssets.Instance.Stamp, Instance.StampHolder).transform;
                        stamp.position = midPosition;
                        BoxList.Add(stamp);
                    }
                }
            }

            public override void Processing(int id, Direction direction){}
        }

        public class Line : Mode//******************************************This is Line Class****************************************** 
        {

            List<global::Line> HorizontalLineList = new();
            List<global::Line> VerticalLineList = new();

            public Line() 
            {
                Instance.Pointer.localScale = new Vector2(.5f, .5f);
                LineCreation();
            }
            void LineCreation()
            {
                //*************Creating Horizontal lines*************
                int posY_H = Instance.MaxRow * (Instance.space / 2), i = 0;

                for (int row = 0; row < (Instance.MaxRow + 1); row++)
                {
                    int posX = -(Instance.MaxColumn - 1) * (Instance.space / 2);

                    for (int column = 0; column < (Instance.MaxColumn); column++)
                    {
                        Transform horizontalLine = Instantiate(GameAssets.Instance.Line, Instance.HorizontalLines).transform;
                        horizontalLine.position = new Vector2(posX, posY_H);
                        global::Line line = horizontalLine.GetComponent <global::Line>();

                        line.data.SetData(i, Direction.Horizontal);//give id and direction to the lines...

                        HorizontalLineList.Add(line);


                        //adding vertical idies to the horizontal lines
                        if (row != 0) // UpSide's
                        {
                            line.LeftLinkList.Add(i - Instance.MaxColumn + row - 1);
                            line.RightLinkList.Add(i - Instance.MaxColumn + row);
                        }
                        if (row != Instance.MaxRow) // DownSide's
                        {
                            line.LeftLinkList.Add(i + row);
                            line.RightLinkList.Add(i + row + 1);
                        }

                        i++;


                        posX += Instance.space;
                    }

                    posY_H -= Instance.space;
                }
                //*****************************Creating Vertical lines****************************************** 
                int posY_V = (Instance.MaxRow - 1) * (Instance.space / 2);
                i = 0;
                for (int row = 0; row < Instance.MaxRow; row++)
                {
                    int posX = -(Instance.MaxColumn * (Instance.space / 2));

                    for (int column = 0; column < (Instance.MaxColumn + 1); column++)
                    {
                        Transform verticalLine = Instantiate(GameAssets.Instance.Line, Instance.VerticalLines).transform;
                        verticalLine.Rotate(0, 0, 90);
                        verticalLine.position = new Vector2(posX, posY_V);
                        global::Line line = verticalLine.GetComponent <global::Line>();

                        line.data.SetData(i++, Direction.Vertical);

                        VerticalLineList.Add(line);

                        posX += Instance.space;
                    }

                    posY_V -= Instance.space;
                }


            }
            public override void Update()
            {

                if (Input.GetMouseButtonDown(0))
                {
                    Instance.Pointer.gameObject.SetActive(true);
                    Instance.Pointer.position = Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition); // get touch position at the screen 

                }
                else if (Input.GetMouseButton(0))
                {
                    Instance.Pointer.position = Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);

                }
                else if (Input.GetMouseButtonUp(0))
                {
                    Instance.Pointer.gameObject.SetActive(false);
                }
            }
            public override void Processing(int id){}

            public override void Processing(int id, Direction direction)
            {
                if (direction == Direction.Horizontal)
                {

                    //****Passes id to the vertical lines****
                    foreach (int value in HorizontalLineList[id].LeftLinkList) VerticalLineList[value].RightLinkList.Add(id);
                    foreach (int value in HorizontalLineList[id].RightLinkList) VerticalLineList[value].LeftLinkList.Add(id);

                    if (id >= Instance.MaxColumn)
                    {
                        if (StampDetection(id, true))// Upward
                        {
                            StampCreation(id, id - Instance.MaxColumn, HorizontalLineList);

                        }

                    }

                    if (id < Instance.MaxRow * Instance.MaxColumn)
                    {
                        if (StampDetection(id, false))// downward
                        {
                            StampCreation(id, id + Instance.MaxColumn, HorizontalLineList);

                        }
                    }
                }
                else
                {

                    if (VerticalLineList[id].RightLinkList.Count == 2 && VerticalLineList[id + 1].IsSelected)
                    {
                        StampCreation(id, id + 1, VerticalLineList);

                    }

                    if (VerticalLineList[id].LeftLinkList.Count == 2 && VerticalLineList[id - 1].IsSelected)
                    {
                        StampCreation(id, id - 1, VerticalLineList);

                    }

                }

            }

            bool StampDetection(int id, bool upSide)
            {
                if (!HorizontalLineList[id + ((upSide ? -1 : 1) * Instance.MaxColumn)].IsSelected) return false;

                if (!VerticalLineList[HorizontalLineList[id].LeftLinkList[upSide ? 0 : ^1]].IsSelected) return false;

                if (!VerticalLineList[HorizontalLineList[id].RightLinkList[upSide ? 0 : ^1]].IsSelected) return false;

                return true;
            }
            void StampCreation(int id_1, int id_2, List<global::Line> list)
            {

                Transform stamp = Instantiate(GameAssets.Instance.Stamp, Instance.StampHolder).transform;

                stamp.position = (list[id_1].transform.position + list[id_2].transform.position) / 2;

            }
        }
    }
}