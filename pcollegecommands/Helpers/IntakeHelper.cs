using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Helpers
{
    public class IntakeHelper
    {
        private static readonly Dictionary<string, int> MonthToId = new Dictionary<string, int>
    {
        { "Jan", 1 },
        { "Feb", 2 },
        { "Mar", 3 },
        { "Apr", 4 },
        { "May", 5 },
        { "Jun", 6 },
        { "Jul", 7 },
        { "Aug", 8 },
        { "Sep", 9 },
        { "Oct", 10 },
        { "Nov", 11 },
        { "Dec", 12 }
    };

        public static List<List<int>> SplitIntakes(string intakeString)
        {
          //  var intakes = new List<List<int>>();
            var intakes = new List<List<int>>();
            var parts = intakeString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                var splitPart = trimmedPart.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (splitPart.Length == 2)
                {
                    var period = splitPart[0].Trim();
                    var status = splitPart[1].Trim();

                    // Extract the month part from the period (e.g., "Jan-2024")
                    var monthPart = period.Split('-')[0];

                    if (MonthToId.TryGetValue(monthPart, out int monthId))
                    {
                        intakes.Add(new List<int> { monthId });
                    }
                    else
                    {
                        intakes.Add(new List<int>()); // Add an empty list if month is not found
                    }
                }
                else
                {
                    // Handle case where there is no valid format
                    intakes.Add(new List<int>());
                }
            }

            return intakes;
        }
    }
}
