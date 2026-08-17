using System;
using System.Collections.Generic;
using System.Text;

namespace Recipes.Domain.Entities
{
    public sealed record RecipeIngredient(string name, string measure);

}
