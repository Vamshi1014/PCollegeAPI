using System.Data;

namespace Flyurdreamcommands.Models.Datafields
{
    public class DataTables
    {
        public DataTable DocumentsDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("document_id", typeof(int));
            table.Columns.Add("document_name", typeof(string));
            table.Columns.Add("file_path", typeof(string)); // Change to string to store URL
            table.Columns.Add("uploaded_by", typeof(string));
            table.Columns.Add("uploaded_at", typeof(DateTime));
            table.Columns.Add("document_type", typeof(int));
            table.Columns.Add("container_name", typeof(string));
            return table;
        }

        public DataTable TargetCountriesType()
        {
            DataTable table = new DataTable();
            table.Columns.Add("CountryId", typeof(int));
            table.Columns.Add("CountryName", typeof(string));
            table.Columns.Add("CreatedBy", typeof(int)); 
            table.Columns.Add("UploadedBy", typeof(int));
            table.Columns.Add("CompanyId", typeof(int));
            table.Columns.Add("BranchId", typeof(int));
            return table;
        }

        public DataTable EstimateNumberOfStudentsPerIntakeType()
        {
            DataTable table = new DataTable();
            table.Columns.Add("IntakeId", typeof(int));
            table.Columns.Add("EstimateOfStudents", typeof(int));
            table.Columns.Add("CreatedBy", typeof(int)); // Change to string to store URL
            table.Columns.Add("UploadedBy", typeof(int));
            table.Columns.Add("CompanyId", typeof(int));
            table.Columns.Add("BranchId", typeof(int));
            
            return table;
        }
        public DataTable EnglishExamScoreDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ExamRequirementId", typeof(int));
            table.Columns.Add("ExamTypeId", typeof(int));
            table.Columns.Add("StudentId", typeof(int));
            table.Columns.Add("ListeningScore", typeof(string));
            table.Columns.Add("SpeakingScore", typeof(string));
            table.Columns.Add("ReadingScore", typeof(string));
            table.Columns.Add("WritingScore", typeof(string));
            table.Columns.Add("VerbalReasoningScore", typeof(string));
            table.Columns.Add("QuantitativeReasoningScore", typeof(string));
            table.Columns.Add("AnalyticalWritingScore", typeof(string));
            table.Columns.Add("MinimumScore", typeof(string));
            table.Columns.Add("OverallScore", typeof(string));
            table.Columns.Add("LiteracyScore", typeof(string));
            table.Columns.Add("ConversationScore", typeof(string));
            table.Columns.Add("ComprehensionScore", typeof(string));
            table.Columns.Add("ProductionScore", typeof(string));
            table.Columns.Add("Createdon", typeof(DateTime));
            return table;
        }

        public DataTable StudentDocumentDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("StudentDocumentId", typeof(int));
            table.Columns.Add("DocumentId", typeof(int));
            table.Columns.Add("StudentId", typeof(int));
            return table;
        }

        public DataTable EmergencyContactDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("EmergencyId", typeof(int));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("Telephone", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("StudentId", typeof(int));

            return table;
        }
        public DataTable VisaRefusalDataTable()
        {
            DataTable visaRefusalTable = new DataTable();

            // Define the columns of the DataTable
            visaRefusalTable.Columns.Add("VisarefusalId", typeof(int)); // Assuming this is an identity column
            visaRefusalTable.Columns.Add("RefusalCountry", typeof(string));
            visaRefusalTable.Columns.Add("RefusalDate", typeof(DateTime));
            visaRefusalTable.Columns.Add("Description", typeof(string));
            visaRefusalTable.Columns.Add("StudentId", typeof(int));

            return visaRefusalTable;
        }


        public DataTable ResponsesDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ResponsesId", typeof(int));
            table.Columns.Add("QuestionId", typeof(int));
            table.Columns.Add("CompanyId", typeof(int));
            table.Columns.Add("Response", typeof(string));
            table.Columns.Add("DocumentId", typeof(string));
            return table;
        }
        public DataTable ReferencesDataTable()
        {
            DataTable referencesTable = new DataTable();

            // Define the columns of the DataTable
            referencesTable.Columns.Add("ReferenceID", typeof(int));
            referencesTable.Columns.Add("FirstName", typeof(string));
            referencesTable.Columns.Add("LastName", typeof(string));
            referencesTable.Columns.Add("Organisation", typeof(string));
            referencesTable.Columns.Add("Telephone", typeof(string));
            referencesTable.Columns.Add("Email", typeof(string));


            return referencesTable;
        }
        public DataTable CompanyAddressesTable()
        {
            DataTable companyAddressesTable = new DataTable();
            companyAddressesTable.Columns.Add("CompanyAddressId", typeof(int));
            companyAddressesTable.Columns.Add("CompanyId", typeof(int));
            companyAddressesTable.Columns.Add("AddressId", typeof(int));

            return companyAddressesTable;
        }
        public DataTable CompanyReferenceTable()
        {
            DataTable companyAddressesTable = new DataTable();
            companyAddressesTable.Columns.Add("CompanyReferenceId", typeof(int));
            companyAddressesTable.Columns.Add("CompanyId", typeof(int));
            companyAddressesTable.Columns.Add("ReferenceID", typeof(int));

            return companyAddressesTable;
        }
        public DataTable EducationDataTable()
        {
            // Create a new DataTable
            DataTable educationTable = new DataTable();

            // Define the columns of the DataTable            
            educationTable.Columns.Add("education_details_Id", typeof(int));
            educationTable.Columns.Add("education_level", typeof(string));
            educationTable.Columns.Add("school_name", typeof(string));
            educationTable.Columns.Add("percentage", typeof(decimal));
            educationTable.Columns.Add("grade", typeof(string));
            educationTable.Columns.Add("gpa", typeof(decimal));
            educationTable.Columns.Add("english_marks", typeof(int));
            educationTable.Columns.Add("maths_marks", typeof(int));
            educationTable.Columns.Add("physics_marks", typeof(int));
            educationTable.Columns.Add("chemistry_marks", typeof(int));
            educationTable.Columns.Add("Student_id", typeof(int));

            // Optionally, set additional properties for columns if needed
            // For example, you can set AllowDBNull to true if some columns can be null

            return educationTable;
        }


        public DataTable WorkExperienceDataTable()
        {
            // Create a new DataTable
            DataTable workExperienceTable = new DataTable();

            // Define the columns of the DataTable
            workExperienceTable.Columns.Add("WorkExperienceId", typeof(int));
            workExperienceTable.Columns.Add("StartDate", typeof(DateTime));
            workExperienceTable.Columns.Add("EndDate", typeof(DateTime));
            workExperienceTable.Columns.Add("Designation", typeof(string));
            workExperienceTable.Columns.Add("Responsibilities", typeof(string));
            workExperienceTable.Columns.Add("ProjectName", typeof(string));
            workExperienceTable.Columns.Add("CreatedOn", typeof(DateTime));
            workExperienceTable.Columns.Add("UpdatedOn", typeof(DateTime));
            workExperienceTable.Columns.Add("CreatedBy", typeof(int));
            workExperienceTable.Columns.Add("StudentId", typeof(int));
            workExperienceTable.Columns.Add("UpdatedBy", typeof(int));
            workExperienceTable.Columns.Add("CompanyName", typeof(string));
            workExperienceTable.Columns.Add("Till_Now", typeof(bool));

            // Optionally, set additional properties for columns if needed
            // For example, you can set AllowDBNull to true if
            return workExperienceTable;
        }

        public DataTable UniversityEntryRequirementsDataTable()
        {
            // UniversityEntryRequirementsDataTable UERDT;
            DataTable uERDT = new DataTable();
            uERDT.Columns.Add("UniversityEntryRequirementsId", typeof(int));
            uERDT.Columns.Add("Typeid", typeof(int));
            uERDT.Columns.Add("Universityid", typeof(int));
            uERDT.Columns.Add("EntryCountryid", typeof(int));
            uERDT.Columns.Add("UniversityCountryid", typeof(int));
            uERDT.Columns.Add("HSCId", typeof(int));
            uERDT.Columns.Add("Percentage", typeof(decimal));
            uERDT.Columns.Add("MOIId", typeof(int));
            uERDT.Columns.Add("Education_Gap", typeof(int));
            uERDT.Columns.Add("Created_By", typeof(int));
            uERDT.Columns.Add("Created_On", typeof(DateTime));
            uERDT.Columns.Add("Updated_By", typeof(int));
            uERDT.Columns.Add("Updated_On", typeof(DateTime));
            uERDT.Columns.Add("IsActive", typeof(bool));
            return uERDT;
        }
        public DataTable examRequirementsTable()
        {
            DataTable examRequirementsTable = new DataTable();
            examRequirementsTable.Columns.Add("ExamTypeId", typeof(int));
            examRequirementsTable.Columns.Add("ListeningScore", typeof(string));
            examRequirementsTable.Columns.Add("SpeakingScore", typeof(string));
            examRequirementsTable.Columns.Add("ReadingScore", typeof(string));
            examRequirementsTable.Columns.Add("WritingScore", typeof(string));
            examRequirementsTable.Columns.Add("VerbalReasoningScore", typeof(string));
            examRequirementsTable.Columns.Add("QuantitativeReasoningScore", typeof(string));
            examRequirementsTable.Columns.Add("AnalyticalWritingScore", typeof(string));
            examRequirementsTable.Columns.Add("MinimumScore", typeof(string));
            examRequirementsTable.Columns.Add("OverallScore", typeof(string));
            examRequirementsTable.Columns.Add("LiteracyScore", typeof(string));
            examRequirementsTable.Columns.Add("ConversationScore", typeof(string));
            examRequirementsTable.Columns.Add("ComprehensionScore", typeof(string));
            examRequirementsTable.Columns.Add("ProductionScore", typeof(string));
            examRequirementsTable.Columns.Add("Createdon", typeof(DateTime));
            examRequirementsTable.Columns.Add("CreatedBy", typeof(int));
            examRequirementsTable.Columns.Add("UpdatedBy", typeof(int));
            examRequirementsTable.Columns.Add("UpdatedOn", typeof(DateTime));
            return examRequirementsTable;
        }

        public DataTable UniversityExamRequirementsTable()
        {
            DataTable examRequirementsTable = new DataTable();
            examRequirementsTable.Columns.Add("UniversityExamRequirementId", typeof(int));
            examRequirementsTable.Columns.Add("UniversityId", typeof(int));
            examRequirementsTable.Columns.Add("AcademicCategoryId", typeof(int));
            examRequirementsTable.Columns.Add("ExamRequirementId", typeof(int));
            examRequirementsTable.Columns.Add("IsActive", typeof(bool));
            examRequirementsTable.Columns.Add("UniversityCountryId", typeof(int));
            return examRequirementsTable;
        }



    }
}

