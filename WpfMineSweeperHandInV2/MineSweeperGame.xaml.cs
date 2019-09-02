using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfMineSweeperHandInV2
{

    enum GameState
    {
        NotStarted,
        Running,
        Ended
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MineSweeperGame : UserControl, INotifyPropertyChanged
    {
        private Grid gameGrid;
        private MineField[,] mineFields; //We need this to retroactivly palce mines so that the player does not hit a mine on the first try
        private int totalMines;
        private int totalFields;
        private GameState gameState = GameState.NotStarted;
        //TODO time passed
        private int minesFlagged;
        private int fieldsRevealed;

        public event PropertyChangedEventHandler PropertyChanged;

        public MineSweeperGame(int rows, int columns, int mines)
        {
            InitializeComponent();

            this.TotalMines = mines;
            this.totalFields = rows * columns;
            this.MinesFlagged = 0;
            this.FieldsRevealed = 0;
            this.mineFields = new MineField[rows, columns];

            BuildGrid(rows, columns);
            PopulateGrid();
            EstablishNeighbours();
            
        }

        public MineSweeperGame()
        {

        }

        private void BuildGrid(int rows, int columns)
        {
            //Init grid
            Grid newGrid = new Grid();
            newGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            newGrid.VerticalAlignment = VerticalAlignment.Stretch;
            newGrid.Margin = new Thickness(20); //TODO i would like the gameboard not strech weirdly, needs complicated binding. 


            //Define Rows
            for (int i = 0; i < rows; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                newGrid.RowDefinitions.Add(rowDefinition);
            }

            //Define columns
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                newGrid.ColumnDefinitions.Add(columnDefinition);
            }

            //Add to content
            gameGrid = newGrid; //Binding?
            this.Content = newGrid;
        }

        private void AddMineField(int row, int column, MineField mineField)
        {
            Grid.SetRow(mineField, row);
            Grid.SetColumn(mineField, column);
            gameGrid.Children.Add(mineField);
        }

        private void PopulateGrid()
        {
            //Gather needed values 
            int rows = gameGrid.RowDefinitions.Count();
            int columns = gameGrid.ColumnDefinitions.Count();
            int amountOFields = rows * columns;

            //Create the minefields
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    MineField tempMineField = new MineField(this);

                    //Register listeners
                    tempMineField.PropertyChanged += MineFieldPropertyChanged; //With that we dont need to keep a direct reference in the MineField to this object.
                    //Add to list
                    mineFields[r, c] = tempMineField;
                    //Add to grid
                    AddMineField(r, c, tempMineField);
                }
            }
        }

        private void EstablishNeighbours()
        {


            for (int row = 0; row < mineFields.GetLength(0); row++)
            {
                for (int column = 0; column < mineFields.GetLength(1); column++)
                {
                    //Get value
                    MineField tempMineField = mineFields[row, column];

                    //Setup N (Not in Top row)
                    if (row != 0)
                    {
                        tempMineField.Neigbours[CardinalDirection.North] = mineFields[row - 1, column];

                    }

                    //Setup NE
                    if (row != 0 && column != mineFields.GetUpperBound(1))
                    {
                        tempMineField.Neigbours[CardinalDirection.NorthEast] = mineFields[row - 1, column + 1];
                    }

                    //Setup E(Not at the rightmost column)
                    if (column != mineFields.GetUpperBound(1))
                    {
                        tempMineField.Neigbours[CardinalDirection.East] = mineFields[row, column + 1];
                    }

                    //Setup SE
                    if (row != mineFields.GetUpperBound(0) && column != mineFields.GetUpperBound(1))
                    {
                        tempMineField.Neigbours[CardinalDirection.SouthEast] = mineFields[row + 1, column + 1];
                    }

                    //Setup S (Not in bottom row)
                    if (row != mineFields.GetUpperBound(0))
                    {
                        tempMineField.Neigbours[CardinalDirection.South] = mineFields[row + 1, column];
                    }

                    //Setup SW
                    if (row != mineFields.GetUpperBound(0) && column != 0)
                    {
                        tempMineField.Neigbours[CardinalDirection.SouthWest] = mineFields[row + 1, column - 1];
                    }

                    //Setup W (Not at The left border)
                    if (column != 0)
                    {
                        tempMineField.Neigbours[CardinalDirection.West] = mineFields[row, column - 1];
                    }

                    //Setup NW
                    if (row != 0 && column != 0)
                    {
                        tempMineField.Neigbours[CardinalDirection.NorthWest] = mineFields[row - 1, column - 1];
                    }
                }
            }
        }

        private void MineFieldPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            //Here we can listen to what property was changed (by name.... i hate it) and react.
            //A Possible solution would be to implemente my own events... that seems liker an overkill of work :(

            if (e.PropertyName.Equals("SuspectedMine")) //TODO convert to Switch
            {
                if (sender is MineField)
                {
                    MineField tempMineField = (MineField)sender;
                    if (tempMineField.SuspectedMine)
                    {
                        MinesFlagged++;
                    }
                    else
                    {
                        MinesFlagged--;
                    }
                }
            }
            if (e.PropertyName.Equals("IsEnabled"))
            {
                if (sender is MineField)
                {
                    MineField tempMineField = (MineField)sender;
                    if (tempMineField.IsEnabled)
                    {
                        fieldsRevealed--;
                    }
                    else
                    {
                        fieldsRevealed++;
                        //here we react depending on the gamestate
                        if (gameState == GameState.NotStarted)
                        {
                            PlaceMines(tempMineField);
                            gameState = GameState.Running;
                        }
                        if (tempMineField.ContainsMine)
                        {
                            GameLost();
                        }
                        else
                        {
                            if (DetectWin())
                            {
                                GameWin();
                            }
                        }
                    }
                }
            }
        }


        private void PlaceMines(MineField mineField)
        {
            //Init needed values
            Random random = new Random();
            int placedMines = 0;

            while (placedMines < TotalMines)
            {
                int randomRow = random.Next(0, mineFields.GetLength(0));
                int randomColumn = random.Next(0, mineFields.GetLength(1));

                MineField pickedMinefield = mineFields[randomRow, randomColumn];

                if (!Object.ReferenceEquals(mineField, pickedMinefield))
                {
                    pickedMinefield.ContainsMine = true;
                    placedMines++;
                }
            }

            //Fore the minefield to update its number (Dirty hack, we should listen to the Neigbourslist, contains mine)
            mineField.ContainsMine = false;
        }

        private void GameLost()
        {
            gameGrid.IsEnabled = false; //Or something else
        }

        private bool DetectWin()
        {
            if (fieldsRevealed == (totalFields - TotalMines))
            {
                return true;
            }
            return false;
        }

        private void GameWin()
        {
            MessageBox.Show("You Win!", "Win!");
        }


        public int MinesFlagged
        {
            get { return minesFlagged; }
            set
            {
                minesFlagged = value;
                PropertyIsChanged();
            }
        }

        public int FieldsRevealed
        {
            get { return fieldsRevealed; }
            set
            {
                fieldsRevealed = value;
                PropertyIsChanged();
            }
        }

        public int TotalMines
        {
            get { return totalMines; }
            set
            {
                totalMines = value;
                PropertyIsChanged();
            }
        }

        private void PropertyIsChanged([CallerMemberName] String memberName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName)); //I would realy like to add state changed too here
        }

    }




}
