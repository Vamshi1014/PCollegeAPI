using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class CompanyQuestionsMapping
    {
        public int CompanyFormQuestionsId { get; set; }
        public int FormQuestionId { get; set; }
        public int CompanyId { get; set; }
        public bool FieldEnable { get; set; }

    }
}
