using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Game.Hunt.Persistence
{
    public class TxtDataAccess :IDataAccess
    {
        public async Task SaveAsync(string path, CT[,] data, int size, int clicks, int remaining,int x,int y)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path)) // fájl megnyitása
                {
                    await writer.WriteLineAsync(size.ToString()+" "+clicks.ToString()+" "+remaining.ToString()+" "+x.ToString()+" "+y.ToString()); // kiírjuk a méreteket
                    for (Int32 i = 0; i < size; i++)
                    {
                        for (Int32 j = 0; j < size; j++)
                        {
                            await writer.WriteAsync(data[i, j].ToString() + " "); // kiírjuk az értékeket
                        }
                        await writer.WriteLineAsync();
                    }
                }
            }
            catch
            {
                throw new HuntDataException();
            }
        }
        public async Task<Tuple<CT[,],int,int,int,int,int>> LoadAsync(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    String line = await reader.ReadLineAsync() ?? String.Empty;
                    String[] values = line.Split(' ');
                    int tableSize = Int32.Parse(values[0]);
                    int clicks = Int32.Parse(values[1]);
                    int remaining = Int32.Parse(values[2]);
                    int x = Int32.Parse(values[3]);
                    int y = Int32.Parse(values[4]);
                    CT[,] table = new CT[tableSize, tableSize];

                    for (int i = 0; i < tableSize; i++)
                    {
                        line = await reader.ReadLineAsync() ?? String.Empty;
                        values = line.Split(' ');
                        for (int j = 0; j < tableSize; j++)
                        {
                            Enum.TryParse<CT>(values[j], out table[i,j]);
                        }
                    }
                    return new Tuple<CT[,], int, int, int, int, int>(table, tableSize,clicks, remaining, x, y);
                }
            }
            catch
            {
                throw new HuntDataException();
            }
        }
    }
}
