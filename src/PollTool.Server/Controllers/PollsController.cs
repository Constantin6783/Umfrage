using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PollTool.Server.Models;
using PollTool.Server.Models.Api;
using PollTool.Server.Models.Requests;
using PollTool.Server.Models.Response;
using ApiPoll = PollTool.Server.Models.Api.Poll;
using DbPoll = PollTool.Server.Models.Poll;

namespace PollTool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly PollContext _context;
        private readonly ILogger _logger;

        public PollsController(PollContext context, ILogger<PollsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Polls
        [HttpPost]
        public async Task<ActionResult<GetPollsResponse>> GetPolls([FromBody] BaseRequest request)
        {
            if (!request.IsValid()) return BadRequest();


            var response = new GetPollsResponse();

            response.Polls = new List<ApiPoll> { new ApiPoll { Description  ="Tolle Umfrage 1", PollId = 1, Title = "Umfrage 1"},
            new ApiPoll { Description  ="Tolle Umfrage 2", PollId = 2, Title = "Umfrage 2"} ,
            new ApiPoll { Description  ="Tolle Umfrage 3", PollId = 3, Title = "Umfrage 3"} ,};

            response.Success = true;
            return response;
            try
            {
                var polls = await _context.Polls.Select(p => new ApiPoll
                {
                    Title = p.Title,
                    Description = p.Description,
                    PollId = p.PollId
                }).ToListAsync();
                response.Polls = polls;
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetPolls));
                response.ErrorMessage = "Critical error, please contact the administrator";
            }


            return response;
        }

        // GET: api/Polls/5
        [HttpPost("{id}")]
        public async Task<ActionResult<GetPollResponse>> GetPoll([FromBody] GetPollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            //var poll = await _context.Polls.FindAsync(id);

            //if (poll == null)
            //{
            //    return NotFound();
            //}

            return new GetPollResponse();
        }

        // PUT: api/Polls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<CreatePollResponse>> CreatePoll([FromBody] CreatePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            return new CreatePollResponse();
        }

        // DELETE: api/Polls/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse>> DeletePoll([FromBody] DeletePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            //TODO: Log Exceptions
            if (await _context.Polls.FindAsync(request.PollID) is DbPoll foundPoll)
            {
                _context.Polls.Remove(foundPoll);
                await _context.SaveChangesAsync();
                return new BaseResponse { Success = true };
            }
            return new BaseResponse { ErrorMessage = "Poll not found!" };

        }
    }
}
