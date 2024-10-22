using Game.Hunt.Persistence;
using System.Diagnostics;
using System.Net.WebSockets;

namespace Game.Hunt.Model
{
    public class HuntModel
    {

        private const string ninja = "🐱‍👤"; // 1
        private const string cop = "👨‍✈️"; // 2
        private const string blackCircle = "🔵"; // 3
        private const string blueCircle = "⚫"; // 4

        private CT[,]? Table;
        public int TableSize { get; set; }
        private int ModelTableSize;

        private int clicks;
        private int remaining;

        private int prevX;
        private int prevY;

        public event EventHandler<GameEventArgs>? cellevent;
        public event EventHandler<GameEventArgs>? ongameover;
        public event EventHandler<GameEventArgs>? showsteps;
        public event EventHandler<GameEventArgs>? adjusttablesize;

        private IDataAccess DataAccess = null!;

        public HuntModel()
        {
            TableSize = 0;
            DataAccess = new TxtDataAccess();
        }

        private void SetCellValue(int x, int y, CT value)
        {
            Table![x, y] = value;
            if (x == 0 || y == 0 || x == ModelTableSize - 1 || y == ModelTableSize - 1) return;
            if (value == CT.Ninja)
            {
                cellevent?.Invoke(this, new GameEventArgs(x-1, y-1,ninja));
            } 
            else if (value == CT.Cop)
            {
                cellevent?.Invoke(this, new GameEventArgs(x-1, y-1,cop));
            }
            else if (value == CT.PossibleStepNinja)
            {
                cellevent?.Invoke(this, new GameEventArgs(x - 1, y - 1, blackCircle));
            }
            else if (value == CT.PossibleStepCop)
            {
                cellevent?.Invoke(this, new GameEventArgs(x - 1, y - 1, blueCircle));
            }
            else if (value == CT.Empty)
            {
                cellevent?.Invoke(this, new GameEventArgs(x-1, y-1, ""));
            }
        }
        public void StartNewGame(int size)
        {
            TableSize = size;
            ModelTableSize = TableSize + 2;
            prevX = prevY = -1;
            clicks = 0;
            remaining = GetRemainingsSteps();
            Table = new CT[ModelTableSize, ModelTableSize];
            for(int i = 0; i < ModelTableSize; i++)
            {
                for(int j = 0; j < ModelTableSize; j++)
                {
                    Table[i, j] = 0;
                }
            }
            SetCellValue(1, 1, CT.Cop);
            SetCellValue(1, TableSize, CT.Cop);
            SetCellValue(TableSize,1, CT.Cop);
            SetCellValue(TableSize, TableSize, CT.Cop);
            SetCellValue(TableSize/2+1,TableSize/2+1, CT.Ninja);
            showsteps?.Invoke(this, new GameEventArgs(remaining.ToString()));
        }

        public void ClickOnTableCell(int index)
        {
            int x = index / 10 + 1;
            int y = index % 10 + 1;
            if(prevX != -1)
            {
                if (Table![x, y] == CT.PossibleStepNinja && clicks%2==0)
                {
                    MovePlayer(x, y,true);
                    clicks++;
                }
                else if(Table![x, y] == CT.PossibleStepCop && clicks % 2 == 1)
                {
                    MovePlayer(x, y, false);
                    clicks++;
                    remaining--;
                    showsteps?.Invoke(this, new GameEventArgs(remaining.ToString()));
                }
                else
                {
                    RemovePossibleSteps();
                }
            }
            if (clicks % 2 == 0 && Table![x, y] == CT.Ninja)
            {
                prevX = x;
                prevY = y;
                ShowPossibleSteps(prevX, prevY, true);
            }
            if (clicks % 2 == 1 && Table![x, y] == CT.Cop)
            {
                prevX = x;
                prevY = y;
                ShowPossibleSteps(prevX, prevY, false);
            }
            CheckIfEscaperLost();
            if (remaining == 0) ongameover?.Invoke(this, new GameEventArgs("The Chasers could not catch the Escaper!", "The Escaper"));
        }
        private void ShowPossibleSteps(int x,int y,bool escaperturn)
        {
            if (escaperturn)
            {
                if (Table![prevX + 1, prevY] == CT.Empty)
                {
                    SetCellValue(prevX + 1, prevY, CT.PossibleStepNinja);
                }
                if (Table[prevX, prevY + 1] == CT.Empty)
                {
                    SetCellValue(prevX, prevY + 1, CT.PossibleStepNinja);
                }
                if (Table[prevX - 1, prevY] == CT.Empty)
                {
                    SetCellValue(prevX - 1, prevY, CT.PossibleStepNinja);
                }
                if (Table[prevX, prevY - 1] == CT.Empty)
                {
                    SetCellValue(prevX, prevY - 1, CT.PossibleStepNinja);
                }
            } else
            {
                if (Table![prevX + 1, prevY] == CT.Empty) SetCellValue(prevX + 1, prevY, CT.PossibleStepCop);
                if (Table[prevX, prevY + 1] == CT.Empty) SetCellValue(prevX, prevY + 1, CT.PossibleStepCop);
                if (Table[prevX - 1, prevY] == CT.Empty) SetCellValue(prevX - 1, prevY, CT.PossibleStepCop);
                if (Table[prevX, prevY - 1] == CT.Empty) SetCellValue(prevX, prevY - 1, CT.PossibleStepCop);
            }
        }

