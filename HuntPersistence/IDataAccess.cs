using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Hunt.Persistence
{
    public interface IDataAccess
    {
        public Task SaveAsync(string path, CT[,] data, int size, int clicks, int remaining, int x, int y);

        public Task<Tuple<CT[,],int,int,int,int,int>> LoadAsync(string path);
    }
}
