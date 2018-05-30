using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public partial class EmployeeItem
{
    [JsonProperty("EmployeeKey")]
    public long EmployeeKey { get; set; }

    [JsonProperty("ParentEmployeeKey")]
    public long ParentEmployeeKey { get; set; }

    [JsonProperty("EmployeeNationalIDAlternateKey")]
    public string EmployeeNationalIdAlternateKey { get; set; }

    [JsonProperty("ParentEmployeeNationalIDAlternateKey")]
    public string ParentEmployeeNationalIdAlternateKey { get; set; }

    [JsonProperty("SalesTerritoryKey")]
    public long SalesTerritoryKey { get; set; }

    [JsonProperty("FirstName")]
    public string FirstName { get; set; }

    [JsonProperty("LastName")]
    public string LastName { get; set; }

    [JsonProperty("MiddleName")]
    public string MiddleName { get; set; }

    [JsonProperty("Title")]
    public string Title { get; set; }

    [JsonProperty("LoginID")]
    public string LoginId { get; set; }

    [JsonProperty("EmailAddress")]
    public string EmailAddress { get; set; }

    [JsonProperty("Phone")]
    public string Phone { get; set; }

    [JsonProperty("MaritalStatus")]
    public string MaritalStatus { get; set; }

    [JsonProperty("EmergencyContactName")]
    public string EmergencyContactName { get; set; }

    [JsonProperty("EmergencyContactPhone")]
    public string EmergencyContactPhone { get; set; }

    [JsonProperty("Gender")]
    public string Gender { get; set; }

    [JsonProperty("BaseRate")]
    public string BaseRate { get; set; }

    [JsonProperty("VacationHours")]
    public long VacationHours { get; set; }

    [JsonProperty("SickLeaveHours")]
    public long SickLeaveHours { get; set; }

    [JsonProperty("DepartmentName")]
    public string DepartmentName { get; set; }

    [JsonProperty("Status")]
    public string Status { get; set; }

    public string FullName
    {
        get
        {
            return FirstName + " " + LastName;
        }
    }
}


