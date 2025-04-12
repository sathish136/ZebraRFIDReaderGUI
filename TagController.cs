using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ZebraRFIDReaderGUI
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private static Form1 mainForm;

        public static void Initialize(Form1 form)
        {
            mainForm = form;
        }

        [HttpGet]
        [Route("tags")]
        public ActionResult<IEnumerable<object>> GetTags()
        {
            try
            {
                var tags = mainForm.GetAllTags()
                    .Select(tag => new { tagID = tag.TagID });
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
