//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
//{
//    [Authorize(Roles = "SuperAdmin,Admin")]
//    public class FinancialController : BaseController
//    {
//        private readonly IFinancialService _financialService;

//        public FinancialController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
//                                  IFinancialService financialService, ILogger<FinancialController> logger)
//            : base(context, userManager, logger)
//        {
//            _financialService = financialService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var model = new FinancialDashboardViewModel
//            {
//                TotalRevenue = await _context.Payments
//                    .Where(p => p.Status == PaymentStatus.Paid)
//                    .SumAsync(p => p.Amount),
//                PendingPayments = await _context.Payments
//                    .Where(p => p.Status == PaymentStatus.Pending)
//                    .SumAsync(p => p.Amount),
//                MonthlyRevenue = await _financialService.GetMonthlyRevenueAsync(),
//                RecentPayments = await _context.Payments
//                    .Include(p => p.Student.User)
//                    .OrderByDescending(p => p.PaidDate ?? p.DueDate)
//                    .Take(10)
//                    .ToListAsync(),
//                OverduePayments = await _context.Payments
//                    .Include(p => p.Student.User)
//                    .Where(p => p.Status == PaymentStatus.Overdue)
//                    .ToListAsync()
//            };

//            return View(model);
//        }

//        public async Task<IActionResult> Payments(int page = 1, PaymentStatus? status = null)
//        {
//            var query = _context.Payments
//                .Include(p => p.Student.User)
//                .AsQueryable();

//            if (status.HasValue)
//            {
//                query = query.Where(p => p.Status == status.Value);
//            }

//            var payments = await query
//                .OrderByDescending(p => p.DueDate)
//                .Skip((page - 1) * 20)
//                .Take(20)
//                .ToListAsync();

//            var model = new PaymentListViewModel
//            {
//                Payments = payments,
//                CurrentPage = page,
//                TotalPages = (int)Math.Ceiling(await query.CountAsync() / 20.0),
//                SelectedStatus = status
//            };

//            return View(model);
//        }


//        public async Task<IActionResult> Salaries()
//        {
//            var salaries = await _context.Salaries
//                .Include(s => s.Teacher.User)
//                .OrderByDescending(s => s.Year)
//                .ThenByDescending(s => s.Month)
//                .ToListAsync();

//            return View(salaries);
//        }

//        [HttpPost]
//        public async Task<IActionResult> CalculateMonthlySalaries(int month, int year)
//        {
//            await _financialService.CalculateMonthlySalariesAsync(month, year);
//            return RedirectToAction(nameof(Salaries));
//        }

//        public async Task<IActionResult> SalaryDetails(int id)
//        {
//            var salary = await _context.Salaries
//                .Include(s => s.Teacher.User)
//                .FirstOrDefaultAsync(s => s.id == id);
//            if (salary == null) return NotFound();
//            return View(salary);
//        }
//    }
//}
