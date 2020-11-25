using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHome;
using SmartHome.Data;

namespace SmartHome.Controllers
{
    public class DeviceImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeviceImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeviceImages
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.DeviceImages.ToListAsync());
        }

        // GET: DeviceImages/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deviceImage = await _context.DeviceImages
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (deviceImage == null)
            {
                return NotFound();
            }

            return View(deviceImage);
        }

        // GET: DeviceImages/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeviceImages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ImageId,Image")] DeviceImage deviceImage, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                if(Image?.Length > 0)
                {
                    byte[] uploadedImage = null;
                    using (var ms = new MemoryStream())
                    {
                        Image.CopyTo(ms);
                        uploadedImage = ms.ToArray();
                    }
                    //deviceImage.Device.Images.Add(new DeviceImage() { Device = device, Image = uploadedImage });
                    _context.Add(deviceImage);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(deviceImage);
        }

        // GET: DeviceImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deviceImage = await _context.DeviceImages.FindAsync(id);
            if (deviceImage == null)
            {
                return NotFound();
            }
            return View(deviceImage);
        }

        // POST: DeviceImages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ImageId,Image")] DeviceImage deviceImage)
        {
            if (id != deviceImage.ImageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deviceImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeviceImageExists(deviceImage.ImageId))
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
            return View(deviceImage);
        }

        // GET: DeviceImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deviceImage = await _context.DeviceImages
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (deviceImage == null)
            {
                return NotFound();
            }

            return View(deviceImage);
        }

        // POST: DeviceImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deviceImage = await _context.DeviceImages.FindAsync(id);
            _context.DeviceImages.Remove(deviceImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeviceImageExists(int id)
        {
            return _context.DeviceImages.Any(e => e.ImageId == id);
        }
    }
}
