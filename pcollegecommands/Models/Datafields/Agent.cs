using Microsoft.Extensions.Logging;
using pcollegecommands.Models.Datafields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Spire.Pdf.General.Render.Font.OpenTypeFile.Table_GSUB.LigatureSubst.LigatureSet;

namespace Flyurdreamcommands.Models.Datafields
{
    public class Agent
    {

        public CompanyDetails? Company { get; set; }
        public Agent_Information? AgentInformation { get; set; }
        public List<TargetCountries>? listTargetCountries { get; set; }
        public List<EstimateStudentsperintake>? EstimateStudentsperintake { get; set; }
        public List<CompanyDocuments>? listCompanyDocuments { get; set; }
        public IsUpdate? IsUpdate { get;set;}

    }
 
     public class Agent_Information
    {
        public int AgentID { get; set; }
        public string? AgentName { get; set; }
        public string? CertifyingPersonName { get; set; }
        public string? CertifyingPersonRole { get; set; }
        public string? Signature { get; set; }
        public DateTime Date { get; set; }
        public string? ICEFAccreditation { get; set; }
        public int ICEFDocument { get; set; }
        public string? LegalStatus { get; set; }
        public int LegalStatusDocument { get; set; }
        public string? ServiceCharges { get; set; }
        public int Status { get; set; }
        public int IsActive { get; set; }

        public int CompanyId { get; set; }
        public int BranchId { get; set; }

        public string? Agent_Unique_Id { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
        public User? UploadedBy { get; set; }

        public DateTime UploadedOn { get; set; }
    }

}
