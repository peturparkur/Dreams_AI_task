using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public Transform parent;
    public int num_column = 10; //number of column / horizontal dimension N
    public int num_rows = 10; //number of rows / vertical dimension M

    public int[,] board; //state of the board
    //1 => up,   -1 => down
    //=> could be boolean state but leaving it as integer for generalisation later for maybe partial states
    //eg.: it takes 2 turns for it to go up/down

    public int[,] kern; //current kernel for hit pattern
                        //by default the hit should flip the adjacent grid points 
                        //=> 
                        //[0, -1, 0] 
                        //[-1, -1, -1]
                        //[0, -1, 0]
                        //in row form

    public bool loop = false; //does the world loop around the edges of the grid
    public float mult_x = 1f;
    public float mult_y = 1f;


    //public Camera cam;
    public float image_size; //orthographic size
    public float aspect_ratio; //aspect ratio of visalisation
    public SpriteRenderer[,] points; //these are the visuals
    //public Object prefab;
    //we want all states to be down
    public Color up_state = new Color(255f,0f,0f,1f); //up state
    public Color down_state = new Color(0f, 0f,255,1f); //down state
    public Color potential_state = new Color(255f, 255f, 255f, 0.5f);
    public SpriteRenderer[,] potentialHits;

    public Game(int dimX, int dimY, int[,] kernel,float cam_size, float aspect_ratio, bool loop=false)
    {
        this.num_column = dimX;
        this.num_rows = dimY;
        EmptyBoard();
        kern = kernel.Clone() as int[,];
        image_size = cam_size;
        this.aspect_ratio = aspect_ratio;
        this.loop = loop;
    }

    public void ResetGame(int dimX, int dimY)
    {
        this.num_column = dimX;
        this.num_rows = dimY;
        EmptyBoard();
    }

    public int OnCount()
    {
        //count the number of on states
        //want to minimise
        int count=0;
        for(int i=0; i<num_column; i++)
        {
            for(int j=0; j<num_rows; j++)
            {
                if (board[i, j] > 0) count += 1;
            }
        }
        return count;
    }

    public float Score()
    {

        //for humans we need to show the inverse of the score
        //so it will be score=1/(1+count)
        //this is to be maximised
        return (num_rows*num_column) / (1f + (float)OnCount());
    }

    public bool Win()
    {
        //if all the lights are blue game is over
        //so we just need to find a single red one
        for(int i=0; i<num_column; i++)
        {
            for(int j=0; j<num_rows; j++)
            {
                if (board[i, j] > 0) return false; //this one is turned on
            }
        }
        return true;
    }

    void Init_Default()
    {
        EmptyBoard();
        SetKernel(DefaultKernel());
    }

    public static void Print2DArray<T>(T[,] matrix)
    {
        string str = "";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                str += matrix[i, j] + " ";
            }
            str += "\n";
        }
        //print(str);
    }

    public static void Fill2DArray<T>(T[,] matrix, T x)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = x;
            }
        }
    }

    //could generalise this to NxN kernels
    public int[,] KernelAdjecent(int n)
    {
        int[,] kernel = new int[n, n];
        int c = n / 2; //int division halving
        for(int i=0; i<n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if(i==c || j==c)
                {
                    kernel[c, i] = -1;
                    kernel[i, c] = -1;
                }
                else
                {
                    kernel[i, j] = 1;
                }
            }
        }
        return kernel;
    }

    public int[,] KernelCross(int n)
    {
        int[,] kernel = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    kernel[i, i] = -1;
                    kernel[i, n - 1 - i] = -1;
                }
                else
                {
                    if (i + j != n - 1)
                    {
                        kernel[i, j] = 1;
                    }
                }
            }
        }
        return kernel;
    }

    public int[,] DefaultKernel() //return an 3x3 kernel for adjacent change
    {
        int[,] kernel = new int[3, 3];
        kernel[0, 0] = kernel[0, 2] = kernel[2, 0] = kernel[2, 2] = 1;
        kernel[0, 1] = kernel[1, 0] = kernel[1, 1] = kernel[1, 2] = kernel[2, 1] = -1;
        Print2DArray(kernel);
        return kernel;
    }
    public int[,] DefaultKernelCross() //return an 3x3 kernel for adjacent change
    {
        int[,] kernel = new int[3, 3];
        kernel[0, 0] = kernel[0, 2] = kernel[2, 0] = kernel[2, 2] = kernel[1, 1] = -1;
        kernel[0, 1] = kernel[1, 0] = kernel[1, 2] = kernel[2, 1] = 1;
        Print2DArray(kernel);
        return kernel;
    }
    public int[,] DefaultKernelBigCross() //return an 5x5 kernel for adjacent change
    {
        int[,] kernel = new int[5, 5];
        for(int i=0; i<5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (i == j)
                {
                    kernel[i, i] = -1;
                    kernel[i, 4 - i] = -1;
                }
                else
                {
                    if (i + j != 5-1)
                    {
                        kernel[i, j] = 1;
                    }
                }
            }
        }
        Print2DArray(kernel);
        return kernel;
    }

    public void SetBoardState(int[,] state)
    {
        board = state.Clone() as int[,]; //copy the state to the board
        //print(board);
    }

    public int[,] GenerateRandomState(float p, int m, int n)
    {
        //generate a random MxN board;
        //with probability p that the value is negative => -1
        //simplest thing => could be extended for more complicated

        int[,] state = new int[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float r = UnityEngine.Random.value; //unity in-built random variable could setup more sophisticated with a seed
                int v = 1;
                if (r < p)
                {
                    v = -1;
                }
                state[i, j] = v;
            }
        }
        return state;
    }

    public void EmptyBoard()
    {
        board = new int[num_column, num_rows];
        Fill2DArray(board, 1);
        Print2DArray(board);
    }

    public void SetKernel(int[,] kernel)
    {
        kern = kernel.Clone() as int[,];
        Print2DArray(kernel);
    }

    public void Hit(int x, int y) //hit action on board point x,y,    where 0<=x<dimX; 0<=y<dimY
    {
        //we apply the pseudo convolution on x,y
        KernelApply(board, kern, x, y); //we might
        Print2DArray(board);
        UpdateVisual();
    }

    void KernelApply(int[,] target, int[,] kernel, int x, int y) //since the target is an array the change is reflected
    {
        int w = kernel.GetLength(0); //width
        int h = kernel.GetLength(1); //height

        int cw = w / 2;
        int ch = h / 2;

        //we apply the kernel centered on x,y (for now assume it's odd wide)
        //if even we might just apply it slighly off centered
        //Debug.Log("center hit = " + x + ", " + y);
        for(int i=0; i < w; i++)
        {
            for(int j=0; j < h; j++)
            {
                int u = x - cw + i;
                int v = y - ch + j;
                if (loop) //would need to change visualisation
                {
                    //Debug.Log("before (u, v) = (" + u + ", " + v + ")");
                    if (u < 0) u = target.GetLength(1) + u;
                    if (v < 0) v = target.GetLength(0) + v;
                    //Debug.Log("after1 (u, v) = (" + u + ", " + v + ")");
                    if (u > target.GetLength(1) - 1) u = -u - target.GetLength(1);
                    if (v > target.GetLength(0) - 1) v = -v - target.GetLength(0);
                   //Debug.Log("after (u, v) = (" + u + ", " + v + ")");
                }
                else
                {
                    if (u < 0 || u > target.GetLength(0) - 1) continue;
                    if (v < 0 || v > target.GetLength(0) - 1) continue;
                }
                //Debug.Log("(u, v) = (" + u + ", " + v + ") => " + kernel[i,j]);
                target[u, v] = target[u, v] * kernel[i, j];
            }
        }
        //Debug.Log("end");

        //multiplies the target by the kernel as the new state
    }

    public void UpdateVisual()
    {
        if (points == null) return;
        //we want to show how the current state looks like
        //the normalised (0,1) center points of the grid is
        float dx = 1 / (float)num_column;
        float dy = 1 / (float)num_rows;
        float d_min = Mathf.Min(dx, dy);
        float size = image_size;
        float ratio = aspect_ratio; //ratio=(width/height)

        for(int i=0; i<num_column; i++)
        {
            for(int j=0; j<num_rows; j++)
            {
                points[i, j].transform.position = new Vector3(mult_x*(i * dx*2f*size*ratio - ratio*size), mult_y*(-(j+1) * dy*2f*size + size), 0);//bacause screen start from bottom left, while matrix from top left
                points[i, j].transform.localScale = new Vector3(mult_x*10f * d_min, mult_y*10f * d_min, 1f);
                //print(new Vector3(i * dx * Screen.width, j * dy * Screen.height, 0));
                points[i, j].color = board[i, j] == 1 ? up_state : down_state;
            }
        }
    }

    public void DrawHit(int[,] kernel, int x, int y)
    {
        if (potentialHits == null) return;

        float dx = 1 / (float)num_column;
        float dy = 1 / (float)num_rows;
        float d_min = Mathf.Min(dx, dy);
        float size = image_size;
        float ratio = aspect_ratio; //ratio=(width/height)

        int cw = kernel.GetLength(0) / 2;
        int ch = kernel.GetLength(1) / 2;

        for (int i=0; i<kernel.GetLength(0); i++)
        {
            for(int j=0; j<kernel.GetLength(1); j++)
            {

                //assume the kernel is centered on x,y
                potentialHits[i,j].transform.position = new Vector3(mult_x*((i + x-cw) * dx * 2f * size * ratio - ratio * size),mult_y*(-((j + y-ch) + 1) * dy * 2f * size + size), -1f);
                potentialHits[i, j].transform.localScale = new Vector3(mult_x*10f * d_min, mult_y*10f * d_min, 1f);
                potentialHits[i, j].color = kernel[i,j] == -1 ? potential_state : new Color(0,0,0,0);
            }
        }
    }
}
