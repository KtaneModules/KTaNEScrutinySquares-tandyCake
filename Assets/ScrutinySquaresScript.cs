using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ScrutinySquaresScript : MonoBehaviour {

    static readonly Dictionary<SquareColor, Color> colorLookup = new Dictionary<SquareColor, Color>()
    {
        { SquareColor.Red, Color.red },
        { SquareColor.Green, Color.green },
        { SquareColor.Blue, Color.blue },
        { SquareColor.Cyan, Color.cyan },
        { SquareColor.Pink, Color.magenta },
        { SquareColor.Yellow, Color.yellow },
        { SquareColor.Black, Color.black },
        { SquareColor.White, Color.white }
    };
    static readonly Dictionary<Property, string> propertyLogMessages = new Dictionary<Property, string>()
    {
        { Property.Word, "word" },
        { Property.WordColor, "color of the word" },
        { Property.Background, "color of the background" },
        { Property.Border, "color around the word" }
    };

    const int MIN_PATH = 5;
    const int MAX_PATH = 7;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMRuleSeedable Ruleseed;

    public KMSelectable[] arrows;
    public KMSelectable statusLight;

    public MeshRenderer background, top, left, bottom, right;
    public TextMesh text;

    private SquareInfo[] grid = new SquareInfo[25];

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    int currentCell;
    List<int> path = new List<int>(25);
    int pathLength;
    List<SquareInfo> pathCells;
    int pathPointer = 0;
    int[] dirSfx;

    void Awake () {
        moduleId = moduleIdCounter++;
        GetRuleseed();
        for (int i = 0; i < 4; i++)
        {
            int ix = i;
            arrows[ix].OnInteract += () => { Move((Dir)ix); return false; };
        }
        statusLight.OnInteract += () => { Submit(); return false; };
        dirSfx = Enumerable.Range(0, 4).ToArray().Shuffle();
    }
    void GetRuleseed()
    {
        MonoRandom rng = Ruleseed.GetRNG();
        SquareColor[,] table = new SquareColor[25,4];
        for (int curCell = 0; curCell < 25; curCell++)
        {
            bool isValid = true;
            for (int prop = 0; prop < 4; prop++)
                table[curCell, prop] = (SquareColor)rng.Next(8);
            for (int prevCell = 0; prevCell < curCell; prevCell++)
            {
                int score = 0;
                for (int prop = 0; prop < 4; prop++)
                    if (table[prevCell, prop] == table[curCell, prop])
                        score++;
                if (score > 1)
                    isValid = false;
            }
            if (!isValid || table[curCell, 1] == table[curCell, 2] ||
                (table[curCell, 1] == SquareColor.White && table[curCell, 2] == SquareColor.Yellow) ||
                (table[curCell, 1] == SquareColor.Yellow && table[curCell, 2] == SquareColor.White) ||
                (table[curCell, 1] == SquareColor.Pink && table[curCell, 2] == SquareColor.Red) ||
                (table[curCell, 1] == SquareColor.Red && table[curCell, 2] == SquareColor.Pink) ||
                (table[curCell, 1] == SquareColor.Green && table[curCell, 2] == SquareColor.Cyan) ||
                (table[curCell, 1] == SquareColor.Cyan && table[curCell, 2] == SquareColor.Green)) 
            {
                curCell--;
            }
        }
        for (int i = 0; i < 25; i++)
        {
            grid[i] = new SquareInfo(table[i, 0], table[i, 1], table[i, 2], table[i, 3]);
            //Log(grid[i].ToString());
        }
    }

    void Start ()
    {
        GeneratePath();
        SetCells();
        SetDisplay(pathCells[pathPointer]);
        LogAnswer(pathCells[pathPointer]);
    }

    void GeneratePath()
    {
        currentCell = Rnd.Range(0, 25);
        path.Add(currentCell);
        int tracingCell = currentCell; 
        bool[] visited = Enumerable.Repeat(false, 25).ToArray();

        while (GetAdjacents(tracingCell).Any(p => !visited[p]))
        {
            visited[tracingCell] = true;
            KeyValuePair<Dir, int> chosenMovement = GetMovements(tracingCell).PickRandom(mvnt => !visited[mvnt.Value]);
            grid[tracingCell].direction = chosenMovement.Key;
            tracingCell = chosenMovement.Value;
            path.Add(tracingCell);
        }
        pathLength = Math.Min(path.Count, Rnd.Range(MIN_PATH, MAX_PATH + 1));
        Log("Path generated: {0}.", path.Take(pathLength).Select(x => GetCoordinate(x)).Join(">"));
        pathCells = path.Take(pathLength).Select(x => grid[x]).ToList();   
    }
    void SetCells()
    {
        for (int i = 0; i < pathLength - 1; i++)
            pathCells[i].Alter();
        pathCells[pathLength - 1].AlterTwo();
    }

    void SetDisplay(SquareInfo curInfo)
    {

        text.text = curInfo.GetProp(Property.Word).ToString().ToUpper();
        text.color = colorLookup[curInfo.GetProp(Property.WordColor)];
        background.material.color = colorLookup[curInfo.GetProp(Property.Background)];

        float topWidth = 0.005f + 0.015f * curInfo.GetProp(Property.Word).ToString().Length;
        float sideDist = topWidth / 2 - .0015f;

        top.transform.localScale = new Vector3(topWidth, .004f, 1);
        bottom.transform.localScale = new Vector3(topWidth, .004f, 1);
        right.transform.localPosition = new Vector3(sideDist, 0, 0);
        left.transform.localPosition = new Vector3(-sideDist, 0, 0);
        foreach (MeshRenderer rend in new[] { top, bottom, left, right })
            rend.material.color = colorLookup[curInfo.GetProp(Property.Border)];

        
    }
    void LogAnswer(SquareInfo cell)
    {
        Log("Current cell: {0}.", cell);
        if (cell.alteredProps.Count > 1)
            Log("The {0} and the {1} are both wrong; you should submit here.", propertyLogMessages[cell.alteredProps[0]], propertyLogMessages[cell.alteredProps[1]]);
        else Log("The {0} is wrong; you should press {1}.", propertyLogMessages[cell.alteredProps.Single()], cell.direction);
    }

    void Move(Dir d)
    {
        Audio.PlaySoundAtTransform("valentine " + dirSfx[(int)d], transform);
        if (moduleSolved)
            return;
        if (grid[currentCell] == pathCells.Last())
        {
            Log("Tried to move while on the final cell, strike.");
            Module.HandleStrike();
        }
        else if (d == pathCells[pathPointer].direction)
        {
            Log("You correctly pressed {0}.", d);
            pathPointer++;
            currentCell = path[pathPointer];
            SetDisplay(pathCells[pathPointer]);
            LogAnswer(pathCells[pathPointer]);
        }
        else
        {
            Log("You pressed {0} when you should have pressed {1}. Strike!", d, pathCells[pathPointer].direction);
            Module.HandleStrike();
        }
    }
    void Submit()
    {
        if (moduleSolved)
            return;
        if (grid[currentCell] == pathCells.Last())
            Solve();
        else
        {
            Log("Tried to submit when not on the final cell. Strike!");
            Module.HandleStrike();
        }
    }

    void Solve()
    {
        moduleSolved = true;
        Audio.PlaySoundAtTransform("solve", transform);
        Log("Submitted on the final cell. Module solved!");
        Module.HandlePass();
        StartCoroutine(SolveAnim());
    }

    IEnumerator SolveAnim()
    {
        SetDisplay(new SquareInfo(SquareColor.Black, SquareColor.Black, SquareColor.Black, SquareColor.Black));
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            SetDisplay(new SquareInfo(SquareColor.SOLVED, SquareColor.Green, SquareColor.Black, SquareColor.Green));
            yield return new WaitForSeconds(0.875f);
            SetDisplay(new SquareInfo(SquareColor.SOLVED, SquareColor.Black, SquareColor.Green, SquareColor.Black));
            yield return new WaitForSeconds(0.875f);
        }
    }

    Dictionary<Dir, int> GetMovements(int pos)
    {
        Dictionary<Dir, int> output = new Dictionary<Dir, int>();
        if (pos >= 5) output.Add(Dir.Up, pos - 5);
        if (pos < 20) output.Add(Dir.Down, pos + 5);
        if (pos % 5 != 0) output.Add(Dir.Left, pos - 1);
        if (pos % 5 != 4) output.Add(Dir.Right, pos + 1);
        return output;
    }
    IEnumerable<int> GetAdjacents(int pos)
    {
        return GetMovements(pos).Values;
    }
    string GetCoordinate(int pos)
    {
        return "ABCDE"[pos % 5].ToString() + (pos / 5 + 1);
    }
    void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Scrutiny Squares #{0}] {1}", moduleId, string.Format(msg, args));
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} U/L/D/R> to move up, left, down, or right. Use <!{0} submit> to submit.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        if (Regex.IsMatch(command, @"^(U(P)?|R(IGHT)?|D(OWN)?|L(EFT)?)$"))
        {
            yield return null;
            arrows["URDL".IndexOf(command[0])].OnInteract();
        }
        else if (Regex.IsMatch(command, @"^S(UBMIT)?$"))
        {
            yield return null;
            statusLight.OnInteract();
        }
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        while (!moduleSolved)
        {
            if (grid[currentCell] == pathCells.Last())
                statusLight.OnInteract();
            else
            {
                arrows[(int)grid[currentCell].direction].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}
