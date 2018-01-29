using DanielExample.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanielExample.Rest
{
    public interface IPokemonApi
    {
        [Get("/pokemon/{number}")]
        Task<Pokemon> GetPokemon(string number);

    }
}
