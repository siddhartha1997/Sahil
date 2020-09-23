using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AccountMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        Uri baseAddress = new Uri("https://localhost:44304/api");   //Port No.
        HttpClient client;

        public AccountController()
        {
            client = new HttpClient();
            client.BaseAddress = baseAddress;

        }
        int acid = 1;
        public static List<customeraccount> customeraccounts = new List<customeraccount>()
        {
            new customeraccount{custId=1,CAId=101,SAId=102}
        };
        public static List<CurrentAccount> currentAccounts = new List<CurrentAccount>()
        {
            new CurrentAccount{CAId=101,CBal=1000}
        };
        public static List<SavingsAccount> savingsAccounts = new List<SavingsAccount>()
        {
            new SavingsAccount{SAId=102,SBal=500}
        };
        // GET: api/<AccountController>
        [HttpGet]
        public IEnumerable<customeraccount> Get()
        {
            return customeraccounts;
        }

        // GET api/<AccountController>/5
        [HttpGet]
        [Route("getCustomerAccounts/{id}")]
        public string getCustomerAccounts(int id)
        {
            var a = customeraccounts.Find(c => c.custId == id);
            var ca = currentAccounts.Find(cac => cac.CAId == a.CAId);
            var sa = savingsAccounts.Find(sac => sac.SAId == a.SAId);
            return "Current Account(" + ca.CAId.ToString() + "):: Rs." + ca.CBal.ToString() + ".00\n" + "Savings Account(" + sa.SAId.ToString() + "):: Rs." + sa.SBal.ToString()+".00";
        }

        // POST api/<AccountController>
        [HttpPost]
        [Route("createAccount")]
        public customeraccount createAccount([FromBody] Customer customer)
        {
            customeraccount a = new customeraccount
            {
                custId = customer.id,
                CAId = (customer.id * 100) + acid,
                SAId = (customer.id * 100) + (acid + 1)
            };
            customeraccounts.Add(a);
            var cust = customeraccounts.Find(c => c.custId == customer.id);
            CurrentAccount ca = new CurrentAccount
            {
                CAId = (customer.id * 100) + acid,
                CBal = 0.00
            };
            SavingsAccount sa = new SavingsAccount
            {
                SAId = (customer.id * 100) + (acid+1),
                SBal = 0.00
            };
            return cust;
        }
        [HttpGet]
        [Route("getAccount/{id}")]
        public string getAccount(int id)
        {
            if(id%2!=0)
            {
                var ca = currentAccounts.Find(a => a.CAId == id);
                return "Current Account(" + ca.CAId + "):: Rs." + ca.CBal+".00";
            }
            var sa = savingsAccounts.Find(a => a.SAId == id);
            return "Savings Account(" + sa.SAId + "):: Rs." + sa.SBal+".00";
        }
        [HttpPost]
        [Route("deposit")]
        public string deposit([FromBody] dwacc value)
        {
            string data = JsonConvert.SerializeObject(value);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(client.BaseAddress + "/Transaction/deposit/", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data1 = response.Content.ReadAsStringAsync().Result;
                if (data1 == "Success")
                {
                    if(value.AccountId%2==0)
                    {
                        var sa = savingsAccounts.Find(a => a.SAId == value.AccountId);
                        sa.SBal = sa.SBal + value.Balance;
                        return "Deposited Successfully.New Account Rs." + sa.SBal + ".00";
                    }
                    var ca = currentAccounts.Find(a => a.CAId == value.AccountId);
                    ca.CBal = ca.CBal + value.Balance;
                    return "Deposited Successfully.New Account Rs."+ca.CBal+".00";
                }
                return "Deposition Failed";
            }
            return "Link Failure";
        }
        [HttpPost]
        [Route("withdraw")]
        public string withdraw([FromBody] dwacc value)
        {
            string data = JsonConvert.SerializeObject(value);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(client.BaseAddress + "/Transaction/withdraw/", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data1 = response.Content.ReadAsStringAsync().Result;
                if (data1 == "Transaction Success")
                {
                    if (value.AccountId % 2 == 0)
                    {
                        var sa = savingsAccounts.Find(a => a.SAId == value.AccountId);
                        sa.SBal = sa.SBal - value.Balance;
                        return "Withdrawn Successfully.New Balance Rs." + sa.SBal + ".00";
                    }
                    var ca = currentAccounts.Find(a => a.CAId == value.AccountId);
                    ca.CBal = ca.CBal - value.Balance;
                    return "Withdrawn Successfully.New Balance Rs."+ca.CBal+".00";
                }
                return "Withdrawn Failed";
            }
            return "Link Failure";
        }
        [HttpPost]
        [Route("transfer")]
        public string transfer([FromBody] transfers value)
        {
            double sb = 0.0, db = 0.0;
            string data = JsonConvert.SerializeObject(value);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(client.BaseAddress + "/Transaction/transfer/", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data1 = response.Content.ReadAsStringAsync().Result;
                if (data1 == "Transaction Success")
                {
                    if (value.source_accid % 2 == 0)
                    {
                        var sas = savingsAccounts.Find(a => a.SAId == value.source_accid);
                        sas.SBal = sas.SBal - value.amount;
                        sb = sas.SBal;
                    }
                    else
                    {
                        var cas = currentAccounts.Find(a => a.CAId == value.source_accid);
                        cas.CBal = cas.CBal - value.amount;
                        sb = cas.CBal;
                    }
                    if (value.destination_accid % 2 == 0)
                    {
                        var sa = savingsAccounts.Find(a => a.SAId == value.destination_accid);
                        sa.SBal = sa.SBal + value.amount;
                        db = sa.SBal;
                    }
                    else
                    {
                        var ca = currentAccounts.Find(a => a.CAId == value.destination_accid);
                        ca.CBal = ca.CBal + value.amount;
                        db = ca.CBal;
                    }
                    return "Sender Account Balance Rs." + sb + ".00\n" + "Receiver Account Balance Rs." + db + ".00";
                }
                return "Transfer Failure.Check minimum balance in account";
            }
            return "Link Failure";
        }
    }
}