        private void CheckIfEscaperLost()
        {
            int counter = 0;
            int x=-1, y=-1;
            for (int i = 0; i < ModelTableSize; i++)
            {
                for (int j = 0; j < ModelTableSize; j++)
                {
                    if (Table![i, j] == CT.Ninja)
                    {
                        x = i;
                        y = j;
                        break;
                    }
                }
                if (x != -1) break;
            }
            if (GoodCoordinates(x + 1, y)) counter++;
            if (GoodCoordinates(x , y + 1)) counter++;
            if (GoodCoordinates(x - 1, y)) counter++;
            if (GoodCoordinates(x, y - 1)) counter++;
            if (counter == 0) ongameover?.Invoke(this, new GameEventArgs("The Escaper Lost the Game!", "The Chasers"));
        }

        private bool GoodCoordinates(int x, int y)
        {
            return x!=0 && y!=0 && x!=ModelTableSize-1 && y!=ModelTableSize-1 && Table![x,y]!=CT.Cop;
        }
        private void RemovePossibleSteps()
        {
            if (prevX == -1) return;
            if(Table![prevX+1,prevY]==CT.PossibleStepCop || Table![prevX+1,prevY]==CT.PossibleStepNinja) SetCellValue(prevX + 1, prevY, CT.Empty);
            if(Table![prevX,prevY+1]==CT.PossibleStepCop || Table![prevX,prevY+1]==CT.PossibleStepNinja) SetCellValue(prevX, prevY + 1, CT.Empty);
            if(Table![prevX-1,prevY]==CT.PossibleStepCop || Table![prevX-1,prevY]==CT.PossibleStepNinja) SetCellValue(prevX - 1, prevY, CT.Empty);
            if(Table![prevX,prevY-1]==CT.PossibleStepCop || Table![prevX,prevY-1]==CT.PossibleStepNinja) SetCellValue(prevX, prevY - 1, CT.Empty);
            prevX = -1;
            prevY = -1;
        }

        private void MovePlayer(int x, int y,bool escaperturn)
        {
            SetCellValue(prevX, prevY, CT.Empty);
            RemovePossibleSteps();
            if (escaperturn) SetCellValue(x, y, CT.Ninja);
            else SetCellValue(x, y, CT.Cop);

        }

        private int GetRemainingsSteps()
        {
            return TableSize * 4;
        }

        public async Task SaveAsync(string path)
        {
            await DataAccess.SaveAsync(path, Table!,ModelTableSize,clicks,remaining,prevX,prevY);
        }
        public async Task LoadAsync(string path)
        {
            var data = await DataAccess.LoadAsync(path);
            adjusttablesize?.Invoke(this, new GameEventArgs(data.Item2-2));
            Table = data.Item1;
            ModelTableSize = data.Item2;
            TableSize = ModelTableSize - 2;
            clicks = data.Item3;
            remaining = data.Item4;
            prevX = data.Item5;
            prevY = data.Item6;
            for(int i = 0; i < ModelTableSize; i++)
            {
                for(int j = 0; j < ModelTableSize; j++)
                {
                    SetCellValue(i, j, Table[i, j]);
                }
            } 
        }
    }
}