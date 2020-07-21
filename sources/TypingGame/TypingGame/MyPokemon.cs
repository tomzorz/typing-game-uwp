using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace TypingGame
{
    public class MyPokemon
    {
        public string Name { get; set; }

        public int Number { get; set; }

        public string ImageUri => $"https://pokeres.bastionbot.org/images/pokemon/{Number}.png";
    }
}
