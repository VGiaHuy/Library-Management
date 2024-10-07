﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using WebAPI.DTOs;
using WebAPI.Helper;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly QuanLyThuVienContext _context;
        private readonly IMapper _mapper;

        public BookController(IMapper mapper, QuanLyThuVienContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Book
        [HttpGet]
        public async Task<IActionResult> GetAllBook()
        {
            try
            {
                var sachList = await _context.Saches.Include(x => x.TtSaches).ToListAsync();

                List<SachDTO> sachDtos = _mapper.Map<List<SachDTO>>(sachList);

                return Ok(sachDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Book/name
        [HttpGet("{tenSach}")]
        public IActionResult GetBookByName(string tenSach)
        {
            try
            {
               
                if (string.IsNullOrEmpty(tenSach))
                {
                    return BadRequest(new { success = false, message = "Tên sách không được để trống." });
                }
                
                var sachLoc = _context.Saches.Include(x => x.TtSaches).Where(item =>
                    item.TenSach.Contains(tenSach) ||
                    item.TheLoai.Contains(tenSach) ||
                    item.TacGia.Contains(tenSach) ||
                    item.NgonNgu.Contains(tenSach) ||
                    item.Nxb.Contains(tenSach)
                ).ToList();

                List<SachDTO> sachDtos = _mapper.Map<List<SachDTO>>(sachLoc);

                if (sachDtos.Any())
                {
                    return Ok(new { success = true, sachList = sachDtos });
                }
                else
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sách nào phù hợp." });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi tìm kiếm sách." });
            }
        }


        // GET: api/Book/{ngonNgu}/{theLoai}/{namXB}
        [HttpGet("{ngonNgu}/{theLoai}/{namXB}")]
        public IActionResult GetBookByCategory(string ngonNgu, string theLoai, string namXB)
        {
            try
            {
                var sachLoc = _context.Saches.Include(x => x.TtSaches).AsQueryable();

                if (ngonNgu != "All")
                {
                    sachLoc = sachLoc.Where(m => m.NgonNgu == ngonNgu);
                }

                if (theLoai != "All")
                {
                    sachLoc = sachLoc.Where(m => m.TheLoai == theLoai);
                }

                if (namXB != "All")
                {
                    int namXBValue = int.Parse(namXB);
                    sachLoc = sachLoc.Where(m => m.NamXb == namXBValue);
                }

                List<SachDTO> sachDtos = _mapper.Map<List<SachDTO>>(sachLoc);

                if (sachDtos.Any())
                {
                    return Ok(new { success = true, sachList = sachDtos });
                }
                else
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sách hoặc sách không tồn tại." });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi lọc sách." });
            }
        }


        /*// PUT: api/Book/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Sach sach)
        {
            if (id != sach.MaSach)
            {
                return BadRequest();
            }

            _context.Entry(sach).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
*/
        /*// POST: api/Book
        [HttpPost]
        public async Task<ActionResult<Sach>> PostBook(Sach sach)
        {
            _context.Saches.Add(sach);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSach", new { id = sach.MaSach }, sach);
        }*/

        /*// DELETE: api/Book/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var sach = await _context.Saches.FindAsync(id);
            if (sach == null)
            {
                return NotFound();
            }

            _context.Saches.Remove(sach);
            await _context.SaveChangesAsync();

            return NoContent();
        }
*/
       /* private bool BookExists(int id)
        {
            return _context.Saches.Any(e => e.MaSach == id);
        }*/
    }
}
