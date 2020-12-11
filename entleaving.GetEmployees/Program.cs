using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using entleaving.freee;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace entleaving.GetEmployees {
  /// <summary></summary>
  public class Program {
    // TODO: COMPANY_ID を指定
    private static readonly int COMPANY_ID = 0;
    //private static readonly int COMPANY_ID = ;


    /// <summary></summary>
    public static async Task Main(string[] args) {
      var outputPath = $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}-{COMPANY_ID}-employees.csv";
      var w = new StreamWriter(path: outputPath, append: false, encoding: System.Text.Encoding.UTF8);

      var token = await ResponseToken.LoadAsync("access_token.json");
      var api = FreeeAPIFactory.GetInstance(token.AccessToken);
      var resp = await freee.Controllers.Employee.GetCompanyEmployeesAsync(COMPANY_ID);
      var jarr = JsonConvert.DeserializeObject(resp) as JArray;
      var dataset = jarr?.Select(jobj => jobj.Values().Select(jvalue => jvalue.Value<object?>()?.ToString() ?? string.Empty).ToArray())
        .ToArray();

      var propertyNames = jarr?.First?
        .Where(value => value is JProperty)
        .Cast<JProperty>()
        .Select(prop => prop.Name)
        .ToArray();

      if(propertyNames != null) {
        //Console.Error.WriteLine(string.Join(",", propertyNames));
        w.WriteLine(string.Join(",", propertyNames));
      }

      if(dataset != null) {
        foreach(var inline in dataset) {
          w.WriteLine(string.Join(',', inline));
        }
      }

      await w.FlushAsync();
      w.Dispose();
    }
  }
}
