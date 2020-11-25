using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SmartHome.Data;
using SmartHome.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using SmartHome.Classes;
using System;
using System.Drawing.Drawing2D;

namespace SmartHome.Controllers
{
    public class DevicesController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DevicesController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Devices
        public async Task<IActionResult> Index(string sortBy, string currentFilter, string searchString, int? pageNumber)
        {
            //The three sort methods: by name, by type, or by user.
            ViewData["NameSort"] = string.IsNullOrEmpty(sortBy) ? "name_desc" : "";
            ViewData["TypeSort"] = sortBy == "Type" ? "type_desc" : "Type";
            ViewData["UserSort"] = sortBy == "Username" ? "user_desc" : "Username";

            //Go back to the first page if we're searching; save the search either way
            if (searchString != null)
                pageNumber = 1;
            else
                currentFilter = searchString;
            ViewData["CurrentFilter"] = searchString;

            IEnumerable<Device> devices;
            if (!string.IsNullOrEmpty(searchString))
            {
                //Since we have a search string, use it!
                devices = await _context.Devices
                    .Include(x => x.DeviceType)
                    .Include(x => x.Images)
                    .Include(x => x.AddedBy)
                    .Where(x => x.DeviceName.Contains(searchString))
                    .ToListAsync();
            }
            else
            {
                //Otherwise, display everything just as it comes out of the database
                devices = await _context.Devices
                    .Include(x => x.DeviceType)
                    .Include(x => x.Images)
                    .Include(x => x.AddedBy)
                    .ToListAsync();
            }

            //Surely there's a better way to do this than a switch
            switch (sortBy)
            {
                case "name_desc":
                    devices = devices.OrderByDescending(x => x.DeviceName);
                    break;
                case "Type":
                    devices = devices.OrderBy(x => x.DeviceType.TypeName);
                    break;
                case "type_desc":
                    devices = devices.OrderByDescending(x => x.DeviceType.TypeName);
                    break;
                case "User":
                    devices = devices.OrderByDescending(x => x.AddedBy.UserName);
                    break;
                case "user_desc":
                    devices = devices.OrderByDescending(x => x.AddedBy.UserName);
                    break;
                default:
                    devices = devices.OrderBy(x => x.DeviceName);
                    break;
            }

            //Currently, pages are hard-set to 5. This would make a lovely setting somewhere.
            int pageSize = 5;
            return View(Pagination<Device>.CreateAsync(devices, pageNumber ?? 1, pageSize));
        }

