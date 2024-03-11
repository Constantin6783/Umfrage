using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using PollTool.Server.Models;
using PollTool.Server.Models.Api;
using PollTool.Server.Models.Requests;
using PollTool.Server.Models.Response;
using ApiPoll = PollTool.Server.Models.Api.Poll;
using DbPoll = PollTool.Server.Models.Poll;
using ApiQuestion = PollTool.Server.Models.Api.Question;
using DbQuestion = PollTool.Server.Models.Question;
using ApiAnswer = PollTool.Server.Models.Api.Answer;
using DbAnswer = PollTool.Server.Models.Answer;
namespace PollTool.Server.Controllers
{
    [Route("api/[controller]/[action]")]
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

            
            var response = new CreatePollResponse();


            try
            {
                if (_context.Polls.Any(p => p.Title == request.Title)) return new CreatePollResponse { ErrorMessage = $"Umfrage '{request.Title}' existiert bereits!" };
                var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                var poll = _context.Polls.Add(new DbPoll
                {
                    Title = request.Title,
                    Description = request.Description,
                    CreatedBy = ip
                }).Entity;
                await _context.SaveChangesAsync();

                var question = _context.Questions.Add(new DbQuestion
                {
                     PollId = poll.PollId,
                      Title = request.Title,
                }).Entity;

                await _context.SaveChangesAsync();

                _context.Answers.AddRange(request.Answers.Select(a => new DbAnswer { QuestionId = question.QuestionId, Text = a.Text }));

                await _context.SaveChangesAsync();
                response.Success = true;
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
