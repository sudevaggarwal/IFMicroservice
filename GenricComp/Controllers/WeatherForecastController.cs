using CommonUtilitesForAll.Models;
using GenricCompBA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GenricComp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IGCBA _iGCBA;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IGCBA iGCBA)
        {
            _logger = logger;
            _iGCBA = iGCBA;
        }

        [HttpGet]
        public dynamic Get()
        {
            dynamic result = null;
            result = _iGCBA.GetDataSetDetail();
            return result;
        }

        [HttpPost]
        public async Task<dynamic> Post(Dept dept)
        {
            dynamic result = null;
            using(HttpClient client= new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44323");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                string jsonString = JsonConvert.SerializeObject(dept, Formatting.Indented);
                StringContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                // client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");
                HttpResponseMessage responcePost = await client.PostAsync("/weatherforecast", httpContent);
                string responcePostResult = await responcePost.Content.ReadAsStringAsync();

                HttpResponseMessage responceGet = await client.GetAsync("/weatherforecast");
                string responceGetResult = await responceGet.Content.ReadAsStringAsync();
            }
            return result;
        }
    }
}
