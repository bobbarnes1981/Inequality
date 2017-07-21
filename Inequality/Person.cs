using System.Drawing;

namespace Inequality
{
    class Person
    {
        public Color Color { get; set; }

        public int Money { get; set; }

        public Person(Color color, int money)
        {
            Color = color;
            Money = money;
        }
    }
}
