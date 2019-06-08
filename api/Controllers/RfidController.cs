using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Constants;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RfidController : ControllerBase
    {
        private FactoryContext _context;

        public RfidController(FactoryContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]RfidTagViewModel model)
        {
            if(string.IsNullOrWhiteSpace(model.Tag))
                return BadRequest("Tag cannot be empty.");

            var rfid = await _context.Rfids.FirstOrDefaultAsync(r => r.Tag == model.Tag);
            if (rfid != null)
                return Ok();

            _context.Rfids.Add(new Rfid{ Tag = model.Tag });
            if (0 < await _context.SaveChangesAsync())
                return Ok();
            
            return BadRequest("Couldn't create tag and I think it's something you did.");
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromBody]RfidTagViewModel model)
        {
            if(string.IsNullOrWhiteSpace(model.Tag))
                return BadRequest("Tag cannot be empty.");

            var rfid = await _context.Rfids.FirstOrDefaultAsync(r => r.Tag == model.Tag);
            if (rfid == null)
                return Ok();
            
            const string sql = "delete from RfidEvents where Rfid = @p0; delete from Rfids where id = @p0;";
            var result = await _context.Database.ExecuteSqlCommandAsync(sql, rfid.Id);
            if (result > 0)
                return Ok();
            
            return BadRequest("Again, something went wrong and I think you did it.");
        }
    }
}