using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
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
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly PollContext _context;
        private readonly ILogger _logger;

        private static List<ApiPoll> _debugPolls = new List<ApiPoll>
        {
            new ApiPoll { Description  ="Tolle Umfrage 1", PollId = 1, Title = "Umfrage 1", DoneByUser = true, OwnedByUser = false},
            new ApiPoll { Description  ="Tolle Umfrage 2", PollId = 2, Title = "Umfrage 2", DoneByUser = true, OwnedByUser = false},
            new ApiPoll { Description  ="Tolle Umfrage 3", PollId = 3, Title = "Umfrage 3", DoneByUser = false, OwnedByUser = true} ,
        };

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

            response.Polls = _debugPolls;

            response.Success = true;
            return response;
            try
            {
                //var uid = HttpContext.Connection.Id
                var uid = HttpContext.Connection.RemoteIpAddress.Address;
                var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                var polls = await _context.Polls.Select(p => new ApiPoll
                {
                    Title = p.Title,
                    Description = p.Description,
                    PollId = p.PollId,
                    DoneByUser = _context.Questions.Where(q => q.PollId == p.PollId)
                    .Any(q => _context.UserAnswers.Any(ua => ua.QuestionId == q.QuestionId && ua.UserAnswerId == uid)),
                    OwnedByUser = p.CreatedBy == ip
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

        // POST: api/CreatePoll
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CreatePollResponse>> CreatePoll([FromBody] CreatePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();

            if (_debugPolls.Any(p => p.Title == request.Title)) return new CreatePollResponse { ErrorMessage = $"Umfrage '{request.Title}' existiert bereits!" };
            var response = new CreatePollResponse();
            _debugPolls.Add(new ApiPoll { Title = request.Title, Description = request.Description, DoneByUser = false, OwnedByUser = true, PollId = _debugPolls.Max(p => p.PollId) + 1});
            response.Success = true;
            return response;


            try
            {
                if (_context.Polls.Any(p => p.Title == request.Title)) return new CreatePollResponse { ErrorMessage = $"Umfrage '{request.Title}' existiert bereits!" };
                _context.Polls.Add(new DbPoll
                {
                    Title = request.Title,
                    Description = request.Description,
                    CreatedBy = HttpContext.ToString()
                });




            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreatePoll));
                response.ErrorMessage = "Critical error, please contact the administrator";
            }



            return response;
        }

        // POST: api/Polls/DeletePoll
        [HttpPost]
        public async Task<ActionResult<BaseResponse>> DeletePoll([FromBody] DeletePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();

            if (_debugPolls.FirstOrDefault(p => p.PollId == request.PollID) is ApiPoll pollToDelete)
            {                
                _debugPolls.Remove(pollToDelete);
                return new BaseResponse { Success = true };

            }
            return new BaseResponse { ErrorMessage = "Poll not found!" };
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            if (await _context.Polls.FindAsync(request.PollID) is DbPoll foundPoll)
            {
                if (foundPoll.CreatedBy != ip) return Unauthorized();

                _context.Polls.Remove(foundPoll);
                await _context.SaveChangesAsync();
                return new BaseResponse { Success = true };

            }
            return new BaseResponse { ErrorMessage = "Poll not found!" };

        }
    }
}
