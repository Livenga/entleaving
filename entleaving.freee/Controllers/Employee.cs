using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using entleaving.freee;


namespace entleaving.freee.Controllers {
  /// <summary></summary>
  public static class Employee {
    public static async Task<string> CreateAsync(
        int      companyId,
        string   lastName,
        string   firstName,
        string   lastNameKana,
        string   firstNameKana,
        DateTime birthDate,
        DateTime entryDate,
        string   payCalcType,
        int      payAmount,
        string?  num                          = null,
        string?  workingHoursSystemName       = null,
        string?  companyReferenceDateRuleName = null,
        int?     gender                       = null,
        bool?    marriedF                     = null) {
      var o = new {
        company_id = companyId,
        employee   = new {
          num = num,
          working_hours_system_name = workingHoursSystemName,
          company_reference_date_rule_name = companyReferenceDateRuleName,
          last_name = lastName,
          first_name = firstName,
          last_name_kana = lastNameKana,
          first_name_kana = firstNameKana,
          birth_date = birthDate.ToString("yyyy-MM-dd"),
          entry_date = entryDate.ToString("yyyy-MM-dd"),
          pay_calc_type = payCalcType,
          pay_amount = payAmount,
          gender = gender,
          married_f = marriedF
        }
      };


      var api  = FreeeAPIFactory.GetInstance();
      var json = JsonConvert.SerializeObject(
          o,
          new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

      using(var content = new StringContent(
            content:   json,
            encoding:  Encoding.UTF8,
            mediaType: "application/json"))
      using(var req = api.PostRequest(
              url:       "https://api.freee.co.jp/hr/api/v1/employees",
              content:   content,
              useBearer: true)) {
        var resp = await HttpFactory.GetInstance().SendAsync(req);
        return await api.GetContentAsync(resp);
      }
    }

    /// <summary></summary>
    public static async Task<string> GetCompanyEmployeesAsync(
        int companyId,
        int per  = 0,
        int page = 25) {
      var api = FreeeAPIFactory.GetInstance();

      using(var req = api.GetRequest($"https://api.freee.co.jp/hr/api/v1/companies/{companyId}/employees")) 
      using(var resp = await HttpFactory.GetInstance().SendAsync(req)) {
        return await api.GetContentAsync(resp);
      }
    }
  }
}
