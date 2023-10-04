namespace APITEST.Models
{
    public class Employee
    {
        public string email { get; set; }
        public string personID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public List<string> roles { get; set; }
        public object credlyEmployeeState { get; set; }
        public bool isAdmin { get; set; }
        public bool isBusinnesRep { get; set; }
        public bool isPractitioner { get; set; }
    }
}
