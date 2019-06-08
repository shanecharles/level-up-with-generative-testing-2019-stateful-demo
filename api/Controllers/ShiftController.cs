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
    public class ShiftController : ControllerBase
    {
        private const string Break = "Break";
        // Changed name from Shift
        private const string Floor = "Floor";

        private DateTime _now = DateTime.UtcNow;

        private readonly FactoryContext _context;

        public ShiftController(FactoryContext context)
        {
            _context = context;
        }
        

        [HttpGet]
        [Route("status/{tag}")]
        public async Task<ActionResult<StatusResponse>> Status(string tag)
        {
            var rfid = await _context.Rfids.FirstOrDefaultAsync(r => r.Tag == tag);

            if (rfid == null)
                return NotFound();
            
            var openEvents = await _context.RfidEvents.Where(r => 
                        r.Rfid == rfid.Id
                        && r.StartUtc <= _now
                        && r.EndUtc == null)
                    .ToListAsync();

            if (!openEvents.Any())
                return new StatusResponse{Status="OffFloor", Actions=new []{ShiftAction.StartFloor}};
            
            if (openEvents.Any(e => e.EventType == Break))
                return new StatusResponse{Status="OnBreak", Actions=new []{ShiftAction.EndBreak, ShiftAction.EndFloor}};
            
            return new StatusResponse{Status="OnFloor", Actions=new []{ShiftAction.StartBreak, ShiftAction.EndFloor}};
        }



        [HttpPost]
        public async Task<ActionResult> Post([FromBody]ActionRequest actionRequest)
        {
            var action = actionRequest.Action;
            var tag = actionRequest.Tag;
            if(!ValidAction(action))
                return BadRequest($"Invalid action: {action}");

            var rfid = await _context.Rfids.FirstOrDefaultAsync(r => r.Tag == tag);
            if(rfid == null)
                return NotFound($"RFID tag '{tag}' not registered.");

            if (action.Equals(ShiftAction.StartFloor, StringComparison.CurrentCultureIgnoreCase))
                await CreateRfidEvent(rfid.Id, Floor);
            
            else if (action.Equals(ShiftAction.StartBreak, StringComparison.CurrentCultureIgnoreCase))
            {
                var rfidEvent = await _context.RfidEvents.FirstOrDefaultAsync(re =>
                        re.Rfid == rfid.Id 
                        && re.EventType == Floor 
                        && re.StartUtc <= _now
                        && re.EndUtc == null);

                if (rfidEvent == null)
                    return BadRequest("Not currently on Floor");

                await GetOrCreateRfidEvent(rfid.Id, Break);
            }
            else if (action.Equals(ShiftAction.EndBreak, StringComparison.CurrentCultureIgnoreCase))
            {
                if (await SetRfidEventEnd(rfid.Id, Break) == null)
                    return BadRequest("Not currently on Break"); 
            }

            else if (action.Equals(ShiftAction.EndFloor, StringComparison.CurrentCultureIgnoreCase))
            {
                if (await SetRfidEventEnd(rfid.Id, Floor) == null)
                    return BadRequest("Not currently on Floor"); 

                await SetRfidEventEnd(rfid.Id, Break);
            }

            return Ok();
        }

        private async Task<RfidEvent> CreateRfidEvent(int rfid, string eventType)
        {
            var r = new RfidEvent
            {
                Rfid = rfid,
                EventType = eventType,
                StartUtc = _now
            };
            _context.RfidEvents.Add(r);
            await _context.SaveChangesAsync();
            return r;
        }

        private async Task<RfidEvent> SetRfidEventEnd(int rfid, string eventType)
        {
            var re = await _context.RfidEvents.FirstOrDefaultAsync(r => r.Rfid == rfid 
                            && r.StartUtc <= _now
                            && r.EventType == eventType
                            && r.EndUtc == null);

            if (re == null)
                return null;    
                
            re.EndUtc = _now;
            await _context.SaveChangesAsync();
            return re;
        }

        private bool ValidAction(string action)
        {
            return ShiftAction.ActionLookup.Contains(action);
        }







        private async Task<RfidEvent> GetOrCreateRfidEvent(int id, string eventType)
        {
            var rfidEvent = await _context.RfidEvents.FirstOrDefaultAsync(re => re.Rfid == id 
                                    && re.EventType == eventType
                                    && re.EndUtc == null);

            return rfidEvent ?? await CreateRfidEvent(id, eventType);
        }
    }
}