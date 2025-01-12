﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net.Http.Headers;
using WebApp.Admin.Data;
using WebApp.Areas.Admin.Data;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/thongtindocgia")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]

    public class ThongTinDocGiaController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7028/api/admin");
        private readonly HttpClient _client;

        public ThongTinDocGiaController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;

        }


        [Route("")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("QuanLyKho"))
            {
                return RedirectToAction("LoiPhanQuyen", "phanquyen");
            }
            else
            {
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/TheDocGia/GetAllTheDocGia");

                if (response.IsSuccessStatusCode)
                {
                    string dataJson = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JsonConvert.DeserializeObject<APIResponse<List<DTO_DocGia_TheDocGia>>>(dataJson);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        var data = apiResponse.Data;
                        ViewData["ThongTinDocGia"] = data;
                        return View();
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return View();
                }
            }
        }


        [HttpGet]
        [Route("GetAllThongTinDocGia")]
        public ActionResult GetAllThongTinDocGia()
        {
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/TheDocGia/GetAllTheDocGia").Result;

            if (response.IsSuccessStatusCode)
            {
                string dataJson = response.Content.ReadAsStringAsync().Result;
                var apiResponse = JsonConvert.DeserializeObject<APIResponse<List<DTO_DocGia_TheDocGia>>>(dataJson);

                if (apiResponse != null && apiResponse.Success)
                {
                    return Json(new { success = true, data = apiResponse.Data });
                }
                else
                {
                    return Json(new { success = false, Message = apiResponse.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = response.Content.ReadAsStringAsync() });
            }
        }


        [HttpPost]
        [Route("ThongTinTheDocGia")]
        public ActionResult ThongTinTheDocGia(int id)
        {
            try
            {
                DTO_DocGia_TheDocGia theDocGia = new DTO_DocGia_TheDocGia();

                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + $"/TheDocGia/ThongTinTheDocGia/{id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string dataJson = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JsonConvert.DeserializeObject<APIResponse<DTO_DocGia_TheDocGia>>(dataJson);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        // Trả về dữ liệu JSON nếu thành công
                        return Json(new { success = true, data = apiResponse.Data });
                    }
                    else
                    {
                        // Trả về thông báo lỗi nếu không tìm thấy sách
                        return Json(new { success = false, message = apiResponse.Message });
                    }
                }
                else
                {
                    return Json(new { success = false, message = response.Content.ReadAsStringAsync() });
                }


            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                Console.WriteLine($"Error in ChiTietDocGia: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        [HttpPost]
        [Route("CapNhatThongTin")]
        public ActionResult CapNhatThongTin(int maDocGia, DateOnly ngaySinh, string diaChi, string gioiTinh, string soDienThoai, string tenDocGia, string token)
        {
            try
            {
                DocGium dg = new DocGium();

                dg.Madg = maDocGia;
                dg.Hotendg = tenDocGia;
                dg.Ngaysinh = ngaySinh;
                dg.Diachi = diaChi;
                dg.Gioitinh = gioiTinh;
                dg.Sdt = soDienThoai;

                // đính kèm token khi gọi API
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // call API
                HttpResponseMessage response = _client.PostAsJsonAsync(_client.BaseAddress + "/ThongTinDocGia/UpdateThongTinDocGia", dg).Result;

                if (response.IsSuccessStatusCode)
                {
                    string dataJson = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JsonConvert.DeserializeObject<APIResponse<DocGium>>(dataJson);

                    if(apiResponse != null && apiResponse.Success)
                    {
                        return Json(new { success = true, data = dg });
                    }
                    else
                    {
                        return Json(new { success = false, message = apiResponse.Message });
                    }
                }
                else
                {
                    return Json(new { success = false, message = response.Content.ReadAsStringAsync() });
                }

            }
            catch (Exception ex)
            {
                // Kiểm tra xem lỗi có phải do tồn tại số điện thoại không
                if (ex.Message.Contains("Số điện thoại đã tồn tại."))
                    return Json(new { success = false, message = "Số điện thoại đã tồn tại." });

                // Xử lý lỗi nếu có
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }


    }
}
