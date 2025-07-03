using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    // [Authorize(Roles = "SuperAdmin,Admin")]
    public class RoomManagementController : BaseController
    {
        public RoomManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<RoomManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        // GET: RoomManagement
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.OrderByDescending(r => r.id).ToListAsync();
            return View(rooms);
        }

        // GET: RoomManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoomManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Code,Capacity,Type,Equipment,Location,IsActive,Description")] Room room)
        {
            _logger.LogInformation("Create Room POST method hit.");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid. Attempting to add room.");
                _context.Add(room);
                await _context.SaveChangesAsync();
                await LogActivityAsync("CreateRoom", "Room", room.id.ToString());
                TempData["SuccessMessage"] = "Phòng học đã được tạo thành công!";
                _logger.LogInformation("Room created successfully with ID: {RoomId}", room.id);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogWarning("ModelState is invalid. Errors: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        _logger.LogError("Model State Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
            }
            return View(room);
        }

        // GET: RoomManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: RoomManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Name,Code,Capacity,Type,Equipment,Location,IsActive,Description")] Room room)
        {
            if (id != room.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    await LogActivityAsync("UpdateRoom", "Room", room.id.ToString());
                    TempData["SuccessMessage"] = "Phòng học đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: RoomManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: RoomManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            await LogActivityAsync("DeleteRoom", "Room", id.ToString());
            TempData["SuccessMessage"] = "Phòng học đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.id == id);
        }
    }
} 