        // GET: Devices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Include(x => x.DeviceType)
                .Include(x => x.Images)
                .Include(x => x.AddedBy)
                .FirstOrDefaultAsync(m => m.DeviceId == id);
            if (device == null)
            {
                return NotFound();
            }
            ViewBag.Thumbs = device.Images.ConvertAll(x => new KeyValuePair<int, string>(x.ImageId, x.ThumbString));
            return View(device);
        }

        // GET: Devices/Create
        public IActionResult Create()
        {
            ViewBag.DeviceTypes = _context
                .DeviceTypes
                .Select(x => new SelectListItem(x.TypeName, x.TypeId.ToString()))
                .ToList();
            return View();
        }

        // POST: Devices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeviceName,Description,DeviceTypeTypeId")] DevicesViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Device device = new Device()
                {
                    DeviceName = vm.DeviceName,
                    Description = vm.Description,
                    DeviceType = _context.DeviceTypes.First(x => x.TypeId == int.Parse(vm.DeviceTypeTypeId)),
                    AddedBy = _context.Users.Find(_userManager.GetUserId(User))
                };
                _context.Add(device);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.DeviceTypes = _context
                .DeviceTypes
                .Select(x => new SelectListItem(x.TypeName, x.TypeId.ToString()))
                .ToList();
            return View();
        }

        // GET: Devices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Include(x => x.DeviceType)
                .Include(x => x.AddedBy)
                .Where(x => x.DeviceId == id)
                .FirstAsync();

            if (device == null)
            {
                return NotFound();
            }
            List<SelectListItem> lst = _context
                .DeviceTypes
                .Select(x => new SelectListItem(x.TypeName, x.TypeId.ToString()))
                .ToList();
            ViewBag.DeviceTypes = lst;
            return View(new DevicesViewModel() {
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                Description = device.Description,
                DeviceTypeTypeId = device.DeviceTypeTypeId.ToString()
            });
        }

        // POST: Devices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeviceId,DeviceName,Description,DeviceTypeTypeId")] DevicesViewModel vm)
        {
            if (id != vm.DeviceId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var device = new Device()
                {
                    DeviceId = vm.DeviceId,
                    DeviceName = vm.DeviceName,
                    Description = vm.Description,
                    DeviceType = _context.DeviceTypes.First(x => x.TypeId == int.Parse(vm.DeviceTypeTypeId)),
                    AddedBy = _context.Users.Find(_userManager.GetUserId(User))
                };

                try
                {
                    _context.Update(device);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeviceExists(device.DeviceId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var lst = _context
                .DeviceTypes
                .Select(x => new SelectListItem(x.TypeName, x.TypeId.ToString()))
                .ToList();
            ViewBag.DeviceTypes = lst;
            return View(new DevicesViewModel()
            {
                DeviceId = vm.DeviceId,
                DeviceName = vm.DeviceName,
                Description = vm.Description,
                DeviceTypeTypeId = vm.DeviceTypeTypeId.ToString()
            });
        }

        // GET: Devices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Include(x => x.DeviceType)
                .Include(x => x.Images)
                .Include(x => x.AddedBy)
                .FirstOrDefaultAsync(m => m.DeviceId == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        // POST: Devices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Devices/AddImage/5
        public async Task<IActionResult> AddImage(int? id)
        {
            if (id == null)
                return NotFound();

            var device = await _context.Devices
                .Where(x => x.DeviceId == id)
                .Include(x => x.DeviceType)
                .Include(x => x.AddedBy)
                .FirstAsync();

            if (device == null)
                return NotFound();
            var vm = new ImageViewModel()
            {
                DeviceId = device.DeviceId
            };
            ViewData["DeviceName"] = device.DeviceName + " (" + device.DeviceType.TypeName + ")";
            return View(vm);
        }

        // POST: Devices/AddImage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(int id, ImageViewModel vm)
        {
            if (id != vm.DeviceId)
                return NotFound();

            //This list will hold the images being uploaded
            var lst = new List<DeviceImage>();
            var device = await _context.Devices.FindAsync(id);

            //Loop through every image
            foreach (IFormFile file in vm.Files)
            {
                try
                {
                    //The image object, just add images
                    DeviceImage img = new DeviceImage()
                    {
                        Device = device,
                        ContentType = file.ContentType,
                        Description = file.FileName
                    };

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);

                        //The normal image is easy, since we're keeping the original
                        var image = Image.FromStream(ms);
                        img.Image = ms.ToArray();

                        //Set the width and height for the thumbnails with the proper ratio
                        var rect = new Rectangle(0, 0, 100, 100);
                        if (image.Width > image.Height)
                        {
                            rect.Height = 100 * image.Height / image.Width;
                            rect.Y = 50 - rect.Height / 2;
                        }
                        else
                        {
                            rect.Width = 100 * image.Width / image.Height;
                            rect.X = 50 - rect.Width / 2;
                        }

                        var thumb = new Bitmap(100, 100);
                        thumb.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                        using (var g = Graphics.FromImage(thumb))
                        {
                            //These setting will help the image resize nicely
                            g.CompositingMode = CompositingMode.SourceCopy;
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            //Draw background, just a 100x100px square
                            g.FillRectangle(Brushes.White, 0, 0, 100, 100);

                            //Add the image itself, centered and resized
                            using (var wrapMode = new ImageAttributes())
                            {
                                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                            }
                        }

                        //Save the resulting thumbnail as a jpg
                        using (var stream = new MemoryStream())
                        {
                            thumb.Save(stream, ImageFormat.Jpeg);
                            stream.Position = 0;
                            img.Thumbnail = stream.ToArray();
                        }
                    }
                    lst.Add(img);
                } catch
                {
                    //Whatever the issue was, just don't store the image
                }
            }
            //Save the image(s) to the database, presuming there's anything to save
            if (lst.Count > 0)
            {
                await _context.AddRangeAsync(lst);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewImage(int? id)
        {
            if (id == null)
                return NotFound();

            var img = await _context.DeviceImages
                .Include(x => x.Device)
                .FirstAsync(x => x.ImageId == id);
            if (img == null)
                return NotFound();
            return View(img);
        }

        // GET: Devices/DeleteImage/5
        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
                return NotFound();

            var img = await _context.DeviceImages.FirstAsync(m => m.ImageId == id);
            if (img == null)
                return NotFound();

            return View(img);
        }

        // POST: Devices/DeleteImage/5
        [HttpPost, ActionName("DeleteImage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImageConfirmed(int id)
        {
            var img = await _context
                .DeviceImages
                .Include(x => x.Device)
                .Where(x => x.ImageId == id)
                .FirstAsync();
            var DeviceId = img.Device.DeviceId;
            _context.DeviceImages.Remove(img);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details",new { id = DeviceId });
        }

        private bool DeviceExists(int id)
        {
            return _context.Devices.Any(e => e.DeviceId == id);
        }
    }
}
