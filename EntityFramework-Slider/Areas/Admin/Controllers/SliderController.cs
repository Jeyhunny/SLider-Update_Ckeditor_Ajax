﻿using EntityFramework_Slider.Areas.Admin.ViewModels;
using EntityFramework_Slider.Data;
using EntityFramework_Slider.Helpers;
using EntityFramework_Slider.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFramework_Slider.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;   // uploaddan qabaq wwwroot a chatmaq uchun bu interface den istifade edib chatiriq
        
        public SliderController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Slider> sliders = await _context.Sliders.ToListAsync();
            return View(sliders);
        }


        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();
           

            Slider slider = await _context.Sliders.FirstOrDefaultAsync(m => m.Id == id);

           
            if (slider is null) return NotFound();
 
            return View(slider);
        }




        
        [HttpGet]
        public IActionResult Create()    
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderCreateVM slider)
        {
            try
            {
                if (!ModelState.IsValid)  
                {
                    return View(slider);
                }

                foreach (var photo in slider.Photos)
                {
                    if (!photo.CheckFileType("image/")) 
                    {
                        ModelState.AddModelError("Photo", "File type must be image");
                        return View();
                    }

                    if (!photo.CheckFileSize(200)) 
                    {
                        ModelState.AddModelError("Photo", "Image size must be max 200kb");
                        return View();
                    }
                }


                foreach (var photo in slider.Photos)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + photo.FileName;  

                    string path = FileHelper.GetFilePath(_env.WebRootPath, "img", fileName);  

                    await FileHelper.SaveFileAsync(path, photo);

                    Slider newSlider = new()  
                    {
                        Image = fileName
                    };

                    await _context.Sliders.AddAsync(newSlider); 

                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            catch (Exception)
            {

                throw;
            }

        }




   





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                Slider slider = await _context.Sliders.FirstOrDefaultAsync(m => m.Id == id);

                if (slider is null) return NotFound();


                
                string path = FileHelper.GetFilePath(_env.WebRootPath, "img", slider.Image);   // path vasitesi ile roota chatiriq. yeni proyektimizde yuklenen wekli hara qoyacayiqsa ora chatmaq uchun

                //if (System.IO.File.Exists(path))
                //{
                //    System.IO.File.Delete(path);
                //}

                FileHelper.DeleteFile(path);  

                _context.Sliders.Remove(slider);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                throw;
            }
        }






        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
           
                if (id is null) return BadRequest();

                Slider slider = await _context.Sliders.FirstOrDefaultAsync(m => m.Id == id);

                if (slider is null) return NotFound();

                return View(slider);

          
        }


        





        
        [HttpPost]    
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Slider slider)
        {
            try
            {
                if (id == null) return BadRequest();
                Slider dbSlider = await _context.Sliders.FirstOrDefaultAsync(m => m.Id == id);
                if (dbSlider is null) return NotFound();

                if (slider.Photo == null) 

                {
                    return RedirectToAction(nameof(Index));
                }

                if (!slider.Photo.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Photo", "Please choose correct image type");
                    return View(dbSlider);
                }

                if (!slider.Photo.CheckFileSize(200))
                {
                    ModelState.AddModelError("Photo", "Image size must be max 200kb");
                    return View(dbSlider);
                }

               


                string oldPath = FileHelper.GetFilePath(_env.WebRootPath, "img", dbSlider.Image); 
                FileHelper.DeleteFile(oldPath);   

                string fileName = Guid.NewGuid().ToString() + "_" + slider.Photo.FileName;

                string newPath = FileHelper.GetFilePath(_env.WebRootPath, "img", fileName); 

                //using (FileStream stream = new FileStream(newPath, FileMode.Create))     
                //{
                //    await slider.Photo.CopyToAsync(stream);
                //}

                await FileHelper.SaveFileAsync(newPath, slider.Photo);


                dbSlider.Image = fileName; 

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return View();
            }
        
        }



        [HttpPost]
        public async Task<IActionResult> SetStatus(int? id)
        {
            if (id is null) return BadRequest();

            Slider slider = await _context.Sliders.FirstOrDefaultAsync(m => m.Id == id);

            if (slider == null) return NotFound();

           

            slider.SoftDelete = !slider.SoftDelete;  

            await _context.SaveChangesAsync();

            return Ok(slider.SoftDelete);
        }


    }





   
   

}
