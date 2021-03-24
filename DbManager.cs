using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; //so we can send network packets

public class DbManager //if we want to do more complicated network logic
{
    public string url;
    public DbManager(string url)
    {
        this.url = url;
    }

    IEnumerator UploadData(string url, string field_name, string data) //delegate
    {
        //url is the upload destination URL
        WWWForm form = new WWWForm();
        form.AddField(field_name, data);

        //upload
        using (UnityWebRequest w = UnityWebRequest.Post(url, form))
        {
            yield return w.SendWebRequest(); //send the "command"
            if(w.isNetworkError || w.isHttpError)
            {
                Debug.Log(w.error);
            }
            else
            {

            }
        }
    }

    public string Matrix2String<T>(T[,] matrix)
    {
        string s = "";
        for(int i=0; i<matrix.GetLength(0); i++)
        {
            for(int j=0; j<matrix.GetLength(1); j++)
            {
                s += matrix[i, j].ToString();
                //if we're not in the last column we add the column
                if (j < matrix.GetLength(1) - 1)
                    s += ", ";
            }
            //if we're not in the last row, we add a semi colon to mark the end of the row
            if (i < matrix.GetLength(0) - 1)
                s += "; ";
        }
        return s;
    }

    public string Data2String(List<int[]> moves) //the formating for the list of moves
    {
        string s = "";
        for(int i=0; i<moves.Count; i++)
        {
            s += moves[i][0] + ", " + moves[i][1];
            if (i < moves.Count - 1)
                s += "; ";
        }
        return s;
    }

    public WWWForm PackageGameData(string id, int[,] init_state, int[,] kernel, int move_num, int move_x, int move_y)
    {
        //id is the game_id based on timstamp of the start of the game
        //init_state is the initial state of the board
        //the board is MxN in size
        //kernel is the local rule how the board is updated based on hit position
        //move_num is which move it was
        //move_x is the column position of the hit
        //move_y is the row position of the hit
        WWWForm form = new WWWForm();
        form.AddField("game_id", id);
        form.AddField("init_state", Matrix2String(init_state));
        form.AddField("num_rows", init_state.GetLength(0));
        form.AddField("num_cols", init_state.GetLength(1));
        form.AddField("kernel", Matrix2String(kernel));
        form.AddField("ker_rows", kernel.GetLength(0));
        form.AddField("ker_cols", kernel.GetLength(1));
        form.AddField("move_num", move_num);
        form.AddField("move_x", move_x);
        form.AddField("move_y", move_y);
        return form;
    }

    public WWWForm PackageMovesData(string game_id, int[,] init_state, int[,] kernel, List<int[]> moves, bool win) //package the data into a database format
    {
        WWWForm form = new WWWForm();
        form.AddField("game_id", game_id);
        form.AddField("init_state", Matrix2String(init_state));
        form.AddField("num_rows", init_state.GetLength(0));
        form.AddField("num_cols", init_state.GetLength(1));
        form.AddField("kernel", Matrix2String(kernel));
        form.AddField("ker_rows", kernel.GetLength(0));
        form.AddField("ker_cols", kernel.GetLength(1));
        form.AddField("moves", Data2String(moves));
        form.AddField("finished", win.ToString());
        return form;
    }

    public IEnumerator UploadGameData(string url, WWWForm data) //upload the given data form
    {
        //upload
        Debug.Log("upload to: " + url);
        using (UnityWebRequest w = UnityWebRequest.Post(url, data))
        {
            yield return w.SendWebRequest(); //send the "command"
            if (w.isNetworkError || w.isHttpError)
            {
                Debug.Log(w.error);
            }
            else
            {

            }
        }
    }
    public IEnumerator UploadGameData(WWWForm data)
    {
        return UploadGameData(url, data);
    }

}
