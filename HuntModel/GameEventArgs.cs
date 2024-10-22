using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Hunt.Model
{
    public class GameEventArgs
    {
        public int x;
        public int y;
        public string? text;

        public string? winner;

        public string? steps;

        public int size;
        public GameEventArgs(int x, int y, string? text)
        {
            this.x = x;
            this.y = y;
            this.text = text;
        }

        public GameEventArgs(string? text, string? winner)
        {
            this.text = text;
            this.winner = winner;
        }

        public GameEventArgs(string steps)
        {
            this.steps = steps;
        }

        public GameEventArgs(int size)
        {
            this.size = size;
        }
    }
}
