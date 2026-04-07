using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluence.Models
{
    public class Profile
    {
        public double InitialBalance { get; set; }
        public double MonthlyIncome { get; set; }
        public double MonthlyLimit { get; set; }
        public DateTime Payday { get; set; }
    }
}
