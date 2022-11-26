using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace MatrixWordChallenge
{
    class Program
    {
        static string MatrixChallenge(string[] strArr)
        {
            string result = "Moo";
            //Construct Grid.
            Grid grid = new Grid(InputWordSplitter(strArr[0]).ToArray());
            //Organize Search Words.
            string[] searchWords = InputWordSplitter(strArr[1]).ToArray();
            //Find the Words in the matrix.
            List<string> missingWords = new List<string>();
            for (int i = 0; i < searchWords.Length; i++)
            {
                //words that cannot be found are added to the missing word list for printing.
                if (!grid.FindWord(searchWords[i]))
                {
                    missingWords.Add(searchWords[i]);
                }
            }
            if (missingWords.Count > 0)
            {
                result = "";
                for (int i = 0; i < missingWords.Count; i++)
                {
                    if (i > 0)
                    {
                        result += ',';
                    }
                    result += missingWords[i];
                }
            }
            else
            {
                result = "true";
            }

            return result;
        }
        //Formats the string input into a more readable list.
        static List<string> InputWordSplitter(string matrixInput)
        {
            List<string> result = new List<string>();
            string word = "";
            string value = "";
            for (int i = 0; i < matrixInput.Length; i++)
            {
                value = matrixInput[i].ToString();
                //Is Letter?
                if (Regex.IsMatch(value, "[a-z]", RegexOptions.IgnoreCase))
                {
                    word += value;
                }
                //Comma is denotes end of a word and prepares for the next word.
                else if (value == ",")
                {
                    result.Add(word);
                    word = "";
                }
            }
            //Adds the final word contructed since the last word may not have a comma.
            if (!string.IsNullOrEmpty(word))
            {
                result.Add(word);
            }
            return result;
        }
        static void Main(string[] args)
        {
            //input[0] = matrix representation, input[1] = words to find in matrix.
            string[] input1 = new string[] { "rbfg, ukop, fgub, mnry", "bog,bop,gup,fur,ruk" };
            string[] input2 = new string[] { "aaey, rrum, tgmn, ball", "all,ball,mur,raeymnl,tall,true,trum" };
            string[] input3 = new string[] { "aaey, rrum, tgmn, ball", "all,ball,mur,raeymnl,rumk,tall,true,trum,yes" };
            Console.WriteLine(MatrixChallenge(input1));
            Console.WriteLine($"\n{MatrixChallenge(input2)}");
            Console.WriteLine($"\n{MatrixChallenge(input3)}");
        }
    }

    public class Grid
    {
        public Dictionary<string, Cell> Cells { get; }
        public Grid(string[] splitWords)
        {
            Cells = new Dictionary<string, Cell>();
            string key = "";
            //Construct the grid
            for (int i = 0; i < splitWords.Length; i++)
            {
                for (int j = 0; j < splitWords[i].Length; j++)
                {
                    //A cell's key is their letter + the xy co-ordinance.
                    key = splitWords[i][j].ToString() + j.ToString() + i.ToString();
                    Cells.Add(key, new Cell(j, i, splitWords[i][j].ToString()));
                }
            }
            //Go through grid and assign neighbours to the cells.
            foreach (Cell item in Cells.Values)
            {
                item.Neighbours = GetNeighbours(item, splitWords, Cells);
            }
        }
        public Cell[] FindLetterCell(string letter)
        {
            List<Cell> cellsWithLetter = new List<Cell>();
            foreach (string key in Cells.Keys)
            {
                if (key.Contains(letter))
                {
                    cellsWithLetter.Add(Cells[key]);
                }
            }
            return cellsWithLetter.ToArray();
        }
        //Root
        public bool FindWord(string word)
        {
            int index = 0;
            bool wordExists = false;
            //Find first letter of word by searching the dictionary for starting point.
            Cell[] currentLetter = FindLetterCell(word[index].ToString());
            if (currentLetter.Length > 0)
            {
                for (int i = 0; i < currentLetter.Length; i++)
                {
                    if (!wordExists)
                    {
                        currentLetter[i].Visited = true;
                        index++;
                        wordExists = FindNextLetter(currentLetter[i], word, index);

                        //Clean up
                        currentLetter[i].Visited = false;
                        index = 0;
                    }
                }
            }
            return wordExists;
        }
        //Child
        public bool FindNextLetter(Cell currentCell, string word, int index)
        {
            bool letterExists = false;
            string nextLetter = word[index].ToString();
            for (int i = 0; i < currentCell.Neighbours.Count; i++)
            {
                if (!letterExists)
                {
                    //Check if any of the neighbouring cells contain the next letter ignoring previously visited cells.
                    if (currentCell.Neighbours[i].Letter == nextLetter && !currentCell.Neighbours[i].Visited)
                    {
                        //Check if at the end of the word. No need to visit the neighbouring cells if the last letter is found.
                        int wordCheck = index + 1;
                        if (wordCheck < word.Length)
                        {
                            index++;
                            currentCell.Visited = true;
                            letterExists = FindNextLetter(currentCell.Neighbours[i], word, index);

                            //Clean up.
                            index--;
                            currentCell.Visited = false;
                        }
                        else
                        {
                            letterExists = true;
                        }
                    }
                }
            }

            return letterExists;
        }
        //Grabs all the neighbouring cells of a given cell and assigns the neighbour value to it.
        public List<Cell> GetNeighbours(Cell currentCell, string[] splitWords, Dictionary<string, Cell> currentGrid)
        {
            //8 way direction arrays.
            int[] xDirection = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] yDirection = { -1, 0, 1, -1, 1, -1, 0, 1 };

            List<Cell> neighbours = new List<Cell>();
            int x = currentCell.X;
            int y = currentCell.Y;
            string key = "";
            int xkey = -1;
            int ykey = -1;
            for (int i = 0; i < xDirection.Length; i++)
            {
                try
                {
                    xkey = x - xDirection[i];
                    ykey = y - yDirection[i];
                    key = splitWords[ykey][xkey].ToString() + xkey.ToString() + ykey.ToString();

                    if (currentGrid.ContainsKey(key))
                    {
                        neighbours.Add(currentGrid[key]);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
            return neighbours;
        }
    }
    public class Cell
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public string Letter { get; private set; }
        public bool Visited { get; set; }

        public List<Cell> Neighbours { get; set; }
        public Cell(int x, int y, string letter)
        {
            Neighbours = new List<Cell>();
            this.X = x;
            this.Y = y;
            this.Letter = letter;
        }
    }
}
