using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class ProgramMaster
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int ProgramDetailsId { get; set; }
        public string Duration { get; set; }
        public string Intake1 { get; set; }
        public string Intake2 { get; set; }
        public string Intake3 { get; set; }
        public string Intake4 { get; set; }
        public string Intake5 { get; set; }
        public string Intake6 { get; set; }
        public string Intake7 { get; set; }

        public decimal CostOfLiving { get; set; }
        public decimal ApplicationFee { get; set; }
        public decimal TutionFee { get; set; }
        public decimal MinimumDeposit { get; set; }
        public string ProcessingTime { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class Intake
    {
        public int IntakeId { get; set; }
        public string Period { get; set; }
        public string Status { get; set; }
    }

}
