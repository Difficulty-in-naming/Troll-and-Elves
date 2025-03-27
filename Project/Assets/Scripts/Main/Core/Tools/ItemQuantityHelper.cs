using System.Collections.Generic;
using System.Linq;
using EdgeStudio.DataStruct;

namespace EdgeStudio.Tools
{
    public static class ItemQuantityHelper
    {
        public static List<ItemQuantity> MergeDuplicates(this List<ItemQuantity> drops)
        {
            var mergedDrops = drops
                .GroupBy(item => item.Id)
                .Select(group => new ItemQuantity
                {
                    Id = group.Key,
                    Num = group.Sum(item => item.Num)
                })
                .ToList();

            return mergedDrops;
        }
    }
}