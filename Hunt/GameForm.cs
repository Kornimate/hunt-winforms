using Game.Hunt.Model;
using Game.Hunt.Persistence;
using System.Windows.Forms;

namespace Game.Hunt.View
{
    public partial class GameForm : Form
    {
        private Button[,] buttons = null!;
        private HuntModel model;
        public GameForm()
        {
            InitializeComponent();

            model = new HuntModel();

            model.cellevent += new EventHandler<GameEventArgs>(DrawOnCell);
            model.ongameover += new EventHandler<GameEventArgs>(GameOver);
            model.showsteps += new EventHandler<GameEventArgs>(RemainingStepsChanged);
            model.adjusttablesize += new EventHandler<GameEventArgs>(SetTableSize);

            GenerateTable(5);
        }

        private void GenerateTable(int size)
        {
            RemoveTable();
            buttons = new Button[size, size];
            for(int i=0;i<size; i++)
            {
                for(int j = 0; j < size; j++)
                {
                    buttons[i,j] = new Button();
                    buttons[i, j].Height = 100;
                    buttons[i, j].Width = 100;
                    buttons[i, j].FlatStyle = FlatStyle.Popup;
                    buttons[i, j].Location = new Point(i * 100 ,j * 100+30);
                    buttons[i, j].TabIndex = i * 10 + j;
                    buttons[i, j].Font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold);
                    buttons[i,j].MouseClick += new MouseEventHandler(OnButtonClick);
                    Controls.Add(buttons[i, j]);
                }
            }
            model.StartNewGame(size);
        }
        private void RemoveTable()
        {
            for(int i = 0; i < model.TableSize; i++)
            {
                for(int j = 0; j < model.TableSize; j++)
                {
                    Controls.Remove(buttons[i, j]);
                }
            }
        }

        private void OnButtonClick(object? sender, MouseEventArgs e)
        {
            if(sender is Button btn)
            {
                model.ClickOnTableCell(btn.TabIndex);
            }
        }

        private void DrawOnCell(object? sender, GameEventArgs e)
        {
            buttons[e.x, e.y].Text = e.text;
        }

        private void GameOver(object? sender, GameEventArgs e)
        {
            MessageBox.Show($"{e.text}\nThe Winner: {e.winner}","Game Over");
            GenerateTable(model.TableSize);
        }

        private void RemainingStepsChanged(object? sender, GameEventArgs e)
        {
            RemainingSteps.Text = $"Remainings Steps: {e.steps}";
        }

        private void SetTableSize(object? sender, GameEventArgs e)
        {
            GenerateTable(e.size);
        }

        private void x3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateTable(3);
        }

        private void x5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateTable(5);
        }

        private void x7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateTable(7);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("You want to quit the game?","The Hunt Game",MessageBoxButtons.YesNo, MessageBoxIcon.Question)==DialogResult.Yes)
            {
                this.Close();
            }
        }

        private async void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await model.SaveAsync(saveFileDialog.FileName);
                }
                catch (HuntDataException)
                {
                    MessageBox.Show("Unable to load the game!" + Environment.NewLine + "Wrong path or Directory is not accessible!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void loadGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK) // ha kiválasztottunk egy fájlt
            {
                try
                {
                    await model.LoadAsync(openFileDialog.FileName);
                }
                catch (HuntDataException)
                {
                    MessageBox.Show("Unable to load the game!" + Environment.NewLine + "Wrong path or file format!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    GenerateTable(model.TableSize);
                }
            }
        }

        private void ℹToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The player on the turn clicks his/her in-game avatar and then\nchoose from the possible steps.\nThe Escaper moves first!", "Help");
        }
    }
}