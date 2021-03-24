using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InputManager : MonoBehaviour
{
    public Camera mainCam; //visualisation
    public int mouse_button = 0;

    public int num_columns = 10; //number of column / horizontal dimension N
    public int num_rows = 10; //number of rows / vertical dimension M

    public Game game; //game simulation
    public DbManager UploadManager; //packaging and uploading functions
    public float prob = 0.8f;




    //game stuff
    public UnityEngine.Object prefab; //the png file for the circles
    public Text score_text;
    public Text win_text;

    public List<int[]> history; //for data collection
    int[,] initial_state; //for data collection
    public int game_id; //the pseudo id from the timedate format

    public bool can_hit = true;
    public bool end = false;
    public bool sent_data = false;

    public GameObject reset_button; //this includes both position of the button and functionality
    public GameObject new_button;

    public GameObject row_object;
    public GameObject col_object;
    public InputField row_field;
    public InputField column_field;

    public GameObject kernel_object;
    public Dropdown kernel_field;

    int NewGameId()
    {
        DateTime time = DateTime.Now;
        int id = time.GetHashCode();
        return game_id;
    }

    // Start is called before the first frame update
    void Start()
    {
        win_text.text = "";
        row_field.text = num_rows.ToString();
        column_field.text = num_columns.ToString();
        can_hit = true;
        end = false;
        sent_data = false;
        UploadManager = new DbManager("http://website.com/SendData.php"); //was changed for personal information
        game_id = NewGameId(); //the identifier of the game
        //if everything is good, we could identify each game by the time and initial state
        //id_text.text = game_id.ToString();


        if(mainCam == null)
        {
            mainCam = Camera.main;
            print(mainCam.aspect);
        }
        game = new Game(num_columns, num_rows, new int[3, 3], mainCam.orthographicSize, mainCam.aspect, false); //create the game
        Setup(num_columns, num_rows, 0);
        history = new List<int[]>();
    }

    // Update is called once per frame
    void Update()
    {
        if (game == null) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchMenu();
            can_hit = !can_hit;
        }
        if (end)
        {
            //if the game has ended display a win screen and send data for storage
            win_text.text = "You Win!";
            if (sent_data) return;
            UploadMoveData(history, true); //we upload the set of moves and the fact that we finished
            sent_data = true;
            return;
        }
        if (!can_hit) return;
        Vector3 mp = Input.mousePosition; //bottom left is (0,0)
        Vector2 normal_mp = new Vector2(mp.x / Screen.width, mp.y / Screen.height);
        int[] p = ScreenPos2Grid(normal_mp);
        int x = p[0];
        int y = p[1];
        game.DrawHit(game.kern, x, y); //we draw where we will hit

        if (Input.GetMouseButtonDown(mouse_button))
        {
            //Debug.Log(mp);
            //Debug.Log(normal_mp);
            //Debug.Log("(" + normal_mp.x + ", " + normal_mp.y + ")");
            //int[] p = ScreenPos2Grid(normal_mp);
            //print("(" + x + ", " + y + ")");
            game.Hit(x, y);
            
            history.Add(p); //p[0] = 0, p[1] = y
            //UploadGameData(x, y);
            //we also package and send the data
            if (game.Win()) //if the game is over we upload
                end = true;

            UpdateScoreText(); //update the current score
            /*
            if(history.Count >= 10)
            {
                string str = "";
                for(int i=0; i<history.Count; i++)
                {
                    str += "(" + history[i][0] + ", " + history[i][1] + "); ";
                }
                print(str);
            }
            */
        }
    }

    void UploadGameData(int move_x, int move_y)
    {
        //move_x is horizontal position - column id
        //move_y is vertical position - row id
        StartCoroutine(UploadManager.UploadGameData(UploadManager.PackageGameData(game_id.ToString(), initial_state, game.kern, history.Count, move_x, move_y)));
    }

    //we want to store player moves for each game
    //so we might want to generate an ID based on a random number for initial state and timestamp
    void UploadMoveData(List<int[]> moves, bool finished)
    {
        StartCoroutine(UploadManager.UploadGameData(UploadManager.PackageMovesData(game_id.ToString(), initial_state, game.kern, moves, finished)));
    }

    int[] ScreenPos2Grid(Vector2 p)
    {
        //assume p is normalised to screen space from bottom left
        //screen starts from bottom left, while matrices start from top left
        float dx = 1 / num_columns;
        float dy = 1 / num_rows;

        //0<=x<v*dx => x=v
        int x = (int)(p.x * num_columns);
        int y = (int)((1f-p.y) * num_rows);
        //print("x = " + x);
        //print("y = " + y);
        return new int[] {x, y};
    }

    public void SwitchMenu() //turn the main menu on/off
    {
        reset_button.SetActive(!reset_button.activeSelf);
        new_button.SetActive(!new_button.activeSelf);
        row_object.SetActive(!row_object.activeSelf);
        col_object.SetActive(!col_object.activeSelf);
        kernel_object.SetActive(!kernel_object.activeSelf);
        score_text.gameObject.SetActive(!score_text.gameObject.activeSelf);
        //reset_button.enabled = !reset_button.enabled;
    }

    public void ResetGame()
    {
        game.SetBoardState(initial_state);
        UploadMoveData(history, false);//upload the unfinished round
        ResetHistory();
        UpdateScoreText(); //update the current score
        game.UpdateVisual();
        game_id = NewGameId();
    }

    public void NewGame()
    {
        int row = num_rows;
        if (row_field.text != "")
            row = int.Parse(row_field.text);
        int col = num_columns;
        if(column_field.text != "")
            col = int.Parse(column_field.text);

        int k_option = kernel_field.value;
        Debug.Log("kernel option value " + k_option);

        UploadMoveData(history, false);//upload that the player haven't won and want new challenge
        //DeletePoints(game.points);
        //DeletePoints(game.potentialHits);
        //game.ResetGame(col, row, game.DefaultKernel(), mainCam.orthographicSize, mainCam.aspect, false);
        //game.SetBoardState(game.GenerateRandomState(0.8f, col, row));
        //game.points = CreatePoints(col, row);
        //game.potentialHits = CreatePoints(col, row);
        //game.UpdateVisual();
        Debug.Log(row + ", " + col);
        num_columns = col;
        num_rows = row;
        game.ResetGame(col, row);
        Setup(col, row, k_option);
        UpdateScoreText();
        ResetHistory();

        game_id = NewGameId();
        initial_state = game.board.Clone() as int[,];
    }

    void UpdateScoreText()
    {
        score_text.text = "Score : " + game.OnCount(); //update the current score
    }

    public void ResetHistory()
    {
        history = new List<int[]>();
    }

    public void Setup(int cols, int rows, int k) //setup the game with some basic settings
    {
        if (game != null)
        {
            DeletePoints(game.points);
            DeletePoints(game.potentialHits);
        }
        if(k==0)
            game.SetKernel(game.KernelAdjecent(3)); //we set the kernel //could be any function that creates it
        if(k==1)
            game.SetKernel(game.KernelCross(3)); //we set the kernel //could be any function that creates it
        if(k==2)
            game.SetKernel(game.KernelCross(5)); //we set the kernel //could be any function that creates it
        game.SetBoardState(game.GenerateRandomState(prob, cols, rows)); //we set the initial board state
        game.points = CreatePoints(cols, rows); //we create a visualisation circles
        game.potentialHits = CreatePoints(game.kern.GetLength(0), game.kern.GetLength(1)); //we create visualisation circles for where we will hit
        game.UpdateVisual(); //do visualisation

        initial_state = game.board.Clone() as int[,]; //we save a copy of the initial board state
        score_text.text = "Score : " + game.OnCount();
    }

    //setup the game
    public void DeletePoints(SpriteRenderer[,] sprites)
    {
        if (sprites == null) return;
        Debug.Log("call");
        for(int i=0; i<sprites.GetLength(0); i++)
        {
            for(int j=0; j<sprites.GetLength(1); j++)
            {
                if (sprites[i, j] == null) continue;
                Destroy(sprites[i, j].gameObject);
            }
        }
    }

    public SpriteRenderer[,] CreatePoints(int numX, int numY)
    {
        SpriteRenderer[,] p = new SpriteRenderer[numX, numY];
        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                //we create the appropriate amount of points
                SpriteRenderer t = (Instantiate(prefab, transform) as GameObject).GetComponent<SpriteRenderer>();
                p[i, j] = t;
            }
        }
        return p;
    }
}
