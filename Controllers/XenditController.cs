using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Models.Entity;

namespace LibraryManagementSystem.Controllers
{
    // Rute prefix untuk controller Web API Xendit
    [RoutePrefix("api/xendit")]
    public class XenditController : ApiController
    {
        // Endpoint POST untuk membuat invoice pembayaran denda di Xendit
        [HttpPost]
        [Route("create-invoice")]
        public async Task<IHttpActionResult> CreateInvoice([FromBody] JObject requestData)
        {
            // Mengambil Secret Key Xendit dari Web.config
            string secretKey = ConfigurationManager.AppSettings["XenditSecretKey"];

            // Nilai default jika request body kosong
            int amount = 25000;
            string description = "Pembayaran denda buku";
            string externalId = "ORDER-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Membaca data dari request body jika ada (mendukung pengujian dinamis di Postman)
            if (requestData != null)
            {
                if (requestData["amount"] != null)
                {
                    int.TryParse(requestData["amount"].ToString(), out amount);
                }
                if (requestData["description"] != null)
                {
                    description = requestData["description"].ToString();
                }
                if (requestData["external_id"] != null)
                {
                    externalId = requestData["external_id"].ToString();
                }
            }

            // Menyiapkan payload data untuk dikirim ke API Xendit
            var data = new
            {
                external_id = externalId,
                amount = amount,
                description = description,
                currency = "IDR"
            };

            using (var client = new HttpClient())
            {
                // Membuat token otentikasi Basic Auth (Secret Key di-encode ke Base64)
                string auth = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(secretKey + ":")
                );

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", auth);

                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Mengirimkan request POST ke endpoint pembuatan invoice Xendit
                var response = await client.PostAsync(
                    "https://api.xendit.co/v2/invoices",
                    content
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                // Jika request gagal, kembalikan status error beserta detailnya
                if (!response.IsSuccessStatusCode)
                    return BadRequest(responseBody);

                // Parsing response body JSON dari Xendit
                JObject result = JObject.Parse(responseBody);

                // Mengembalikan data invoice yang berhasil dibuat ke client
                return Ok(new
                {
                    message = "Invoice berhasil dibuat",
                    invoice_url = result["invoice_url"]?.ToString(),
                    status = result["status"]?.ToString(),
                    external_id = result["external_id"]?.ToString()
                });
            }
        }

        // Endpoint POST untuk menerima callback / webhook dari Xendit
        [HttpPost]
        [Route("webhook")]
        public async Task<IHttpActionResult> Webhook()
        {
            // Mengambil token webhook dari header 'x-callback-token'
            string tokenDariHeader = Request.Headers.Contains("x-callback-token")
                ? Request.Headers.GetValues("x-callback-token").FirstOrDefault()
                : null;

            // Mengambil token webhook yang terkonfigurasi di Web.config
            string tokenDariConfig = ConfigurationManager.AppSettings["XenditCallbackToken"];

            // Validasi token untuk memastikan request benar-benar dari Xendit
            if (tokenDariHeader != tokenDariConfig)
            {
                return Unauthorized();
            }

            // Membaca body request
            string body = await Request.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(body);

            string externalId = data["external_id"]?.ToString();
            string status = data["status"]?.ToString();

            // Jika status pembayaran adalah PAID atau SETTLED, lakukan update database
            if (status == "PAID" || status == "SETTLED")
            {
                if (!string.IsNullOrEmpty(externalId) && externalId.StartsWith("PAY-MB-", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(externalId.Substring(7), out int parsedNum))
                    {
                        int borrowingId = parsedNum - 100000;

                        using (var db = new LibraryDbContext())
                        {
                            var borrowing = db.Borrowings.Find(borrowingId);
                            if (borrowing != null && borrowing.FineStatus != "Paid")
                            {
                                // Memperbarui status pembayaran denda menjadi Paid
                                borrowing.FineStatus = "Paid";

                                // Menentukan ID pengguna log (gunakan ID anggota atau fallback 1)
                                int logUserId = borrowing.Member != null ? borrowing.Member.UserId : 1;

                                // Mencatat log aktivitas pembayaran denda via Webhook
                                var log = new ActivityLog
                                {
                                    UserId = logUserId,
                                    Action = "Pay Fine",
                                    Description = "Fine for borrowing ID " + borrowing.Id + " paid via Xendit Webhook (Callback - Order: " + externalId + ")",
                                    CreatedAt = DateTime.Now
                                };

                                db.ActivityLogs.Add(log);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }

            // Mengembalikan respon sukses 200 OK ke Xendit
            return Ok(new
            {
                message = "Webhook diterima",
                external_id = externalId,
                status = status
            });
        }
    }
}
