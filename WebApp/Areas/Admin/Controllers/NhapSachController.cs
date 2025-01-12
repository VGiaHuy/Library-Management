﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System;
using System.Text;
using System.Threading;
using WebApp.Areas.Admin.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Headers;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using WebApp.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/nhapsach")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]

    public class NhapSachController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        Uri baseAddress = new Uri("https://localhost:7028/api/admin");
        private readonly HttpClient _client;

        public NhapSachController(IWebHostEnvironment webHostEnvironment)
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("")]
        public IActionResult Index()
        {
            if (User.IsInRole("ThuThu"))
            {
                return RedirectToAction("LoiPhanQuyen", "phanquyen");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Route("GetListNCCPaging_APP")]
        public async Task<IActionResult> GetListNCCPaging_APP([FromBody] GetListPhieuMuonPaging req)
        {
            try
            {
                List<NhaCungCap> nccList = new List<NhaCungCap>();

                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/NhapSach/GetListNCCPaging_API", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<NhaCungCap>>(data);
                    //return Ok(responseObject);
                    return Ok(new { success = true, nccList = responseObject });
                }
                else
                {
                    // Handle unsuccessful status code
                    return BadRequest(new { success = false, message = "Failed to retrieve data from API." });
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetListSachPaging_APP")]
        public async Task<IActionResult> GetListSachPaging_APP([FromBody] GetListPhieuMuonPaging req)
        {
            try
            {
                List<Sach> sachList = new List<Sach>();

                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Nhapsach/GetListSachPaging_API", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<Sach>>(data);
                    //return Ok(responseObject);
                    return Ok(new { success = true, sachList = responseObject });
                }
                else
                {
                    // Handle unsuccessful status code
                    return BadRequest(new { success = false, message = "Failed to retrieve data from API." });
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("ThemNCC_APP")]
        public async Task<IActionResult> ThemNCC_APP([FromBody] NhaCungCap dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Tenncc) || string.IsNullOrEmpty(dto.Sdtncc) || string.IsNullOrEmpty(dto.Diachincc))
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                var jsonContent = JsonConvert.SerializeObject(dto);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/NhapSach/ThemNCC_API", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<InsertRes>(result);
                    return Ok(res);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Thêm NCC thất bại." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"API call failed: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("PhieuNhap_APP")]
        public async Task<IActionResult> PhieuNhap_APP([FromForm] DTO_Tao_Phieu_Nhap dto)
        {
            try
            {
                var lstSachNhap = new List<DTO_Sach_Nhap_Json>();

                var path = Path.Combine(_webHostEnvironment.WebRootPath, "img_web");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach (var item in dto.listSachNhap)
                {
                    string relativeImagePath = "empty";
                    // Lưu ảnh và tạo đường dẫn tương đối
                    if (item.fileImage != null && item.fileImage.Length > 0)
                    {
                        var fileName = Path.GetFileName(item.fileImage.FileName); // Lấy tên file
                        var localImagePath = Path.Combine(path, fileName); // Tạo đường dẫn cục bộ đầy đủ

                        // Lưu ảnh vào thư mục wwwroot/img_web
                        using (var stream = new FileStream(localImagePath, FileMode.Create))
                        {
                            await item.fileImage.CopyToAsync(stream);
                        }

                        // Tạo đường dẫn tương đối để lưu vào SQL
                        relativeImagePath = $"~/img_web/{fileName}";
                    }

                    // Thêm đường dẫn tương đối vào đối tượng DTO_Sach_Nhap_Json
                    lstSachNhap.Add(new DTO_Sach_Nhap_Json()
                    {
                        giaSach = item.giaSach,
                        maSach = item.maSach,
                        moTa = item.moTa,
                        namXB = item.namXB,
                        ngonNgu = item.ngonNgu,
                        nhaXB = item.nhaXB,
                        soLuong = item.soLuong,
                        tacGia = item.tacGia,
                        tenSach = item.tenSach,
                        theLoai = item.theLoai,
                        fileImage = relativeImagePath // Thêm đường dẫn tương đối vào đối tượng DTO_Sach_Nhap_Json
                    });
                }

                var convertData = new DTO_Tao_Phieu_Nhap_Json()
                {
                    MaNhaCungCap = dto.MaNhaCungCap,
                    TenNhaCungCap = dto.TenNhaCungCap,
                    MaNhanVien = dto.MaNhanVien,
                    NgayNhap = dto.NgayNhap,
                    listSachNhap = lstSachNhap
                };

                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new StringContent(JsonConvert.SerializeObject(convertData), Encoding.UTF8, "application/json"), "data");

                var response = await _client.PostAsync($"{_client.BaseAddress}/NhapSach/PhieuNhap_API", multipartContent);
                if (response.IsSuccessStatusCode)
                {
                    // Lấy file PDF từ response content
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = $"PhieuNhap_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                    // Trả về file PDF cho client
                    return File(pdfBytes, "application/pdf", fileName);
                }
                else
                {
                    // Xử lý lỗi khi API trả về trạng thái không thành công
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, message = $"Tạo phiếu nhập thất bại: {errorMessage}" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"API call failed: {ex.Message}" });
            }
        }


        [HttpPost]
        [Route("GetNamXBMax_APP")]
        public async Task<IActionResult> GetNamXBMax_APP()
        {
            try
            {
                // Gọi API POST để lấy dữ liệu từ controller API
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/NhapSach/GetNamXBMax_API", null);

                Debug.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung trả về từ API
                    string data = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response Data: {data}"); // Log dữ liệu trả về từ API

                    // Deserialize giá trị năm XB Max (int)
                    int namXBMax = JsonConvert.DeserializeObject<int>(data);

                    return Ok(new { success = true, namXBMax });
                }
                else
                {
                    Debug.WriteLine("Failed to retrieve NamXBMax.");
                    return BadRequest(new { success = false, message = "Failed to retrieve NamXBMax from API." });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred: {ex.Message}");
                return BadRequest(new { success = false, message = "Error occurred while fetching NamXBMax." });
            }
        }

        [HttpPost]
        [Route("GetNCC_APP")]
        public async Task<IActionResult> GetNCC_APP(int mancc)
        {
            try
            {
                // Gọi API GetListNCC_API thông qua HttpClient
                var requestData = new { mancc = mancc }; // Đóng gói tham số mancc vào một đối tượng

                var reqjson = JsonConvert.SerializeObject(requestData);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                // Gọi API GetListNCC_API từ controller khác
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/NhapSach/GetListNCC_API", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<NhaCungCap>(data);

                    // Nếu gọi API thành công và có dữ liệu, trả về thông tin nhà cung cấp
                    return Ok(new { success = true, ncc = result });
                }
                else
                {
                    // Nếu API trả về lỗi
                    return BadRequest(new { success = false, message = "Failed to retrieve data from API." });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi trong quá trình gọi API
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ImportExcel")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "File không hợp lệ hoặc bị trống." });
            }

            // Kiểm tra định dạng tệp
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { Message = "Chỉ chấp nhận file Excel với định dạng .xlsx hoặc .xls." });
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    // Sao chép nội dung của file vào bộ nhớ
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Tạo nội dung gửi qua MultipartFormDataContent
                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(stream), "file", file.FileName);

                    // Gửi yêu cầu POST tới API
                    HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/NhapSach/ImportExcel", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        // Kiểm tra nếu dữ liệu trả về có chứa trường Message
                        if (string.IsNullOrEmpty(responseData))
                        {
                            return StatusCode(500, new { message = "Dữ liệu trả về từ API không hợp lệ." });
                        }

                        try
                        {
                            // Deserialize dữ liệu từ JSON trả về
                            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                            // Kiểm tra sự tồn tại và tính hợp lệ của dữ liệu trả về
                            if (result["message"] == null ||
                                result["successfulRecords"] == null ||
                                result["errorRecords"] == null ||
                                !(result["successfulRecords"] is JArray successfulRecordsArray) ||
                                !(result["errorRecords"] is JArray errorRecordsArray))
                            {
                                return StatusCode(500, new { Message = "Dữ liệu trả về từ API không hợp lệ hoặc không đầy đủ." });
                            }

                            // Lấy dữ liệu từ JSON
                            string Message = result["message"].ToString();
                            var successfulRecords = successfulRecordsArray.ToObject<List<ImportSachTemp>>();
                            var errorRecords = errorRecordsArray.ToObject<List<ImportSachTemp>>();

                            // Trả về dữ liệu JSON cho modal
                            return Json(new
                            {
                                message = Message,
                                successfulRecords = successfulRecords,
                                errorRecords = errorRecords
                            });
                        }
                        catch (JsonException jsonEx)
                        {
                            // Xử lý lỗi khi deserializing JSON
                            return StatusCode(500, new { Message = $"Lỗi trong quá trình phân tích dữ liệu JSON: {jsonEx.Message}" });
                        }
                        catch (Exception ex)
                        {
                            // Xử lý các lỗi chung khác
                            return StatusCode(500, new { Message = $"Có lỗi xảy ra trong quá trình xử lý: {ex.Message}" });
                        }

                    }

                    // Xử lý lỗi từ API
                    return StatusCode((int)response.StatusCode, new { Message = $"Lỗi từ API: {response.ReasonPhrase}" });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return StatusCode(500, new { Message = $"Lỗi xử lý file: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("PhieuNhap_Excel_APP")]
        public async Task<IActionResult> PhieuNhap_Excel_APP([FromBody] DTO_Tao_Phieu_Nhap_Excel dto)
        {
            try
            {
                if (dto == null || dto.idThongTin == null || dto.idThongTin.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ hoặc danh sách ID rỗng." });
                }

                var reqjson = JsonConvert.SerializeObject(dto);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Nhapsach/PhieuNhap_Excel_API", httpContent);
                

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { success = true, message = "Phiếu nhập đã được tạo thành công." });
                }
                else
                {
                    // Nếu API trả lỗi, trả về lỗi đó
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = $"Lỗi từ API: {response.ReasonPhrase}"
                    });
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về lỗi
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi gọi API PhieuNhap_Excel_API.",
                    error = ex.Message
                });
            }
        }
    }

}

