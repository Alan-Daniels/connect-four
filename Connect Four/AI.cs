using Connection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Connect_Four
{
    class AI
    {
        public event GameConnectionEventHandler<Point> LocationRecieved;
        private readonly Random random;

        private CoinType[][] CoinGrid;
        private Size GridSize;

        public AI(ref CoinType[][] CoinGrid, Size GridSize, int seed)
        {
            this.CoinGrid = CoinGrid;
            this.GridSize = GridSize;
            random = new Random(seed);
        }

        public void MakeMove()
        {
            new Task(SendMessage).Start();
        }

        private void SendMessage()
        {
            Thread.Sleep(350);
            int x, y, count = 100;
            do
            {
                count--;
                x = random.Next((int)GridSize.Width);
                y = GetHeight(x);
            } while (y == -1 && count > 0);
            if (y != -1)
            {
                Point point = new Point(x, y);
                LocationRecieved?.Invoke(this, new GameConnectionEventArgs<Point>(point, ""));
            }
            else
            {
                Console.WriteLine("failed to find a spot to move!");
            }
        }

        private int GetHeight(int column)
        {
            for (int i = (int)GridSize.Height - 1; i >= 0; i--)
            {
                if (CoinGrid[column][i] == CoinType.None)
                {
                    return i + 1;
                }
            }
            return -1;
        }
    }
}
