using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Predobro.Controllers;
using Predobro.Data;
using Predobro.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Store")]
public class StoreController : BaseController
{
    private readonly FoodContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StoreController(FoodContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // List items for this store
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var items = await _context.Items
            .Where(i => i.StoreId == user.Id)
            .ToListAsync();
        return View(items);
    }


    public async Task<IActionResult> MyStore()
    {
        // This is now just a wrapper for Index
        // You could customize this view in the future
        return await Index();
    }

    // GET: Create new item
    public IActionResult Create()
    {
        return View(new Item()); // Initialize with empty item
    }

    // POST: Create new item
    // Explicitly include ONLY the fields we want to bind from form
    
    
    // Add these methods to your existing StoreController

// GET: Store/Edit/5
public async Task<IActionResult> Edit(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.StoreId == user.Id);
    
    if (item == null)
    {
        return NotFound();
    }
    
    return View(item);
}

// Update the Create action to handle image upload
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Name,Description,Price,Quantity")] Item item, IFormFile ImageFile)
{
    Console.WriteLine("POST Create called");

    var user = await _userManager.GetUserAsync(User);

    // Remove the validation errors for StoreId and Store
    ModelState.Remove("StoreId");
    ModelState.Remove("Store");

    if (ModelState.IsValid)
    {
        try
        {
            // Set these fields after validation
            item.StoreId = user.Id;

            // Handle image upload if provided
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Create a unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "items", fileName);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                
                // Store the relative URL
                item.ImageUrl = "/uploads/items/" + fileName;
            }

            // DEBUG: Print item details to console
            Console.WriteLine($"Adding item: {item.Name}, Price: {item.Price}, StoreId: {item.StoreId}");
            
            _context.Items.Add(item);
            var result = await _context.SaveChangesAsync();
            
            Console.WriteLine($"SaveChanges result: {result} records affected");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            ModelState.AddModelError("", "Error saving item: " + ex.Message);
            // Return the view with the error
            return View(item);
        }
    }
    else
    {
        Console.WriteLine("ModelState is invalid:");
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"- {state.Key}: {error.ErrorMessage}");
            }
        }
    }
    
    // If we get here, something went wrong, redisplay the form
    return View(item);
}

// Update the Edit action to handle image upload/deletion
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,ImageUrl")] Item item, 
    IFormFile ImageFile, bool DeleteImage = false)
{
    if (id != item.Id)
    {
        return NotFound();
    }

    var user = await _userManager.GetUserAsync(User);
    
    // Make sure the item belongs to this store
    var originalItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.StoreId == user.Id);
    if (originalItem == null)
    {
        return NotFound();
    }

    // Remove validation errors for StoreId and Store
    ModelState.Remove("StoreId");
    ModelState.Remove("Store");
    
    if (ModelState.IsValid)
    {
        try
        {
            // Preserve the StoreId (don't let it be changed)
            item.StoreId = originalItem.StoreId;
            
            // Handle image changes
            if (DeleteImage && !string.IsNullOrEmpty(originalItem.ImageUrl))
            {
                // Delete the physical file if it exists
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                    originalItem.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                
                // Clear the URL
                item.ImageUrl = null;
            }
            else if (!DeleteImage && ImageFile != null && ImageFile.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(originalItem.ImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                        originalItem.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                
                // Upload new image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "items", fileName);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                
                // Update the URL
                item.ImageUrl = "/uploads/items/" + fileName;
            }
            else
            {
                // Keep the existing image URL
                item.ImageUrl = originalItem.ImageUrl;
            }
            
            _context.Entry(originalItem).State = EntityState.Detached;
            _context.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            ModelState.AddModelError("", "Error updating item: " + ex.Message);
        }
    }
    return View(item);
}

// GET: Store/Delete/5
public async Task<IActionResult> Delete(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.StoreId == user.Id);
    
    if (item == null)
    {
        return NotFound();
    }
    
    return View(item);
}

// POST: Store/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.StoreId == user.Id);
    
    if (item == null)
    {
        return NotFound();
    }
    
    _context.Items.Remove(item);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
}